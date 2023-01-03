using Carbon.Base;
using Carbon.Modules;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Fixes
{
	public partial class Fixes_ItemContainer
	{
		[HookAttribute.Patch("IItemContainerAmountPatch", typeof(ItemContainer), "GetAmount", new System.Type[] { typeof(int), typeof(bool) })]
		[HookAttribute.Identifier("14c9a1716b4248d4b707fbced49641fd")]
		[HookAttribute.Options(HookFlags.Static | HookFlags.Hidden)]

		public class Fixes_ItemContainer_GetAmount_14c9a1716b4248d4b707fbced49641fd
		{
			public static bool Prefix(int itemid, bool onlyUsableAmounts, out int __result, ref ItemContainer __instance)
			{
				var overrides = BaseModule.GetModule<RustOverridesModule>();
				if (!overrides.ConfigInstance.Enabled || !overrides.Config.DisallowSkinnedItemsFromBeingCraftable)
				{
					__result = default;
					return true;
				}

				var num = 0;
				foreach (var item in __instance.itemList)
				{
					if (item.info.itemid == itemid && item.skin == 0 && (!onlyUsableAmounts || !item.IsBusy()))
					{
						num += item.amount;
					}
				}

				__result = num;
				return false;
			}
		}
	}
}
