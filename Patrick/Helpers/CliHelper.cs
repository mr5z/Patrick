using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Patrick.Helpers
{
    public static class CliHelper
	{
		public static readonly string[] DefaultOptionPrefies = new[] { "-", "--" };

		public class Option<TKey> where TKey : struct
		{
			public Option(TKey key, params string[] values)
			{
				Key = key;
				Values = values;
			}
			public void Deconstruct(out TKey key, out string?[] values)
			{
				key = Key;
				values = Values;
			}
			public TKey Key { get; }
			public string?[] Values { get; }
		}

		public class OptionResult<TKey>
		{
			private readonly IDictionary<TKey, IEnumerable<string?>?> options;
			public OptionResult(IDictionary<TKey, IEnumerable<string?>?> options)
			{
				this.options = options;
			}
			public IEnumerable<string?> this[TKey key] => options[key]!;

			public bool TryGetFirst(TKey key, out string? value)
            {
				if (options.TryGetValue(key, out var values) && values != null)
                {
					value = values.First();
					return true;
				}
				value = null;
				return false;
            }
		}

		public static OptionResult<TKey> ParseOptions<TKey>(string text, params Option<TKey>[] options)
			where TKey : struct, Enum
		{
			return ParseOptions(text, DefaultOptionPrefies, options);
		}

		public static OptionResult<TKey> ParseOptions<TKey>(string text, string[] optionPrefix, params Option<TKey>[] options)
			where TKey : struct, Enum
		{
			const char componentSeparator = ' ';
			var values = EnumDefaultValues<TKey, IEnumerable<string?>?>();
			var components = text.Split(componentSeparator, StringSplitOptions.RemoveEmptyEntries);
			var combinedOptions = CombineOption(components, componentSeparator);
			var entries = new Queue<string>(combinedOptions);
			var optionList = new List<Option<TKey>>(options);
			while (entries.Count > 0)
			{
				var entry = entries.Dequeue();
				var option = optionList.FirstOrDefault(e => e.Values.Contains(entry));
				if (option == default)
					continue;

				var valueList = new List<string?>();
				while (entries.Count > 0)
				{
					entry = entries.Peek();
					if (string.IsNullOrEmpty(entry) ||
						optionPrefix.Any(e => entry.StartsWith(e, StringComparison.OrdinalIgnoreCase)))
						break;
					valueList.Add(entries.Dequeue());
				}
				values[option.Key] = valueList;
				optionList.Remove(option);
			}
			return new OptionResult<TKey>(values);
		}

		private static Dictionary<TKey, TValue?> EnumDefaultValues<TKey, TValue>()
			where TKey : struct, Enum
		{
			var type = typeof(TKey);
			if (!type.IsEnum)
				throw new ArgumentException("Key must be type enum.");

			var values = Enum.GetValues(type).OfType<TKey>();

			return values.ToDictionary(e => e, e => default(TValue));
		}

		public static List<string> CombineOption(string[]? texts, char separator)
		{
			var list = new List<string>();

			if (texts == null)
				return list;

			var knownPairs = new char[] { '"', '\'' };

			for (var i = 0; i < texts.Length; ++i)
			{
				var txt = texts[i];
				var pair = knownPairs.FirstOrDefault(txt.StartsWith);

				if (pair != default)
				{
					var (j, value) = FindPair(pair, separator, i, texts);
					i = j;
					list.Add(value);
				}
				else
                {
					list.Add(txt);
				}
			}
			return list;
		}

		private static (int, string) FindPair(char token, char separator, int currentIndex, string[] textArray)
		{
			var prem = textArray[currentIndex];
			if (prem.Length > 1 && prem.StartsWith(token) && prem.EndsWith(token))
				return (currentIndex, prem.Replace(token.ToString(), string.Empty));

            var builder = new StringBuilder(prem + separator);
			var i = currentIndex + 1;
			while (i < textArray.Length)
			{
				var endTxt = textArray[i++];
				builder.Append(endTxt + separator);
				if (endTxt.EndsWith(token))
					break;
			}
			return (i - 1, builder.ToString().Replace(token.ToString(), string.Empty).Trim());
		}
	}
}