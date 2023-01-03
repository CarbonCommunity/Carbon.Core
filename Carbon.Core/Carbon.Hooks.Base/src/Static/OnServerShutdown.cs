using Oxide.Core;

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
		[HookAttribute.Patch("OnServerShutdown", typeof(ServerMgr), "Shutdown", new System.Type[] { })]
		[HookAttribute.Identifier("8a0574c7d2d9420580a5ee90a37de357")]
		[HookAttribute.Options(HookFlags.Static | HookFlags.IgnoreChecksum)]

		// Useful for saving something on server shutdown.

		public class Static_ServerMgr_Shutdown_8a0574c7d2d9420580a5ee90a37de357
		{
			public static void Prefix()
			{
				Carbon.Logger.Log($"Saving plugin configuration and data..");
				HookCaller.CallStaticHook("OnServerSave");
				HookCaller.CallStaticHook("OnServerShutdown");

				Carbon.Logger.Log($"Saving Carbon state..");
				Interface.Oxide.Permission.SaveData();

				Carbon.Logger.Log($"Shutting down Carbon..");
				Interface.Oxide.OnShutdown();
				Community.Runtime.HarmonyProcessor.Clear();
				Community.Runtime.ScriptProcessor.Clear();
			}
		}
	}
}
