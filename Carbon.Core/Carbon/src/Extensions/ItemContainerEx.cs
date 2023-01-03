/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Extensions;

public static class ItemContainerEx
{
	public static int TakeSkinned(this ItemContainer container, int itemid, ulong skinId, bool onlyUsableAmounts)
	{
		var num = 0;

		foreach (var item in container.itemList)
		{
			if (item.info.itemid == itemid && item.skin == skinId && (!onlyUsableAmounts || !item.IsBusy()))
			{
				num += item.amount;
			}
		}

		return num;
	}
}
