/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Static
{
	public partial class Static_ServerMgr
	{
		[HookAttribute.Patch("OnPlayerDisconnected", typeof(ServerMgr), "OnDisconnected", new System.Type[] { typeof(string), typeof(Network.Connection) })]
		[HookAttribute.Identifier("4d9dcdaf8fbd4923a96eef18a7da7488")]
		[HookAttribute.Options(HookFlags.Static | HookFlags.IgnoreChecksum)]

		// Called after the player has disconnected from the server.

		public class Static_ServerMgr_OnDisconnected_4d9dcdaf8fbd4923a96eef18a7da7488
		{
			public static void Postfix(string strReason, Network.Connection connection)
			{
				HookCaller.CallStaticHook("OnPlayerDisconnected", connection.player as BasePlayer, strReason);
			}
		}
	}
}
