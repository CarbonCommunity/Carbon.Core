using UnityEngine;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Fixes
{
	public partial class Fixes_ServerConsole
	{
		[HookAttribute.Patch("IServerConsoleLog", typeof(ServerConsole), "HandleLog", new System.Type[] { typeof(string), typeof(string), typeof(UnityEngine.LogType) })]
		[HookAttribute.Identifier("e540e379423c44b9b5855cff2506d5d1")]
		[HookAttribute.Options(HookFlags.Static | HookFlags.Hidden)]

		public class Fixes_ServerConsole_HandleLog_e540e379423c44b9b5855cff2506d5d1
		{
			public static bool Prefix(string message, string stackTrace, LogType type)
			{
				if (message.StartsWith("Trying to load assembly: DynamicAssembly")) return false;

				return true;
			}
		}
	}
}
