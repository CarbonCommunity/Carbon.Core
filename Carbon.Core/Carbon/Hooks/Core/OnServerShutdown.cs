﻿///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 

using Carbon;
using Carbon.Core;
using Harmony;
using Oxide.Core;

[HarmonyPatch(typeof(ServerMgr), "Shutdown")]
public class OnServerShutdown
{
	internal static TimeSince _call;

	public static void Prefix()
	{
		if (_call <= 0.5f) return;

		Carbon.Logger.Log($"Saving Carbon plugins & shutting down");

		Interface.Oxide.OnShutdown();

		HookExecutor.CallStaticHook("OnServerSave");
		HookExecutor.CallStaticHook("OnServerShutdown");

		CarbonCore.Instance.HarmonyProcessor.Clear();
		CarbonCore.Instance.ScriptProcessor.Clear();

		_call = 0;
	}
}
