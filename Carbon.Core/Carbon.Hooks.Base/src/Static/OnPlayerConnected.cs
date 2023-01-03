/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Static
{
	public partial class Static_BasePlayer
	{
		[HookAttribute.Patch("OnPlayerConnected", typeof(BasePlayer), "PlayerInit", new System.Type[] { typeof(Network.Connection) })]
		[HookAttribute.Identifier("9e1a7b5738f441d698700fcbf25ca8b1")]
		[HookAttribute.Options(HookFlags.Static | HookFlags.IgnoreChecksum)]

		// Called after the player object is created, but before the player has spawned.

		public class Static_BasePlayer_PlayerInit_9e1a7b5738f441d698700fcbf25ca8b1
		{
			public static void Postfix(Network.Connection c)
			{
				HookCaller.CallStaticHook("OnPlayerConnected", c.player as BasePlayer);
			}
		}
	}
}
