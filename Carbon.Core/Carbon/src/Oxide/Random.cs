namespace Oxide.Core;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

public static class Random
{
	internal static System.Random _random = new System.Random();

	public static int Range(int min, int max)
	{
		return _random.Next(min, max);
	}
	public static int Range(int max)
	{
		return _random.Next(max);
	}
	public static double Range(double min, double max)
	{
		return min + _random.NextDouble() * (max - min);
	}
	public static float Range(float min, float max)
	{
		return (float)Range((double)min, (double)max);
	}
}
