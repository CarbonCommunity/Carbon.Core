/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Player
{
	public partial class Entity_BasePlayer
	{
		[HookAttribute.Patch("OnBasePlayerAttacked", typeof(BasePlayer), "OnAttacked", new System.Type[] { typeof(HitInfo) })]
		[HookAttribute.Identifier("4f2d81e4b06f477cb773e7a1cceea710")]

		// Called before the player is being attacked.

		public class Entity_BasePlayer_OnAttacked_4f2d81e4b06f477cb773e7a1cceea710
		{
			public static bool Prefix(HitInfo info, ref BasePlayer __instance)
			{
				return HookCaller.CallStaticHook("OnPlayerSleep", __instance, info) == null;
			}
		}
	}
}
