using System.Linq;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Extensions;

public static class ArrayEx
{
	public static T[] Randomize<T>(this T[] list)
	{
		var listCopy = list.MakeCopy();
		var count = listCopy.Length;

		while (count > 1)
		{
			count--;

			var randomIndex = RandomEx.GetRandomInteger(0, count);
			var value = listCopy[randomIndex];

			listCopy[randomIndex] = listCopy[count];
			listCopy[count] = value;
		}

		return listCopy;
	}

	public static bool IsSame<T>(this T[] source, T[] target)
	{
		return !source.Except(target).Any();
	}
}
