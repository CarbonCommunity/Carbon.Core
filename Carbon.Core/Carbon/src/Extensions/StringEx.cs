using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Components;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Extensions;

public static class StringEx
{
	#region "To" Family

	/// <summary>
	/// Parses the string into a float.
	/// </summary>
	/// <param name="value">String target.</param>
	/// <param name="default">Default value.</param>
	/// <returns></returns>
	public static float ToFloat(this string value, float @default = 0.0f)
	{
		return (float)value.ToDecimal((decimal)@default);
	}

	/// <summary>
	/// Parses the string into a int.
	/// </summary>
	/// <param name="value">String target.</param>
	/// <param name="default">Default value.</param>
	/// <returns></returns>
	public static int ToInt(this string value, int @default = 0)
	{
		var result = value.ToDecimal(@default);

		if (result <= new decimal(int.MinValue))
		{
			return int.MinValue;
		}

		if (!(result >= new decimal(int.MaxValue)))
		{
			return (int)result;
		}

		return int.MaxValue;
	}

	/// <summary>
	/// Parses the string into a uint.
	/// </summary>
	/// <param name="value">String target.</param>
	/// <param name="default">Default value.</param>
	/// <returns></returns>
	public static uint ToUint(this string value, uint @default = 0)
	{
		if (!uint.TryParse(value, out uint result)) result = @default;

		return result;
	}

	/// <summary>
	/// Parses the string into a bool.
	/// </summary>
	/// <param name="value">String target.</param>
	/// <param name="default">Default value.</param>
	/// <returns></returns>
	public static bool ToBool(this string value, bool @default = false)
	{
		if (string.IsNullOrEmpty(value) || value == null)
		{
			return @default;
		}

		if (value == "1")
		{
			return true;
		}

		value = value.Trim().ToLower();

		return value == "true" || value == "t" || (value == "yes" || value == "y") || value == "1";
	}

	/// <summary>
	/// Parses the string into a decimal.
	/// </summary>
	/// <param name="value">String target.</param>
	/// <param name="default">Default value.</param>
	/// <returns></returns>
	public static decimal ToDecimal(this string value, decimal @default = 0M)
	{
		if (!decimal.TryParse(value, out decimal result)) result = @default;

		return result;
	}

	/// <summary>
	/// Parses the string into a long.
	/// </summary>
	/// <param name="value">String target.</param>
	/// <param name="default">Default value.</param>
	/// <returns></returns>
	public static long ToLong(this string value, long @default = 0)
	{
		if (!long.TryParse(value, out long result)) result = @default;

		return result;
	}

	/// <summary>
	/// Parses the string into a ulong.
	/// </summary>
	/// <param name="value">String target.</param>
	/// <param name="default">Default value.</param>
	/// <returns></returns>
	public static ulong ToUlong(this string value, ulong @default = 0)
	{
		if (!ulong.TryParse(value, out ulong result)) result = @default;

		return result;
	}

	/// <summary>
	/// Converts the string to Base64.
	/// </summary>
	/// <param name="value">String target.</param>
	/// <returns></returns>
	public static byte[] ToBytes(this string value)
	{
		return Convert.FromBase64String(value);
	}

	public static string ToCamelCase(this string str)
	{
		var splits = str.Split(' ');
		var result = "";

		foreach (var split in splits)
		{
			result += $"{split[0].ToString().ToUpper()}{split.Substring(1, split.Length - 1)} ";
		}

		return result;
	}

	#endregion

	#region "Validate" Family

	/// <summary>
	/// Checks if a string contains one of the valid characters.
	/// </summary>
	/// <param name="value">String target.</param>
	/// <param name="validCharacters">String of valid characters.</param>
	public static bool IsValid(this string value, string validCharacters)
	{
		foreach (var character in value)
		{
			if (!validCharacters.Contains(character))
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Checks if a string contains one of the valid characters. Returns a array of characters that are invalid.
	/// </summary>
	/// <param name="value">String target.</param>
	/// <param name="validCharacters">String of valid characters.</param>
	/// <returns></returns>
	public static string[] IsValidComplex(this string value, string validCharacters)
	{
		var invalidCharacters = new List<string>();

		foreach (var character in value)
		{
			if (!validCharacters.Contains(character))
			{
				invalidCharacters.Add(character.ToString());
			}
		}

		return invalidCharacters.ToArray();
	}

	#endregion

	#region "Truncate" Family

	/// <summary>
	/// Trucates the string's length to the maximum length (chops it).
	/// </summary>
	/// <param name="value">String target.</param>
	/// <param name="maxLength">String maximum size.</param>
	/// <returns></returns>
	public static string Truncate(this string value, int maxLength)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}

		return value.Length <= maxLength ? value : value.Substring(0, maxLength);
	}

