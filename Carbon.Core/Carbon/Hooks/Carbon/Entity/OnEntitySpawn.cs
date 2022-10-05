﻿///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 

using Carbon.Core;

namespace Carbon.Extended
{
	[CarbonHook("OnEntitySpawn"), CarbonHook.Category(Hook.Category.Enum.Entity)]
	[CarbonHook.Parameter("entity", typeof(BaseNetworkable))]
	[CarbonHook.Info("Called before any networked entity has spawned (including trees).")]
	[CarbonHook.Patch(typeof(BaseNetworkable), "Spawn")]
	public class BaseNetworkable_Spawn_OnEntitySpawn
	{
		public static void Prefix(ref BaseNetworkable __instance)
		{
			HookExecutor.CallStaticHook("OnEntitySpawn", __instance);
		}
	}
}
