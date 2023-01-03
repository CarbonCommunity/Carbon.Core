using Oxide.Core;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Entity
{
	public partial class Entity_PatrolHelicopterAI
	{
		[HookAttribute.Patch("CanPatrolHeliSeePlayer", typeof(PatrolHelicopterAI), "PlayerVisible", new System.Type[] { typeof(BasePlayer) })]
		[HookAttribute.Identifier("d3849557c0c84c4c9248b5252f4c7db2")]

		// Can the Patrol Helicopter see the player.

		public class Entity_PatrolHelicopterAI_PlayerVisible_d3849557c0c84c4c9248b5252f4c7db2
		{
			public static bool Prefix(BasePlayer ply, ref PatrolHelicopterAI __instance, out bool __result)
			{
				var obj = Interface.CallHook("CanPatrolHeliSeePlayer", __instance, ply);
				if (obj is bool)
				{
					__result = (bool)obj;
					return false;
				}

				__result = default;
				return true;
			}
		}
	}
}
