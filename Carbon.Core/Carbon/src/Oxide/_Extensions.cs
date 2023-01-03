/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core.Plugins;

public static class Extensions
{
	public static void Clear(this ItemContainer cont)
	{
		var items = Facepunch.Pool.GetList<Item>();
		items.AddRange(cont.itemList);

		foreach (var item in items)
		{
			item.Remove(.1f);
		}

		ItemManager.DoRemoves();
	}
}
