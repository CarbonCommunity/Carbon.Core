
/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Resources
{
	public partial class Resources_GrowableEntity
	{
		[HookAttribute.Patch("OnGrowableUpdate", typeof(GrowableEntity), "RunUpdate", new System.Type[] { })]
		[HookAttribute.Identifier("0122c151c6dd4865a324d79d05155a57")]

		// Called before growable entity will be updated.

		public class Resources_GrowableEntity_RunUpdate_0122c151c6dd4865a324d79d05155a57
		{
			public static void Prefix(ref GrowableEntity __instance)
			{
				HookCaller.CallStaticHook("OnGrowableUpdate", __instance);
			}
		}
	}
}
