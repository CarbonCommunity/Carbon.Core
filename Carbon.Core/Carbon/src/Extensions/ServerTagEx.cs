using System.Reflection;
using HarmonyLib;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Extensions;

public static class ServerTagEx
{
	internal static PropertyInfo _gameTags = AccessTools.TypeByName("Steamworks.SteamServer").GetProperty("GameTags", BindingFlags.Public | BindingFlags.Static);

	public static bool SetRequiredTag(string tag)
	{
		var tags = _gameTags.GetValue(null) as string;

		if (!tags.Contains($",{tag}"))
		{
			_gameTags.SetValue(null, $"{tags},{tag}");
			return true;
		}

		return false;
	}

	public static bool UnsetRequiredTag(string tag)
	{
		var tags = _gameTags.GetValue(null) as string;

		if (tags.Contains($",{tag}"))
		{
			_gameTags.SetValue(null, tags.Replace($",{tag}", ""));
			return true;
		}

		return false;
	}
}
