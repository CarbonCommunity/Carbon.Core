/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Fun
{
	public partial class Fun_BasePlayer
	{
		[HookAttribute.Patch("OnJackieChan", typeof(BasePlayer), "PlayerInit", new System.Type[] { typeof(Network.Connection) })]
		[HookAttribute.Identifier("98afa60bc10945799214031b1c017ac5")]

		// Checks if player that connected is Jackie Chan.

		public class Fun_BasePlayer_PlayerInit_98afa60bc10945799214031b1c017ac5
		{
			public static void Prefix(Network.Connection c)
			{
				try
				{
					var player = c.player as BasePlayer;

					if (player.displayName == "Jackie Chan")
						HookCaller.CallStaticHook("OnJackieChan", player);
				}
				catch { }
			}
		}
	}
}
