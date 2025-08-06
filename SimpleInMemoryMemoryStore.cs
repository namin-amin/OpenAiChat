
#pragma warning disable SKEXP0001
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Memory;

#pragma warning disable SKEXP0001
public class SimpleInMemoryMemoryStore : IMemoryStore
{
    private readonly ConcurrentDictionary<string, List<MemoryRecord>> _collections = new();

    public Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        _collections.TryAdd(collectionName, new List<MemoryRecord>());
        return Task.CompletedTask;
    }

    public Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        _collections.TryRemove(collectionName, out _);
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<string> GetCollectionsAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var key in _collections.Keys)
        {
            yield return key;
        }
        await Task.CompletedTask;
    }

    public Task<bool> DoesCollectionExistAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_collections.ContainsKey(collectionName));
    }

    public Task<string> UpsertAsync(string collectionName, MemoryRecord record, CancellationToken cancellationToken = default)
    {
        if (!_collections.ContainsKey(collectionName))
            _collections[collectionName] = new List<MemoryRecord>();
        _collections[collectionName].RemoveAll(r => r.Key == record.Key);
        _collections[collectionName].Add(record);
        return Task.FromResult(record.Key);
    }

    public async IAsyncEnumerable<string> UpsertBatchAsync(string collectionName, IEnumerable<MemoryRecord> records, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var record in records)
        {
            await UpsertAsync(collectionName, record, cancellationToken);
            yield return record.Key;
        }
    }

    public Task<MemoryRecord?> GetAsync(string collectionName, string key, bool withEmbedding = false, CancellationToken cancellationToken = default)
    {
        if (_collections.TryGetValue(collectionName, out var records))
        {
            var record = records.Find(r => r.Key == key);
            return Task.FromResult<MemoryRecord?>(record);
        }
        return Task.FromResult<MemoryRecord?>(null);
    }

    public async IAsyncEnumerable<MemoryRecord> GetBatchAsync(string collectionName, IEnumerable<string> keys, bool withEmbedding = false, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_collections.TryGetValue(collectionName, out var records))
        {
            foreach (var key in keys)
            {
                var record = records.Find(r => r.Key == key);
                if (record != null) yield return record;
            }
        }
        await Task.CompletedTask;
    }

    public Task RemoveAsync(string collectionName, string key, CancellationToken cancellationToken = default)
    {
        if (_collections.TryGetValue(collectionName, out var records))
        {
            records.RemoveAll(r => r.Key == key);
        }
        return Task.CompletedTask;
    }

    public Task RemoveBatchAsync(string collectionName, IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        if (_collections.TryGetValue(collectionName, out var records))
        {
            foreach (var key in keys)
            {
                records.RemoveAll(r => r.Key == key);
            }
        }
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<MemoryRecord> GetAllAsync(string collectionName, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_collections.TryGetValue(collectionName, out var records))
        {
            foreach (var record in records)
            {
                yield return record;
            }
        }
        await Task.CompletedTask;
    }

    // Nearest match methods: dummy implementation, no embedding support
    public async IAsyncEnumerable<(MemoryRecord, double)> GetNearestMatchesAsync(string collectionName, ReadOnlyMemory<float> embedding, int limit, double minRelevanceScore, bool withEmbedding = false, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_collections.TryGetValue(collectionName, out var records))
        {
            int count = 0;
            foreach (var record in records)
            {
                if (count++ >= limit) break;
                yield return (record, 1.0); // Always max relevance
            }
        }
        await Task.CompletedTask;
    }

    public Task<(MemoryRecord, double)?> GetNearestMatchAsync(string collectionName, ReadOnlyMemory<float> embedding, double minRelevanceScore, bool withEmbedding = false, CancellationToken cancellationToken = default)
    {
        if (_collections.TryGetValue(collectionName, out var records) && records.Count > 0)
            return Task.FromResult<(MemoryRecord, double)?>((records[0], 1.0));
        return Task.FromResult<(MemoryRecord, double)?>(null);
    }
}
#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0001
