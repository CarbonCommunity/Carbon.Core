using System;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Extensions;

public class RandomEx
{
	public static System.Random Random = new System.Random();
	public const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

	#region String

	public static string GetRandomString(int size)
	{
		if (Random == null)
		{
			Random = new System.Random();
		}

		var randomChars = new char[size];

		for (int i = 0; i < size; i++)
		{
			randomChars[i] = Chars[Random.Next(Chars.Length)];
		}

		return new string(randomChars);
	}

	public static string GetRandomString(int size, string chars)
	{
		if (Random == null)
		{
			Random = new System.Random();
		}

		var randomChars = new char[size];

		for (int i = 0; i < size; i++)
		{
			randomChars[i] = chars[Random.Next(chars.Length)];
		}

		return new string(randomChars);
	}
	public static string GetRandomString(int size, int seed)
	{
		Random = new System.Random(seed);
		var RandomChars = new char[size];

		for (int i = 0; i < size; i++)
		{
			RandomChars[i] = Chars[Random.Next(Chars.Length)];
		}

		return new string(RandomChars);
	}
	public static string GetRandomString(int size, string chars, int seed)
	{
		Random = new System.Random(seed);
		var RandomChars = new char[size];

		for (int i = 0; i < size; i++)
		{
			RandomChars[i] = chars[Random.Next(chars.Length)];
		}

		return new string(RandomChars);
	}

	#endregion

	#region Integer

	public static int GetRandomInteger(int min, int max)
	{
		if (Random == null)
		{
			Random = new System.Random();
		}

		return Random.Next(min, max);
	}
	public static int GetRandomInteger()
	{
		if (Random == null)
		{
			Random = new System.Random();
		}

		return Random.Next(int.MinValue, int.MaxValue);
	}
	public static int GetRandomInteger(int seed)
	{
		Random = new System.Random(seed);
		return Random.Next(int.MinValue, int.MaxValue);
	}
	public static int GetRandomInteger(int min, int max, int seed)
	{
		Random = new System.Random(seed);
		return Random.Next(min, max);
	}

	#endregion

	#region Float

	public static float GetRandomFloat(float min, float max)
	{
		if (Random == null)
		{
			Random = new System.Random();
		}

		return (float)Random.NextDouble() * (max - min) + min;
	}
	public static float GetRandomFloat()
	{
		if (Random == null)
		{
			Random = new System.Random();
		}

		return (float)Random.NextDouble();
	}
	public static float GetRandomFloat(int seed)
	{
		Random = new System.Random(seed);
		return (float)Random.NextDouble() * (float.MaxValue - float.MinValue) + float.MinValue;
	}
	public static float GetRandomFloat(float min, float max, int seed)
	{
		Random = new System.Random(seed);
		return (float)Random.NextDouble() * (max - min) + min;
	}

	#endregion

	#region Shuffle

	public static string GetShuffledString(string str)
	{
		if (Random == null)
		{
			Random = new System.Random();
		}

		var chars = str.ToCharArray();
		var length = chars.Length;

		while (length > 1)
		{
			length--;

			var randomLength = Random.Next(length + 1);
			var value = chars[randomLength];

			chars[randomLength] = chars[length];
			chars[length] = value;
		}

		return new string(chars);
	}
	public static string GetShuffledString(string str, int seed)
	{
		Random = new System.Random(seed);
		var chars = str.ToCharArray();
		var length = chars.Length;

		while (length > 1)
		{
			length--;

			var RandomLength = Random.Next(length + 1);
			var value = chars[RandomLength];

			chars[RandomLength] = chars[length];
			chars[length] = value;
		}

		return new string(chars);
	}

	#endregion
}
