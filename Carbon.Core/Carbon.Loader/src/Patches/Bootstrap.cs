using System.Diagnostics;
using Carbon.LoaderEx.Harmony;
using Carbon.LoaderEx.Utility;
using HarmonyLib;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.LoaderEx.Patches;

internal static class __Bootstrap
{
#if USE_DEBUGGER
	[HarmonyPatch(typeof(Bootstrap), methodName: "Init_Tier0")]
	internal static class __Init_Tier0
	{
		public static void Postfix()
		{
			Logger.Warn("Waiting for a debugger connection..");
			var t = Task.Run(async delegate
			{
				while (!Debugger.IsAttached)
					await Task.Delay(1000);
				return;
			});

			t.Wait();
			Debugger.Break();
		}
	}
#endif

	[HarmonyPatch(typeof(Bootstrap), methodName: "StartupShared")]
	internal static class __StartupShared
	{
		public static void Prefix()
			=> HarmonyLoaderEx.GetInstance().Load("Carbon.dll");
	}
}
