using Oxide.Core;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Resources
{
	public partial class Resources_BaseRidableAnimal
	{
		[HookAttribute.Patch("OnHorseDung", typeof(BaseRidableAnimal), "DoDung", new System.Type[] { })]
		[HookAttribute.Identifier("e5beabfdc524496dbf5657149585bdac")]

		// Called when a dung is spawned at the backside of the horse.
		// Override the return with an item to replace the Dung with any other item that's being dropped.

		public class Resources_BaseRidableAnimal_DoDung_e5beabfdc524496dbf5657149585bdac
		{
			public static bool Prefix(ref BaseRidableAnimal __instance)
			{
				var result = Interface.CallHook("OnHorseDung", __instance);
				var dungItem = (Item)null;

				if (result is Item)
				{
					dungItem = result as Item;
				}

				if (dungItem == null) dungItem = ItemManager.Create(__instance.Dung, 1, 0uL);

				__instance.dungProduction -= 1f;
				dungItem.Drop(__instance.transform.position + -__instance.transform.forward + Vector3.up * 1.1f + Random.insideUnitSphere * 0.1f, -__instance.transform.forward);
				return false;
			}
		}
	}
}
