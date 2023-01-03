using UnityEngine;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_World
{
	public partial class World_World
	{
		[HookAttribute.Patch("OnWorldPrefabSpawn", typeof(World), "Spawn", new System.Type[] { typeof(string), typeof(Prefab), typeof(Vector3), typeof(Quaternion), typeof(Vector3) })]
		[HookAttribute.Identifier("1986e3235a1946149d45132ed5ba3f0a")]

		// Called before a world prefab is spawned.

		public class World_World_Spawn_1986e3235a1946149d45132ed5ba3f0a
		{
			public static void Prefix(string category, Prefab prefab, Vector3 position, Quaternion rotation, Vector3 scale) { }
		}
	}
}
