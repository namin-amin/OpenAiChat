
#pragma warning disable SKEXP0001
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

Console.WriteLine("OpenAI Chat with RAG using Semantic Kernel");

// Set your OpenAI API key here
var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "";
var model = "gpt-3.5-turbo"; // Or "gpt-4"

// Create Semantic Kernel builder

var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(modelId: model, apiKey: openAiApiKey);
var embeddingModel = "text-embedding-ada-002"; // Or another OpenAI embedding model
#pragma warning disable SKEXP0001
var embeddingService = new OpenAITextEmbeddingGenerationService(embeddingModel, openAiApiKey);
#pragma warning restore SKEXP0001
var kernel = builder.Build();
#pragma warning disable SKEXP0001
// Set up in-memory RAG
var memory = new MemoryBuilder()
    .WithMemoryStore(new SimpleInMemoryMemoryStore())
    .WithTextEmbeddingGeneration(embeddingService)
    .Build();
var collection = "chat-knowledge";
await memory.SaveInformationAsync(collection, "Semantic Kernel is an open-source SDK for AI orchestration.", "sk-info-1");
await memory.SaveInformationAsync(collection, "RAG stands for Retrieval-Augmented Generation, combining LLMs with external data.", "sk-info-2");
#pragma warning restore SKEXP0001
while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(userInput) || userInput.ToLower() == "exit") break;

    // Retrieve relevant info from memory (RAG)
    var retrieved = new List<string>();
    await foreach (var item in memory.SearchAsync(collection, userInput, limit: 2, minRelevanceScore: 0.7))
    {
        if (!string.IsNullOrEmpty(item.Metadata.Text))
        {
            retrieved.Add(item.Metadata.Text);
            Console.WriteLine("retrieved RAG {0}", item.Metadata.Text);
        }
    }

    // Build context for the LLM
    var context = string.Join("\n", retrieved);
    var prompt = context.Length > 0
        ? $"Relevant info:\n{context}\n\nUser: {userInput}"
        : userInput;

    // Get response from OpenAI using chat completion
    var chatService = kernel.GetRequiredService<IChatCompletionService>();
    var chatHistory = new ChatHistory();
    chatHistory.AddUserMessage(prompt);
    var response = await chatService.GetChatMessageContentsAsync(chatHistory);
    Console.WriteLine($"Assistant: {response.FirstOrDefault()?.Content}");

}
#pragma warning restore SKEXP0001
