using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Carbon.Oxide;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core;

public static class ExtensionMethods
{
	public static string Basename(this string text, string extension = null)
	{
		if (extension != null)
		{
			if (!extension.Equals("*.*"))
			{
				if (extension[0] == '*')
				{
					extension = extension.Substring(1);
				}
				return Regex.Match(text, "([^\\\\/]+)\\" + extension + "+$").Groups[1].Value;
			}

			var match = Regex.Match(text, "([^\\\\/]+)\\.[^\\.]+$");
			if (match.Success)
			{
				return match.Groups[1].Value;
			}
		}

		return Regex.Match(text, "[^\\\\/]+$").Groups[0].Value;
	}

	public static bool Contains<T>(this T[] array, T value)
	{
		foreach (T t in array)
		{
			if (t.Equals(value))
			{
				return true;
			}
		}

		return false;
	}

	public static string Dirname(this string text)
	{
		return Regex.Match(text, "(.+)[\\/][^\\/]+$").Groups[1].Value;
	}

	public static string Humanize(this string name)
	{
		return Regex.Replace(name, "(\\B[A-Z])", " $1");
	}

	public static bool IsSteamId(this string id)
	{
		ulong num;
		return ulong.TryParse(id, out num) && num > 76561197960265728UL;
	}

	public static bool IsSteamId(this ulong id)
	{
		return id > 76561197960265728UL;
	}

	public static string Plaintext(this string text)
	{
		return Formatter.ToPlaintext(text);
	}

	public static string QuoteSafe(this string text)
	{
		return "\"" + text.Replace("\"", "\\\"").TrimEnd(new char[]
		{
			'\\'
		}) + "\"";
	}

	public static string Quote(this string text)
	{
		return text.QuoteSafe();
	}

	public static T Sample<T>(this T[] array)
	{
		return array[UnityEngine.Random.Range(0, array.Length)];
	}

	public static string Sanitize(this string text)
	{
		return text.Replace("{", "{{").Replace("}", "}}");
	}

	public static string SentenceCase(this string text)
	{
		return new Regex("(^[a-z])|\\.\\s+(.)", RegexOptions.ExplicitCapture).Replace(text.ToLower(), (Match s) => s.Value.ToUpper());
	}

	public static string TitleCase(this string text)
	{
		return CultureInfo.InstalledUICulture.TextInfo.ToTitleCase(text.Contains('_') ? text.Replace('_', ' ') : text);
	}

	public static string Titleize(this string text)
	{
		return text.TitleCase();
	}

	public static string ToSentence<T>(this IEnumerable<T> items)
	{
		var enumerator = items.GetEnumerator();

		if (!enumerator.MoveNext())
		{
			return string.Empty;
		}

		var t = enumerator.Current;

		if (enumerator.MoveNext())
		{
			StringBuilder stringBuilder = new StringBuilder((t != null) ? t.ToString() : null);
			bool flag = true;
			while (flag)
			{
				T t2 = enumerator.Current;
				flag = enumerator.MoveNext();
				stringBuilder.Append(flag ? ", " : " and ");
				stringBuilder.Append(t2);
			}
			return stringBuilder.ToString();
		}
		if (t == null)
		{
			return null;
		}

		return t.ToString();
	}

	public static string Truncate(this string text, int max)
	{
		if (text.Length > max)
		{
			return text.Substring(0, max) + " ...";
		}

		return text;
	}
}
