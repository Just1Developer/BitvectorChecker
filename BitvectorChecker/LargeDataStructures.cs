using System.Collections;

namespace BitvectorChecker;

using System;
using System.Collections.Generic;

public class LargeDataStructures
{
	
}

public class LargeList<T> : IEnumerable<T>
{
	private const long ChunkSize = 1L << 20; // 1 million elements per chunk
	private List<T[]> _chunks;
	private long _count;

	public LargeList()
	{
		_chunks = new List<T[]>();
	}

	public void Add(T item)
	{
		long chunkIndex = _count / ChunkSize;
		long indexInChunk = _count % ChunkSize;

		if (chunkIndex >= _chunks.Count)
		{
			_chunks.Add(new T[ChunkSize]);
		}

		_chunks[(int)chunkIndex][indexInChunk] = item;
		_count++;
	}

	public T this[long index]
	{
		get
		{
			long chunkIndex = index / ChunkSize;
			long indexInChunk = index % ChunkSize;
			return _chunks[(int)chunkIndex][indexInChunk];
		}
		set
		{
			long chunkIndex = index / ChunkSize;
			long indexInChunk = index % ChunkSize;
			_chunks[(int)chunkIndex][indexInChunk] = value;
		}
	}

	public long Count => _count;

	public IEnumerator<T> GetEnumerator()
	{
		for (long i = 0; i < _count; i++)
		{
			yield return this[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

public class LargeDictionary<TKey, TValue>
{
	private const long ChunkSize = 1L << 20; // 1 million elements per chunk
	private List<Dictionary<TKey, TValue>> _chunks;

	public LargeDictionary()
	{
		_chunks = new List<Dictionary<TKey, TValue>>();
	}

	private Dictionary<TKey, TValue> GetChunk(TKey key)
	{
		long hash = Math.Abs(key.GetHashCode());
		long chunkIndex = hash / ChunkSize;

		while (chunkIndex >= _chunks.Count)
		{
			_chunks.Add(new Dictionary<TKey, TValue>());
		}

		return _chunks[(int)chunkIndex];
	}

	public void Add(TKey key, TValue value)
	{
		GetChunk(key).Add(key, value);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return GetChunk(key).TryGetValue(key, out value);
	}

	public TValue GetValueOrDefault(TKey key, TValue defaultValue = default(TValue))
	{
		if (TryGetValue(key, out TValue value))
		{
			return value;
		}
		return defaultValue;
	}

	public TValue this[TKey key]
	{
		get
		{
			return GetChunk(key)[key];
		}
		set
		{
			GetChunk(key)[key] = value;
		}
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		foreach (var chunk in _chunks)
		{
			foreach (var kvp in chunk)
			{
				yield return kvp;
			}
		}
	}
}
