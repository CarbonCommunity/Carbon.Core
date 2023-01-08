///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 

using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Carbon.Hooks
{
	[OxideHook("OnCorpsePopulate", typeof(BaseCorpse)), OxideHook.Category(Hook.Category.Enum.Player)]
	[OxideHook.Parameter("this", typeof(ScarecrowNPC))]
	[OxideHook.Parameter("corpse", typeof(NPCPlayerCorpse))]
	[OxideHook.Info("Useful for denying items' deployment.")]
	[OxideHook.Patch(typeof(ScarecrowNPC), "CreateCorpse")]
	public class ScarecrowNPC_CreateCorpse
	{
		public static bool Prefix(ref ScarecrowNPC __instance, out BaseCorpse __result)
		{
			var npcplayerCorpse = __instance.DropCorpse("assets/prefabs/npc/murderer/murderer_corpse.prefab") as NPCPlayerCorpse;
			if (npcplayerCorpse)
			{
				npcplayerCorpse.transform.position = npcplayerCorpse.transform.position + Vector3.down * __instance.NavAgent.baseOffset;
				npcplayerCorpse.SetLootableIn(2f);
				npcplayerCorpse.SetFlag(BaseEntity.Flags.Reserved5, __instance.HasPlayerFlag(BasePlayer.PlayerFlags.DisplaySash));
				npcplayerCorpse.SetFlag(BaseEntity.Flags.Reserved2, true);
				npcplayerCorpse.TakeFrom(__instance.inventory.containerMain, __instance.inventory.containerWear, __instance.inventory.containerBelt);
				npcplayerCorpse.playerName = "Scarecrow";
				npcplayerCorpse.playerSteamID = __instance.userID;
				npcplayerCorpse.Spawn();
				foreach (ItemContainer container in npcplayerCorpse.containers)
					container.Clear();
				if (__instance.LootSpawnSlots.Length != 0)
				{
					var obj = Interface.CallHook("OnCorpsePopulate", __instance, npcplayerCorpse);
					if (obj is BaseCorpse)
					{
						__result = (BaseCorpse)obj;
						return false;
					}
					foreach (var lootSpawnSlot in __instance.LootSpawnSlots)
					{
						for (int j = 0; j < lootSpawnSlot.numberToSpawn; j++)
						{
							if (Random.Range(0f, 1f) <= lootSpawnSlot.probability)
							{
								lootSpawnSlot.definition.SpawnIntoContainer(npcplayerCorpse.containers[0]);
							}
						}
					}
				}
			}
			__result = npcplayerCorpse;
			return false;
		}
	}

	[OxideHook("OnCorpsePopulate", typeof(BaseCorpse)), OxideHook.Category(Hook.Category.Enum.Player)]
	[OxideHook.Parameter("this", typeof(HumanNPC))]
	[OxideHook.Parameter("corpse", typeof(NPCPlayerCorpse))]
	[OxideHook.Info("Useful for denying items' deployment.")]
	[OxideHook.Patch(typeof(HumanNPC), "CreateCorpse")]
	public class HumanNPC_CreateCorpse
	{
		public static bool Prefix(ref HumanNPC __instance, out BaseCorpse __result)
		{
			var npcplayerCorpse = __instance.DropCorpse("assets/rust.ai/agents/npcplayer/pet/frankensteinpet_corpse.prefab") as NPCPlayerCorpse;
			if (npcplayerCorpse)
			{
				npcplayerCorpse.transform.position = npcplayerCorpse.transform.position + Vector3.down * __instance.NavAgent.baseOffset;
				npcplayerCorpse.SetLootableIn(2f);
				npcplayerCorpse.SetFlag(BaseEntity.Flags.Reserved5, __instance.HasPlayerFlag(BasePlayer.PlayerFlags.DisplaySash), false, true);
				npcplayerCorpse.SetFlag(BaseEntity.Flags.Reserved2, true, false, true);
				npcplayerCorpse.TakeFrom(new ItemContainer[]
				{
					__instance.inventory.containerMain,
					__instance.inventory.containerWear,
					__instance.inventory.containerBelt
				});
				npcplayerCorpse.playerName = __instance.OverrideCorpseName();
				npcplayerCorpse.playerSteamID = __instance.userID;
				npcplayerCorpse.Spawn();
				npcplayerCorpse.TakeChildren(__instance);

				var containers = npcplayerCorpse.containers;
				for (int i = 0; i < containers.Length; i++)
				{
					containers[i].Clear();
				}
				if (__instance.LootSpawnSlots.Length != 0)
				{
					var obj = Interface.CallHook("OnCorpsePopulate", __instance, npcplayerCorpse);
					if (obj is BaseCorpse)
					{
						__result = (BaseCorpse)obj;
						return false;
					}
					foreach (var lootSpawnSlot in __instance.LootSpawnSlots)
					{
						for (int j = 0; j < lootSpawnSlot.numberToSpawn; j++)
						{
							if (Random.Range(0f, 1f) <= lootSpawnSlot.probability)
							{
								lootSpawnSlot.definition.SpawnIntoContainer(npcplayerCorpse.containers[0]);
							}
						}
					}
				}
			}
			__result = npcplayerCorpse;
			return false;
		}
	}

	[OxideHook("OnCorpsePopulate", typeof(BaseCorpse)), OxideHook.Category(Hook.Category.Enum.Player)]
	[OxideHook.Parameter("this", typeof(FrankensteinPet))]
	[OxideHook.Parameter("corpse", typeof(NPCPlayerCorpse))]
	[OxideHook.Info("Useful for denying items' deployment.")]
	[OxideHook.Patch(typeof(FrankensteinPet), "CreateCorpse")]
	public class FrankensteinPet_CreateCorpse
	{
		public static bool Prefix(ref FrankensteinPet __instance, out BaseCorpse __result)
		{
			var npcplayerCorpse = __instance.DropCorpse("assets/rust.ai/agents/npcplayer/pet/frankensteinpet_corpse.prefab") as NPCPlayerCorpse;
			if (npcplayerCorpse)
			{
				npcplayerCorpse.transform.position = npcplayerCorpse.transform.position + Vector3.down * __instance.NavAgent.baseOffset;
				npcplayerCorpse.SetLootableIn(2f);
				npcplayerCorpse.SetFlag(BaseEntity.Flags.Reserved5, __instance.HasPlayerFlag(BasePlayer.PlayerFlags.DisplaySash), false, true);
				npcplayerCorpse.SetFlag(BaseEntity.Flags.Reserved2, true, false, true);
				npcplayerCorpse.TakeFrom(new ItemContainer[]
				{
					__instance.inventory.containerMain,
					__instance.inventory.containerWear,
					__instance.inventory.containerBelt
				});
				npcplayerCorpse.playerName = __instance.OverrideCorpseName();
				npcplayerCorpse.playerSteamID = __instance.userID;
				npcplayerCorpse.Spawn();

				var containers = npcplayerCorpse.containers;
				for (int i = 0; i < containers.Length; i++)
				{
					containers[i].Clear();
				}
				var obj = Interface.CallHook("OnCorpsePopulate", __instance, npcplayerCorpse);
				if (obj is global::BaseCorpse)
				{
					__result = (global::BaseCorpse)obj;
					return false;
				}
			}
			__result = npcplayerCorpse;
			return false;
		}
	}
}
