﻿///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 

using Carbon.Core;
using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(ConVar.Harmony), "Load")]
public class Harmony_Load
{
	public const string CARBON_LOADED = nameof(CARBON_LOADED);

	public static bool Prefix(ConsoleSystem.Arg args)
	{
		var mod = args.Args != null && args.Args.Length > 0 ? args.Args[0] : null;

		if (!mod.Equals("carbon", System.StringComparison.OrdinalIgnoreCase) &&
			 !mod.Equals("carbon-unix", System.StringComparison.OrdinalIgnoreCase)) return true;

		if (string.IsNullOrEmpty(mod) ||
			!mod.StartsWith("carbon", System.StringComparison.OrdinalIgnoreCase)) return true;

		var oldMod = PlayerPrefs.GetString(CARBON_LOADED);

		if (oldMod == mod)
		{
			Carbon.Logger.Warn($"An instance of Carbon v{CarbonCore.Version} is already loaded.");
			return false;
		}
		else
		{
			CarbonCore.Instance?.UnInit();
			HarmonyLoader.TryUnloadMod(oldMod);
			Carbon.Logger.Warn($"Unloaded previous: {oldMod}");
			CarbonCore.Instance = null;
		}

		PlayerPrefs.SetString(CARBON_LOADED, mod);

		return true;
	}
}

[HarmonyPatch(typeof(ConVar.Harmony), "Unload")]
public class Harmony_Unload
{
	public static bool Prefix(ConsoleSystem.Arg args)
	{
		var mod = args.Args != null && args.Args.Length > 0 ? args.Args[0] : null;

		if (string.IsNullOrEmpty(mod)) return true;

		if (mod.Equals("carbon", System.StringComparison.OrdinalIgnoreCase) ||
			 mod.Equals("carbon-unix", System.StringComparison.OrdinalIgnoreCase))
			mod = CarbonDefines.Name;

		if (!mod.StartsWith("carbon", System.StringComparison.OrdinalIgnoreCase)) return true;

		PlayerPrefs.SetString(Harmony_Load.CARBON_LOADED, string.Empty);
		CarbonCore.Instance?.UnInit();
		CarbonCore.Instance = null;

		HarmonyLoader.TryUnloadMod(mod);
		return false;
	}
}
