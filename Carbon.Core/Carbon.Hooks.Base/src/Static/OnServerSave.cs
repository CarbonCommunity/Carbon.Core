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
	public partial class Static_SaveRestore
	{
		[HookAttribute.Patch("OnServerSave", typeof(SaveRestore), "DoAutomatedSave", new System.Type[] { typeof(bool) })]
		[HookAttribute.Identifier("eb9f4139698447f594d20fb698c1eb15")]
		[HookAttribute.Options(HookFlags.Static | HookFlags.IgnoreChecksum)]

		// Called before the server saves.

		public class Static_SaveRestore_DoAutomatedSave_eb9f4139698447f594d20fb698c1eb15
		{
			public static void Prefix(bool AndWait = false)
			{
				Carbon.Logger.Log($"Saving plugin configuration and data..");
				HookCaller.CallStaticHook("OnServerSave");

				Carbon.Logger.Log($"Saving Carbon state..");
				Interface.Oxide.Permission.SaveData();
			}
		}
	}
}
