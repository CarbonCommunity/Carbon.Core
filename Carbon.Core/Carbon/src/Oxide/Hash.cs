using System;
using System.Collections;
using System.Collections.Generic;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core;

public class Hash<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	private readonly IDictionary<TKey, TValue> dictionary;

	public TValue this[TKey key]
	{
		get
		{
			if (TryGetValue(key, out var value))
			{
				return value;
			}

			if (typeof(TValue).IsValueType)
			{
				return (TValue)Activator.CreateInstance(typeof(TValue));
			}

			return default(TValue);
		}
		set
		{
			if (value == null)
			{
				dictionary.Remove(key);
			}
			else
			{
				dictionary[key] = value;
			}
		}
	}

	public ICollection<TKey> Keys => dictionary.Keys;

	public ICollection<TValue> Values => dictionary.Values;

	public int Count => dictionary.Count;

	public bool IsReadOnly => dictionary.IsReadOnly;

	public Hash()
	{
		dictionary = new Dictionary<TKey, TValue>();
	}

	public Hash(IEqualityComparer<TKey> comparer)
	{
		dictionary = new Dictionary<TKey, TValue>(comparer);
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return dictionary.GetEnumerator();
	}

	public bool ContainsKey(TKey key)
	{
		return dictionary.ContainsKey(key);
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		return dictionary.Contains(item);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
	{
		dictionary.CopyTo(array, index);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return dictionary.TryGetValue(key, out value);
	}

	public void Add(TKey key, TValue value)
	{
		dictionary.Add(key, value);
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		dictionary.Add(item);
	}

	public bool Remove(TKey key)
	{
		return dictionary.Remove(key);
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		return dictionary.Remove(item);
	}

	public void Clear()
	{
		dictionary.Clear();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
