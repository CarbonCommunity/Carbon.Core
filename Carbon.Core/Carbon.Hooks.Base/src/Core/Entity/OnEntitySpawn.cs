/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Entity
{
	public partial class Entity_BaseNetworkable
	{
		[HookAttribute.Patch("OnEntitySpawn", typeof(BaseNetworkable), "Spawn", new System.Type[] { })]
		[HookAttribute.Identifier("c7d1643393324307bdaa4c11df129a66")]

		// Called before any networked entity has spawned (including trees).

		public class Entity_BaseNetworkable_Spawn_c7d1643393324307bdaa4c11df129a66
		{
			public static void Prefix(ref BaseNetworkable __instance)
			{
				HookCaller.CallStaticHook("OnEntitySpawn", __instance);
			}
		}
	}
}