	/// <summary>
	/// Trucates the string's length to the maximum length (chops it).
	/// </summary>
	/// <param name="value">String target.</param>
	/// <param name="maxLength">String maximum size.</param>
	/// <param name="elipsis">Usually those '...' at the end of a string.</param>
	/// <param name="countElipsisLength">If true, <paramref name="value"/>'s length will sum up with elipsis length, so it will chop it properly.</param>
	/// <returns></returns>
	public static string Truncate(this string value, int maxLength, string elipsis, bool countElipsisLength = true)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}

		if (countElipsisLength)
		{
			return value.Length <= maxLength ? value : value.Substring(0, maxLength - elipsis.Length) + elipsis;
		}

		return value.Length <= maxLength ? value : value.Substring(0, maxLength) + elipsis;
	}

	#endregion

	#region "Plural" Family

	/// <summary>
	/// Returns a plural when the number is not 1, and singular if it is.
	/// </summary>
	/// <param name="value"></param>
	/// <param name="singularString"></param>
	/// <param name="pluralString"></param>
	/// <returns></returns>
	public static string Plural(this int value, string singularString, string pluralString)
	{
		return value == 1 ? singularString : pluralString;
	}

	#endregion

	#region "Numbering" Family

	public static string ToNumbered(int number, string separatingString = "-", bool camelCase = true)
	{
		if (number == 0)
			return camelCase ? "Zero" : "zero";

		if (number < 0)
			return (camelCase ? "Minus" : "minus ") + ToNumbered(Math.Abs(number), separatingString);

		var words = "";

		if ((number / 1000000) > 0)
		{
			words += ToNumbered(number / 1000000, separatingString) + (camelCase ? " Million" : " million ");
			number %= 1000000;
		}

		if ((number / 1000) > 0)
		{
			words += ToNumbered(number / 1000, separatingString) + (camelCase ? " Thousand" : " thousand ");
			number %= 1000;
		}

		if ((number / 100) > 0)
		{
			words += ToNumbered(number / 100, separatingString) + (camelCase ? " Hundred" : " hundred ");
			number %= 100;
		}

		if (number > 0)
		{
			if (words != "")
				words += "and ";

			var unitsMap = camelCase ?
				new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" } :
				new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
			var tensMap = camelCase ?
				new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" } :
				new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

			if (number < 20)
				words += unitsMap[number];
			else
			{
				words += tensMap[number / 10];

				if ((number % 10) > 0)
					words += separatingString + unitsMap[number % 10];
			}
		}

		return words;
	}

	#endregion

	#region "Morse" Family

	private static string Dot { get { return "."; } }
	private static string Dash { get { return "-"; } }

	public static Dictionary<char, string> MorseMapping { get; set; } = new Dictionary<char, string>()
	{
		{'a', string.Concat(Dot, Dash)},
		{'b', string.Concat(Dash, Dot, Dot, Dot)},
		{'c', string.Concat(Dash, Dot, Dash, Dot)},
		{'d', string.Concat(Dash, Dot, Dot)},
		{'e', Dot.ToString()},
		{'f', string.Concat(Dot, Dot, Dash, Dot)},
		{'g', string.Concat(Dash, Dash, Dot)},
		{'h', string.Concat(Dot, Dot, Dot, Dot)},
		{'i', string.Concat(Dot, Dot)},
		{'j', string.Concat(Dot, Dash, Dash, Dash)},
		{'k', string.Concat(Dash, Dot, Dash)},
		{'l', string.Concat(Dot, Dash, Dot, Dot)},
		{'m', string.Concat(Dash, Dash)},
		{'n', string.Concat(Dash, Dot)},
		{'o', string.Concat(Dash, Dash, Dash)},
		{'p', string.Concat(Dot, Dash, Dash, Dot)},
		{'q', string.Concat(Dash, Dash, Dot, Dash)},
		{'r', string.Concat(Dot, Dash, Dot)},
		{'s', string.Concat(Dot, Dot, Dot)},
		{'t', string.Concat(Dash)},
		{'u', string.Concat(Dot, Dot, Dash)},
		{'v', string.Concat(Dot, Dot, Dot, Dash)},
		{'w', string.Concat(Dot, Dash, Dash)},
		{'x', string.Concat(Dash, Dot, Dot, Dash)},
		{'y', string.Concat(Dash, Dot, Dash, Dash)},
		{'z', string.Concat(Dash, Dash, Dot, Dot)},
		{'0', string.Concat(Dash, Dash, Dash, Dash, Dash)},
		{'1', string.Concat(Dot, Dash, Dash, Dash, Dash)},
		{'2', string.Concat(Dot, Dot, Dash, Dash, Dash)},
		{'3', string.Concat(Dot, Dot, Dot, Dash, Dash)},
		{'4', string.Concat(Dot, Dot, Dot, Dot, Dash)},
		{'5', string.Concat(Dot, Dot, Dot, Dot, Dot)},
		{'6', string.Concat(Dash, Dot, Dot, Dot, Dot)},
		{'7', string.Concat(Dash, Dash, Dot, Dot, Dot)},
		{'8', string.Concat(Dash, Dash, Dash, Dot, Dot)},
		{'9', string.Concat(Dash, Dash, Dash, Dash, Dot)},
		{'?', string.Concat(Dot, Dot, Dash, Dash, Dot, Dot)},
		{'.', string.Concat(Dot, Dash, Dot, Dash, Dot, Dash)},
		{',', string.Concat(Dash, Dash, Dot, Dot, Dash, Dash)},
		{'\'', string.Concat(Dot, Dash, Dash, Dash, Dash, Dot)},
		{'!', string.Concat(Dash, Dot, Dash, Dot, Dash, Dash)},
		{'/', string.Concat(Dash, Dot, Dot, Dash, Dot)},
		{'(', string.Concat(Dash, Dot, Dash, Dash, Dot)},
		{')', string.Concat(Dash, Dot, Dash, Dash, Dot, Dash)},
		{'&', string.Concat(Dot, Dash, Dot, Dot, Dot)},
		{':', string.Concat(Dash, Dash, Dash, Dot, Dot, Dot)},
		{';', string.Concat(Dash, Dot, Dash, Dot, Dash, Dot)},
		{'=', string.Concat(Dash, Dot, Dot, Dot, Dash)},
		{'+', string.Concat(Dot, Dash, Dot, Dash, Dot)},
		{'-', string.Concat(Dash, Dot, Dot, Dot, Dot, Dash)},
		{'_', string.Concat(Dot, Dot, Dash, Dash, Dot, Dash)},
		{'"', string.Concat(Dot, Dash, Dot, Dot, Dash, Dot)},
		{'$', string.Concat(Dot, Dot, Dot, Dash, Dot, Dot, Dash)},
		{'@', string.Concat(Dot, Dash, Dash, Dot, Dash, Dot)}
	};

	public static string ToMorse(string value, string spacing = "/")
	{
		var sb = new StringBody();

		foreach (var @char in value.ToLower())
		{
			if (MorseMapping.ContainsKey(@char))
			{
				sb.Add($"{MorseMapping[@char]} ");
			}
			else if (@char == ' ')
			{
				sb.Add($"{spacing} ");
			}
			else
			{
				sb.Add($"{@char} ");
			}
		}

		return sb.ToString();
	}
	public static string FromMorse(string value, string spacing = "/")
	{
		var splits = value.Split(' ');
		var result = "";

		foreach (var split in splits)
		{
			var splitFormatted = split.Trim();

			if (splitFormatted == spacing)
			{
				result += " ";
			}
			else
			{
				result += MorseMapping.FirstOrDefault(x => x.Value == split).Key;
			}
		}

		return result;
	}

	#endregion

	#region "L33T" Family

	public static Dictionary<char, string> L33tMapping { get; set; } = new Dictionary<char, string>()
	{
		{'a', "4"},
		{'b', "13"},
		{'c', "("},
		{'d', "[)"},
		{'e', "3"},
		{'f', "|="},
		{'g', "6"},
		{'h', "|-|"},
		{'i', "|"},
		{'j', ".]"},
		{'k', "|<"},
		{'l', "1"},
		{'m', "|Y|"},
		{'n', "/\\/"},
		{'o', "0"},
		{'p', "|>"},
		{'q', "0,"},
		{'r', "|2"},
		{'s', "5"},
		{'t', "7"},
		{'u', "[_]"},
		{'v', "\\/"},
		{'w', "\\v/"},
		{'x', "}{"},
		{'y', "'/"},
		{'z', "2"}
	};

	public static string ToL33t(string value, string spacing = " ")
	{
		var sb = new StringBody();

		foreach (var @char in value)
		{
			var lowerChar = char.ToLower(@char);
			var prefix = char.IsUpper(@char) ? "^" : "";

			if (L33tMapping.ContainsKey(lowerChar))
			{
				sb.Add($"{prefix}{L33tMapping[lowerChar]} ");
			}
			else if (lowerChar == ' ')
			{
				sb.Add($"{spacing}");
			}
			else
			{
				sb.Add($"{prefix}{lowerChar} ");
			}
		}

		return sb.ToString();
	}
	public static string FromL33t(string value, string spacing = " ")
	{
		var splits = value.Split(' ');
		var result = "";

		foreach (var split in splits)
		{
			var splitFormatted = split.Trim();

			if (split.Contains(spacing))
			{
				result += " ";
			}
			else if (L33tMapping.ContainsValue(split.Replace("^", "")))
			{
				var normalCharacter = L33tMapping.FirstOrDefault(x => x.Value == split.Replace("^", ""));
				result += $"{(!split.Contains("^") ? normalCharacter.Key : char.ToUpper(normalCharacter.Key))} ";
			}
			else
			{
				result += $"{splitFormatted} ";
			}
		}

		return result;
	}

	#endregion
}
