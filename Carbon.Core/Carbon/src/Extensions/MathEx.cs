using System;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Extensions;

public static class MathEx
{
	public static int Clamp(this int value, int min, int max)
	{
		if (value < min)
		{
			value = min;
		}
		else if (value > max)
		{
			value = max;
		}

		return value;
	}
	public static float Clamp(this float value, float min, float max)
	{
		if (value < min)
		{
			value = min;
		}
		else if (value > max)
		{
			value = max;
		}

		return value;
	}

	public static float Percentage(this float value, float total, float percent = 100)
	{
		return (float)Math.Round((double)percent * value) / total;
	}
	public static float Percentage(this int value, int total, float percent = 100)
	{
		return (float)Math.Round((double)percent * value) / total;
	}
	public static float Percentage(this long value, long total, float percent = 100)
	{
		return (float)Math.Round((double)percent * value) / total;
	}

	public static float Scale(this float oldValue, float oldMin, float oldMax, float newMin, float newMax)
	{
		var oldRange = (oldMax - oldMin);
		var newRange = (newMax - newMin);
		var newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

		return newValue;
	}
	public static int Scale(this int oldValue, int oldMin, int oldMax, int newMin, int newMax)
	{
		var oldRange = (oldMax - oldMin);
		var newRange = (newMax - newMin);
		var newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

		return newValue;
	}
	public static long Scale(this long oldValue, long oldMin, long oldMax, long newMin, long newMax)
	{
		var oldRange = (oldMax - oldMin);
		var newRange = (newMax - newMin);
		var newValue = (((oldValue - oldMin) * newRange) / oldRange) + newMin;

		return newValue;
	}

	public static string ToHex(this int value)
	{
		return value.ToString("X");
	}
	public static int FromHex(this string value)
	{
		return int.Parse(value, System.Globalization.NumberStyles.HexNumber);
	}

	public static string ToBinary(this int value)
	{
		return Convert.ToString(value, 2);
	}
	public static int FromBinary(this string value)
	{
		return Convert.ToInt32(value, 2);
	}
}
