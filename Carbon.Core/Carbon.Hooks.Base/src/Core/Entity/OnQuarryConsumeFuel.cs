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
	public partial class Entity_MiningQuarry
	{
		[HookAttribute.Patch("OnQuarryConsumeFuel", typeof(MiningQuarry), "FuelCheck", new System.Type[] { })]
		[HookAttribute.Identifier("28548a7241fa45bd8da612c9187c1e50")]

		// Used to override the fuel needed for any Quarries & Pumpjacks that are running on a server.

		public class Entity_MiningQuarry_FuelCheck_28548a7241fa45bd8da612c9187c1e50
		{
			public static bool Prefix(ref MiningQuarry __instance, out bool __result)
			{
				if (__instance.pendingWork > 0f)
				{
					__result = true;
					return false;
				}

				var fuel = __instance.fuelStoragePrefab.instance as StorageContainer;
				var item = fuel.inventory.FindItemsByItemName("diesel_barrel"); // Default behaviour
				var obj = Interface.CallHook("OnQuarryConsumeFuel", __instance, item);

				if (obj is Item itemObj)
				{
					item = itemObj;
				}

				if (item != null && item.amount >= 1)
				{
					__instance.pendingWork += __instance.workPerFuel;
					item.UseItem(1);
					__result = true;
					return false;
				}

				__result = false;
				return false;
			}
		}
	}
}