using System.Collections.Generic;
using System.Linq;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Extensions;

public static class StringArrayEx
{
	/// <summary>
	/// Combines a string array into a string with separator contents between them.
	/// </summary>
	/// <param name="array">Targeted string array.</param>
	/// <param name="separator">String array to string with this separation between each word.</param>
	/// <param name="lastSeparator">String array to string with this ending separation between the last word.</param>
	/// <returns></returns>
	public static string ToString(this string[] array, string separator, string lastSeparator = null)
	{
		if (string.IsNullOrEmpty(lastSeparator)) lastSeparator = separator;

		if (array.Length == 0) { return string.Empty; }
		if (array.Length == 1) { return array[0]; }

		var str = "";

		var iArray = new List<string>();
		for (int i = 0; i < array.Length - 1; i++)
		{
			iArray.Add(array[i]);
		}

		str = string.Join(separator, iArray.ToArray());
		str += string.Format("{0}{1}", lastSeparator, array[array.Length - 1]);

		return str;
	}

	/// <summary>
	/// Combines a string array into a string with separator contents between them.
	/// </summary>
	/// <param name="array">Targeted string array.</param>
	/// <param name="startIndex">Gets the array starting from this index.</param>
	/// <param name="separator">String array to string with this separation between each word.</param>
	/// <param name="throwError">Return an error if errored about start index over it's length, if the case.</param>
	/// <returns></returns>
	public static string ToString(this string[] array, int startIndex, string separator = " ", bool throwError = false)
	{
		if (array.Length == 0) { return string.Empty; }
		if (array.Length == 1) { return array[0]; }

		if (startIndex > array.Length)
		{
			return throwError ? string.Format("ERROR! The start index ({0}) is over the length of the arguments ({1}).", startIndex, array.Length) : null;
		}

		return string.Join(separator, array, startIndex, array.Length - startIndex);
	}

	/// <summary>
	/// Combines a string array into a string with separator contents between them.
	/// </summary>
	/// <param name="array">Targeted string array.</param>
	/// <param name="startIndex">Gets the array starting from this index.</param>
	/// <param name="separator">String array to string with this separation between each word.</param>
	/// <param name="lastSeparator">String array to string with this ending separation between the last word.</param>
	/// <param name="throwError">Return an error if errored about start index over it's length, if the case.</param>
	/// <returns></returns>
	public static string ToString(this string[] array, int startIndex, string separator, string lastSeparator = null, bool throwError = false)
	{
		if (lastSeparator == null) lastSeparator = separator;

		if (array.Length == 0) { return string.Empty; }
		if (array.Length == 1) { return array[0]; }

		if (startIndex > array.Length)
		{
			return throwError ? string.Format("ERROR! The start index ({0}) is over the length of the arguments ({1}).", startIndex, array.Length) : null;
		}

		var str = "";

		var iArray = new List<string>();
		for (int i = 0; i < array.Length - 1; i++)
		{
			iArray.Add(array[i]);
		}

		str = string.Join(separator, iArray.ToArray(), startIndex, iArray.Count - startIndex);
		str += string.Format("{0}{1}", lastSeparator, array[array.Length - 1]);

		return str;
	}

	/// <summary>
	/// Cuts text into pieces at chunkSize size and returns them each.
	/// </summary>
	/// <param name="text">Targeted string.</param>
	/// <param name="chunkSize">Chunk size.</param>
	/// <returns></returns>
	public static string[] Split(this string text, int chunkSize, bool includeLeftovers = true)
	{
		var splits = Enumerable.Range(0, text.Length / chunkSize)
			.Select(i => text.Substring(i * chunkSize, chunkSize)).ToArray();

		var remainingText = text.Replace(ToString(splits.ToArray(), ""), "");
		var splitsList = splits.ToList();

		if (includeLeftovers && !string.IsNullOrEmpty(remainingText))
		{
			splitsList.Add(remainingText);
		}

		return splitsList.ToArray();
	}
}
