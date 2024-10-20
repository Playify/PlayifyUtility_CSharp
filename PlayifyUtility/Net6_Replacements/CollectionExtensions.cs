#if NETFRAMEWORK
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;

[PublicAPI]
public static class CollectionExtensions{
	public static TValue? GetValueOrDefault<TKey,TValue>(this IReadOnlyDictionary<TKey,TValue> dictionary,TKey key){
		return dictionary.GetValueOrDefault(key,default!);
	}

	public static TValue GetValueOrDefault<TKey,TValue>(this IReadOnlyDictionary<TKey,TValue> dictionary,TKey key,TValue defaultValue){
		if(dictionary==null) throw new ArgumentNullException(nameof(dictionary));

		return dictionary.TryGetValue(key,out var value)?value:defaultValue;
	}

	public static bool TryAdd<TKey,TValue>(this IDictionary<TKey,TValue> dictionary,TKey key,TValue value){
		if(dictionary==null) throw new ArgumentNullException(nameof(dictionary));

		if(dictionary.ContainsKey(key)) return false;
		dictionary.Add(key,value);
		return true;

	}

	public static bool Remove<TKey,TValue>(this IDictionary<TKey,TValue> dictionary,TKey key,[MaybeNullWhen(false)]out TValue value){
		if(dictionary==null) throw new ArgumentNullException(nameof(dictionary));

		if(dictionary.TryGetValue(key,out value)){
			dictionary.Remove(key);
			return true;
		}

		value=default;
		return false;
	}
}
#endif