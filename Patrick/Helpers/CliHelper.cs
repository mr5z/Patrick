﻿using System.Collections.Generic;
using System.Text;

namespace Patrick.Helpers
{
	static class CliHelper
	{
		public static List<string> CombineOption(string[]? texts, char separator)
		{
			var list = new List<string>();

			if (texts == null)
				return list;

			for (var i = 0; i < texts.Length; ++i)
			{
				var txt = texts[i];
				if (txt.StartsWith("\""))
				{
					var (j, value) = FindPair('"', separator, i, texts);
					i = j;
					list.Add(value);
				}
				else if (txt.StartsWith('\''))
				{
					var (j, value) = FindPair('\'', separator, i, texts);
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
			var builder = new StringBuilder(textArray[currentIndex] + separator);
			var i = currentIndex + 1;
			var prem = builder.ToString();
			if (!prem.StartsWith(token) || !prem.EndsWith(token))
            {
				while (i < textArray.Length)
				{
					var endTxt = textArray[i++];
					builder.Append(endTxt + separator);
					if (endTxt.EndsWith(token))
						break;
				}
			}
			return (i - 1, builder.ToString().Replace(token.ToString(), "").Trim());
		}
	}
}