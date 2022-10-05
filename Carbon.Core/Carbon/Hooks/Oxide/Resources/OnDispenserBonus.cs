﻿///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 

using Carbon.Core;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using static Construction;

namespace Carbon.Extended
{
	[OxideHook("OnDispenserBonus", typeof(Item)), OxideHook.Category(Hook.Category.Enum.Resources)]
	[OxideHook.Parameter("this", typeof(ResourceDispenser))]
	[OxideHook.Parameter("player", typeof(BasePlayer))]
	[OxideHook.Parameter("item", typeof(Item))]
	[OxideHook.Info("Called before the player is given a bonus item for gathering.")]
	[OxideHook.Info("Returning an Item replaces the existing Item")]
	[OxideHook.Info("Returning a false value overrides default behavior")]
	[OxideHook.Patch(typeof(ResourceDispenser), "AssignFinishBonus")]
	public class ResourceDispenser_AssignFinishBonus
	{
		public static bool Prefix(BasePlayer player, float fraction, ref ResourceDispenser __instance)
		{
			__instance.SendMessage("FinishBonusAssigned", SendMessageOptions.DontRequireReceiver);
			if ((double)fraction <= 0.0 || __instance.finishBonus == null)
				return false;
			foreach (ItemAmount finishBonu in __instance.finishBonus)
			{
				int amountToGive = Mathf.CeilToInt((float)(int)finishBonu.amount * Mathf.Clamp01(fraction));
				int gatherBonus = __instance.CalculateGatherBonus((BaseEntity)player, finishBonu, (float)amountToGive);
				Item obj1 = ItemManager.Create(finishBonu.itemDef, amountToGive + gatherBonus);
				if (obj1 != null)
				{
					object obj2 = HookExecutor.CallStaticHook("OnDispenserBonus", (object)__instance, (object)player, (object)obj1);
					if (obj2 is bool)
						if (!(bool)obj2)
							continue;
					if (obj2 is Item)
						obj1 = (Item)obj2;
					player.GiveItem(obj1, BaseEntity.GiveItemReason.ResourceHarvested);
				}
			}
			return false;
		}
	}

	[OxideHook("OnDispenserGather"), OxideHook.Category(Hook.Category.Enum.Resources)]
	[OxideHook.Parameter("this", typeof(ResourceDispenser))]
	[OxideHook.Parameter("entity", typeof(BaseEntity))]
	[OxideHook.Parameter("item", typeof(Item))]
	[OxideHook.Info("Called before the player is given items from a resource.")]
	[OxideHook.Info("Returning a non-null value overrides default behavior")]
	[OxideHook.Patch(typeof(ResourceDispenser), "GiveResourceFromItem")]
	public class ResourceDispenser_GiveResourceFromItem
	{
		public static bool Prefix(BaseEntity entity, ItemAmount itemAmt, float gatherDamage, float destroyFraction, AttackEntity attackWeapon, ref ResourceDispenser __instance)
		{
			if ((double)itemAmt.amount == 0.0)
				return false;
			float num1 = Mathf.Min(gatherDamage, __instance.baseEntity.Health()) / __instance.baseEntity.MaxHealth();
			float num2 = itemAmt.startAmount / __instance.startingItemCounts;
			float num3 = Mathf.Round(Mathf.Clamp(itemAmt.startAmount * num1 / num2, 0.0f, itemAmt.amount));
			float f = (float)((double)num3 * (double)destroyFraction * 2.0);
			if ((double)itemAmt.amount <= (double)num3 + (double)f)
			{
				float num4 = (num3 + f) / itemAmt.amount;
				num3 /= num4;
				f /= num4;
			}
			itemAmt.amount -= Mathf.Floor(num3);
			itemAmt.amount -= Mathf.Floor(f);
			if ((double)num3 < 1.0)
			{
				num3 = (double)UnityEngine.Random.Range(0.0f, 1f) <= (double)num3 ? 1f : 0.0f;
				itemAmt.amount = 0.0f;
			}
			if ((double)itemAmt.amount < 0.0)
				itemAmt.amount = 0.0f;
			if ((double)num3 < 1.0)
				return false;
			int gatherBonus = __instance.CalculateGatherBonus(entity, itemAmt, num3);
			int iAmount = Mathf.FloorToInt(num3) + gatherBonus;
			Item byItemId = ItemManager.CreateByItemID(itemAmt.itemid, iAmount);
			if (HookExecutor.CallStaticHook("OnDispenserGather", (object)__instance, (object)entity, (object)byItemId) != null || byItemId == null)
				return false;
			__instance.OverrideOwnership(byItemId, attackWeapon);
			entity.GiveItem(byItemId, BaseEntity.GiveItemReason.ResourceHarvested);
			return false;
		}
	}
}
