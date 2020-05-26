using System;
using System.Collections.Generic;
using System.Linq;

namespace Cappta.Logging.Extensions
{
	public static class IDictionaryExtensions
	{
		public static IDictionary<string, object> CreateOrGetSubDict(this IDictionary<string, object> dictionary, string key)
		{
			if (dictionary.ContainsKey(key)) { return dictionary[key] as IDictionary<string, object>; }

			var subDict = new SortedDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			dictionary.Add(key, subDict);
			return subDict;
		}

		public static void ForceAdd(this IDictionary<string, object> dictionary, string key, object value)
		{
			if (dictionary.TryAdd(key, value)) { return; }

			var currentValue = dictionary[key];
			if (currentValue is IDictionary<string, object> currentDictionaryValue
				&& value is IDictionary<string, object> dictionaryValue)
			{
				currentDictionaryValue.MergeWith(dictionaryValue);
				return;
			}
			if (currentValue is string currentStringValue
				&& value is string stringValue)
			{
				if (string.Equals(currentStringValue, stringValue)) { return; }
				if (stringValue == string.Empty) { return; }
				if (currentStringValue == string.Empty)
				{
					dictionary[key] = stringValue;
					return;
				}

				dictionary[key] = $"{stringValue}{Environment.NewLine}{currentValue}";
				return;
			}

			var counter = 2;
			while (dictionary.TryAdd($"{key}{counter}", value) == false) { counter++; }
		}

		public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
		{
			try
			{
				dictionary.Add(key, value);
				return true;
			}
			catch { return false; }
		}

		public static void MergeWith(this IDictionary<string, object> baseDictionary, params IDictionary<string, object>[] incomingDictionaries)
		{
			foreach (var incomingDictionary in incomingDictionaries)
			{
				foreach (var kvp in incomingDictionary)
				{
					baseDictionary.ForceAdd(kvp.Key, kvp.Value);
				}
			}
		}

		public static void RemoveNullValues(this IDictionary<string, object> dictionary)
		{
			foreach (var key in dictionary.Keys.ToArray())
			{
				var value = dictionary[key];
				if (value is IDictionary<string, object> subDict)
				{
					subDict.RemoveNullValues();
				}
				else if (value is null || value as string == string.Empty)
				{
					dictionary.Remove(key);
				}
			}
		}

		public static IDictionary<string, object> Flatten(this IDictionary<string, object> dictionary)
			=> dictionary.EnumerateFlatKVPs(null)
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

		private static IEnumerable<KeyValuePair<string, object>> EnumerateFlatKVPs(this IDictionary<string, object> dictionary, string basePath)
		{
			foreach (var kvp in dictionary)
			{
				var key = kvp.Key.ToPascalCase();
				var path = basePath == null ? key : basePath + key;
				if (kvp.Value is IDictionary<string, object> dictionaryValue)
				{
					foreach (var flattened in dictionaryValue.EnumerateFlatKVPs(path))
					{
						yield return flattened;
					}
					continue;
				}

				yield return new KeyValuePair<string, object>(path, kvp.Value);
			}
		}
	}
}
