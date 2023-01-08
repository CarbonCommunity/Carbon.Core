///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 
using UnityEngine.Assertions;

namespace Carbon.Hooks
{
	[OxideHook("OnActiveItemChange"), OxideHook.Category(Hook.Category.Enum.Item)]
	[OxideHook.Parameter("player", typeof(BasePlayer))]
	[OxideHook.Parameter("oldItem", typeof(Item))]
	[OxideHook.Parameter("newItemID", typeof(uint))]
	[OxideHook.Info("Called when active item is attempting to update")]
	[OxideHook.Patch(typeof(BasePlayer), "UpdateActiveItem")]
	public class BasePlayer_UpdateActiveItemE
	{
		public static bool Prefix(uint itemID, ref BasePlayer __instance)
		{
			Assert.IsTrue(__instance.isServer, "Realm should be server!");
			if (__instance.svActiveItemID == itemID)
				return false;
			if (__instance.equippingBlocked)
				itemID = 0U;
			if (__instance.IsItemHoldRestricted(__instance.inventory.containerBelt.FindItemByUID(itemID)))
				itemID = 0U;
			Item activeItem1 = __instance.GetActiveItem();
			if (HookCaller.CallStaticHook("OnActiveItemChange", __instance, activeItem1, itemID) != null)
				return false;
			__instance.svActiveItemID = 0U;
			if (activeItem1 != null)
			{
				HeldEntity heldEntity = activeItem1.GetHeldEntity() as HeldEntity;
				if (heldEntity != null)
					heldEntity.SetHeld(false);
			}
			__instance.svActiveItemID = itemID;
			__instance.SendNetworkUpdate();
			Item activeItem2 = __instance.GetActiveItem();
			if (activeItem2 != null)
			{
				HeldEntity heldEntity = activeItem2.GetHeldEntity() as HeldEntity;
				if (heldEntity != null)
					heldEntity.SetHeld(true);
				__instance.NotifyGesturesNewItemEquipped();
			}
			__instance.inventory.UpdatedVisibleHolsteredItems();
			HookCaller.CallStaticHook("OnActiveItemChanged", __instance, activeItem1, activeItem2);

			return false; // Skip orginal code
		}
	}
}
