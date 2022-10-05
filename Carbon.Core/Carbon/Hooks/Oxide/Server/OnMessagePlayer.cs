﻿///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 

using Oxide.Core;

namespace Carbon.Extended
{
	[OxideHook("OnMessagePlayer", typeof(object)), OxideHook.Category(Hook.Category.Enum.Server)]
	[OxideHook.Info("Useful for intercepting server messages before they get to their intended target.")]
	[OxideHook.Parameter("msg", typeof(string))]
	[OxideHook.Parameter("this", typeof(BasePlayer))]
	[OxideHook.Patch(typeof(BasePlayer), "ChatMessage")]
	public class BasePlayer_ChatMessage
	{
		public static bool Prefix(string msg, ref BasePlayer __instance)
		{
			if (!__instance.isServer) return false;

			return Interface.CallHook("OnMessagePlayer", msg, __instance) == null;
		}
	}
}
