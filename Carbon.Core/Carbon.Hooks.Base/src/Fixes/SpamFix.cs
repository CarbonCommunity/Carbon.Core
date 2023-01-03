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
	public partial class Fixes_Debug
	{
		[HookAttribute.Patch("IDebugLogWarning", typeof(Debug), "LogWarning", new System.Type[] { typeof(object), typeof(UnityEngine.Object) })]
		[HookAttribute.Identifier("e75bcb98e64645bd96afc60742d1171b")]
		[HookAttribute.Options(HookFlags.Static | HookFlags.Hidden)]

		public class Fixes_Debug_LogWarning_e75bcb98e64645bd96afc60742d1171b
		{
			public static bool Prefix(object message, UnityEngine.Object context)
			{
				try
				{
					var log = message.ToString();

					if (
						log.Contains("failed to sample navmesh at position") ||
						log.Contains("not close enough to the NavMesh")
					) return false;
				}
				catch { }

				return true;
			}
		}
	}

	public partial class Fixes_Debug
	{
		[HookAttribute.Patch("IDebugLog", typeof(Debug), "Log", new System.Type[] { typeof(object), typeof(UnityEngine.Object) })]
		[HookAttribute.Identifier("43f0c374d1e14157aa75104af3a6f36b")]
		[HookAttribute.Options(HookFlags.Static | HookFlags.Hidden)]

		public class Fixes_Debug_Log_43f0c374d1e14157aa75104af3a6f36b
		{
			public static bool Prefix(object message, UnityEngine.Object context)
			{
				try
				{
					var log = message.ToString();

					if (
						log.Contains("performing complete refresh, please wait")
					) return false;
				}
				catch { }

				return true;
			}
		}
	}
}
