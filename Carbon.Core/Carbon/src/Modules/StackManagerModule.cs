using System;
using System.Collections.Generic;
using Carbon.Base;
using UnityEngine;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * Copyright (c) 2022 kasvoton
 * All rights reserved.
 *
 */

namespace Carbon.Modules;

public class StackManagerModule : CarbonModule<StackManagerConfig, StackManagerData>
{
	public override string Name => "StackManager";
	public override Type Type => typeof(StackManagerModule);

	private void OnServerInitialized()
	{
		var hasChanged = false;
		foreach (var item in ItemManager.itemList)
		{
			if (DataInstance.ItemMapping.ContainsKey(item.itemid)) continue;

			DataInstance.ItemMapping.Add(item.itemid, item.stackable);
			hasChanged = true;
		}

		if (hasChanged) Save();

		OnEnableStatus();
	}

	public override void OnEnabled(bool initialized)
	{
		base.OnEnabled(initialized);

		if (!initialized || ItemManager.itemList == null) return;

		foreach (var category in Config.Categories)
		{
			foreach (var item in ItemManager.itemList)
			{
				if (item.category != category.Key || Config.Blacklist.Contains(item.shortname) || Config.Items.ContainsKey(item.shortname)) continue;

				DataInstance.ItemMapping.TryGetValue(item.itemid, out var originalStack);

				item.stackable = Mathf.Clamp((int)(originalStack * category.Value * Config.GlobalMultiplier), 1, int.MaxValue);
			}
		}

		foreach (var item in ItemManager.itemList)
		{
			if (!Config.Items.ContainsKey(item.shortname)) continue;

			var multiplier = Config.Items[item.shortname];

			DataInstance.ItemMapping.TryGetValue(item.itemid, out var originalStack);

			item.stackable = Mathf.Clamp((int)(originalStack * multiplier * Config.GlobalMultiplier), 1, int.MaxValue);
		}

		Carbon.Logger.Log("Item stacks patched");
	}
	public override void OnDisabled(bool initialized)
	{
		base.OnDisabled(initialized);

		if (!initialized) return;

		Carbon.Logger.Log("Rolling back item manager");

		foreach (var category in Config.Categories)
		{
			foreach (var item in ItemManager.itemList)
			{
				if (item.category != category.Key || Config.Blacklist.Contains(item.shortname) || Config.Items.ContainsKey(item.shortname)) continue;

				DataInstance.ItemMapping.TryGetValue(item.itemid, out var originalStack);

				item.stackable = originalStack;
			}
		}

		foreach (var item in ItemManager.itemList)
		{
			if (!Config.Items.ContainsKey(item.shortname)) continue;

			DataInstance.ItemMapping.TryGetValue(item.itemid, out var originalStack);

			item.stackable = originalStack;
		}
	}
}

public class StackManagerConfig
{
	public float GlobalMultiplier = 1f;

	public HashSet<string> Blacklist = new HashSet<string>
	{
		"water",
		"water.salt"
	};

	public Dictionary<ItemCategory, float> Categories = new Dictionary<ItemCategory, float>
	{
		{ ItemCategory.Ammunition, 1 },
		{ ItemCategory.Attire, 1 },
		{ ItemCategory.Component, 1 },
		{ ItemCategory.Construction, 1 },
		{ ItemCategory.Electrical, 1 },
		{ ItemCategory.Food, 1 },
		{ ItemCategory.Fun, 1 },
		{ ItemCategory.Items, 1 },
		{ ItemCategory.Medical, 1 },
		{ ItemCategory.Misc, 1 },
		{ ItemCategory.Resources, 1 },
		{ ItemCategory.Tool, 1 },
		{ ItemCategory.Traps, 1 },
		{ ItemCategory.Weapon, 1 }
	};

	public Dictionary<string, float> Items = new Dictionary<string, float>
	{
		{ "explosive.timed", 1 }
	};
}
public class StackManagerData
{
	public Dictionary<int, int> ItemMapping = new Dictionary<int, int>();
}
