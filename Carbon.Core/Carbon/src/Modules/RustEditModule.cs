using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Carbon.Base;
using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static BasePlayer;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * Copyright (c) 2022 bmgjet
 * All rights reserved.
 *
 */

namespace Carbon.Modules;

public class RustEditModule : CarbonModule<RustEditConfig, RustEditData>
{
	public override string Name => "RustEdit.Ext";
	public override Type Type => typeof(RustEditModule);

	private void OnServerInitialized()
	{
		if (!ConfigInstance.Enabled) return;


		#region Spawnpoints

		if (Spawnpoints != null && Spawnpoints.Count != 0)
		{
			PutsWarn($"Found {Spawnpoints.Count:n0} spawn-points.");
		}

		#endregion

		#region NPC Spawner

		if (serializedNPCData == null)
		{
			//Load XML from map
			var data = GetData("SerializedNPCData");

			if (data != null) { serializedNPCData = Deserialize<SerializedNPCData>(data); }
		}
		if (serializedNPCData != null)
		{
			var npcSpawners = serializedNPCData.npcSpawners;
			if (npcSpawners != null && npcSpawners.Count != 0)
			{
				Puts($"Loaded {serializedNPCData.npcSpawners.Count:n0} NPC Spawners.");
				for (int i = 0; i < npcSpawners.Count; i++)
				{
					//Setup spawners
					var npcspawner = new GameObject("NPCSpawner").AddComponent<NPCSpawner>();
					npcspawner.Initialize(serializedNPCData.npcSpawners[i]);
					NPCsList.Add(npcspawner);
				}
			}
		}
		if (NPCThread == null)
		{
			//Start NPC management thread.
			NPCThread = ServerMgr.Instance.StartCoroutine(NPCAIFunction());
		}

		#endregion
	}
	private object CanBradleyApcTarget(BradleyAPC apc, BaseEntity entity)
	{
		if (entity is NPCPlayer && DataInstance.NpcSpawner.APCIgnoreNPCs)
		{
			foreach (var npcspawner in NPCsList)
			{
				if (npcspawner == null || npcspawner.BOT == null) continue;
				if (npcspawner.BOT == entity) return false;
			}
		}

		return null;
	}
	private object OnTurretTarget(AutoTurret turret, BaseCombatEntity entity)
	{
		if (turret.OwnerID == 0 && DataInstance.NpcSpawner.NPCAutoTurretsIgnoreNPCs)
		{
			foreach (var npcspawner in NPCsList)
			{
				if (npcspawner == null || npcspawner.BOT == null)
				{
					continue;
				}
				if (npcspawner.BOT == entity)
				{
					return false;
				}
			}
		}

		return null;
	}
	private object OnBasePlayerAttacked(BasePlayer player, HitInfo info)
	{
		if (!DataInstance.NpcSpawner.UseRustEditNPCLogic)
		{
			// BMG Pirate Logic
			if (info.InitiatorPlayer != null)
			{
				// Has Been Attacked by player
				foreach (var nPCSpawner in NPCsList)
				{
					// Find if is a bot
					if (nPCSpawner.BOT.ToPlayer() == player)
					{
						var attackingplayer = info.InitiatorPlayer;

						// Get attacking player
						if (attackingplayer == null)
						{
							return true;
						}
						if (activePlayerList.Contains(attackingplayer))
						{
							// Attacking player is human player
							// If scarecrow is damage go to where attacker was
							if (player is ScarecrowNPC scarecrow)
							{
								scarecrow.Brain.Navigator.SetDestination(attackingplayer.transform.position, BaseNavigator.NavigationSpeed.Fast);
								return true;
							}

							// If ScientistNPC then fire back on the place player was
							if (player is ScientistNPC npc && npc.Brain != null)
							{
								// Adjust targets and settings temp
								npc.Brain.Senses.Memory.Targets.Clear();
								npc.Brain.Senses.Memory.Players.Clear();
								npc.Brain.Senses.Memory.Threats.Clear();
								npc.Brain.Senses.Memory.Targets.Add(attackingplayer);
								npc.Brain.Senses.Memory.Players.Add(attackingplayer);
								npc.Brain.Senses.Memory.Threats.Add(attackingplayer);

								if (npc.Brain.AttackRangeMultiplier != 10)
								{
									var oldTLR = npc.Brain.TargetLostRange;
									var oldARM = npc.Brain.AttackRangeMultiplier;
									var oldAIM = npc.aimConeScale;
									npc.aimConeScale = 0.5f;
									npc.Brain.TargetLostRange = 800;
									npc.Brain.AttackRangeMultiplier = 10;
									nPCSpawner.cooldown = true;
									npc.Brain.Events.Memory.Entity.Set(attackingplayer, 0);

									npc.Invoke(() =>
									{
										if (nPCSpawner != null) nPCSpawner.cooldown = false;

										// Restore default settings and remove targets
										if (npc != null)
										{
											npc.aimConeScale = oldAIM;
											npc.Brain.TargetLostRange = oldTLR;
											npc.Brain.AttackRangeMultiplier = oldARM;
											npc.Brain.Senses.Memory.Targets.Clear();
											npc.Brain.Senses.Memory.Players.Clear();
											npc.Brain.Senses.Memory.Threats.Clear();
										}
									}, 8f);
								}
							}
						}

						return true;
					}
				}
			}
		}

		return null;
	}

	#region Spawnpoint

	public List<Vector3> Spawnpoints = new List<Vector3>();

	public const string SpawnpointPrefab = "assets/bundled/prefabs/modding/volumes_and_triggers/spawn_point.prefab";

	[ChatCommand("showspawnpoints")]
	public void DoBroadcastSpawnpoints(BasePlayer player)
	{
		if (!ConfigInstance.Enabled) return;

		BroadcastSpawnpoints(player);
	}

	private object OnPlayerRespawn(BasePlayer player, BasePlayer.SpawnPoint point)
	{
		if (!ConfigInstance.Enabled) return null;

		return RespawnPlayer();
	}
	private object OnWorldPrefabSpawn(Prefab prefab, Vector3 position, Quaternion rotation, Vector3 scale)
	{
		if (!ConfigInstance.Enabled) return null;

		if (prefab.Name.Equals(SpawnpointPrefab))
		{
			Spawnpoints.Add(position);
			return true;
		}

		return null;
	}

	public SpawnPoint RespawnPlayer()
	{
		if (Spawnpoints == null || Spawnpoints.Count == 0) { return null; }

		var spawnpoint = Spawnpoints.GetRandom();
		var height = TerrainMeta.HeightMap.GetHeight(spawnpoint);

		if (spawnpoint.y <= height && AntiHack.TestInsideTerrain(spawnpoint)) { spawnpoint.y = height + 0.1f; }

		return new SpawnPoint { pos = spawnpoint, rot = default };
	}
	public void BroadcastSpawnpoints(BasePlayer player)
	{
		if (!player.IsAdmin) return;

		player.ChatMessage("Showing spawn points");

		foreach (var spawnpoint in Spawnpoints)
		{
			player.SendConsoleCommand("ddraw.sphere", 8f, Color.blue, spawnpoint, 1f);
		}
	}

	#endregion

	#region NPC Spawner

	public SerializedNPCSpawner serializedNPCSpawner;
	public SerializedNPCData serializedNPCData;
	public List<NPCSpawner> NPCsList = new List<NPCSpawner>();
	public Coroutine NPCThread;

	public enum NPCType
	{
		Scientist,
		Peacekeeper,
		HeavyScientist,
		JunkpileScientist,
		Bandit,
		Murderer,
		Scarecrow
	}

	public class SerializedNPCData { public List<SerializedNPCSpawner> npcSpawners = new List<SerializedNPCSpawner>(); }

	public class SerializedNPCSpawner
	{
		public int npcType;
		public int respawnMin;
		public int respawnMax;
		public VectorData position;
		public string category;
	}

	public class NPCSpawner : MonoBehaviour
	{
		public SerializedNPCSpawner serializedNPCSpawner;
		public BaseEntity BOT;
		public BaseEntity target = null;
		public bool Respawning = true;
		public float Delay = 0;
		public float RoamDistance = 30f;
		public NPCType type;
		public string prefab;
		public bool cooldown = false;
		public Vector3 OldPos;
		public bool stationary = false;
		public static Dictionary<NPCType, string> typeToPrefab = new Dictionary<NPCType, string>
		{
			{ NPCType.Scientist,"assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_roamtethered.prefab" },
			{ NPCType.Peacekeeper,"assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_peacekeeper.prefab" },
			{ NPCType.HeavyScientist,"assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_heavy.prefab" },
			{ NPCType.JunkpileScientist,"assets/rust.ai/agents/npcplayer/humannpc/scientist/scientistnpc_junkpile_pistol.prefab" },
			{ NPCType.Bandit,"assets/rust.ai/agents/npcplayer/humannpc/banditguard/npc_bandit_guard.prefab" },
			{ NPCType.Murderer,"assets/prefabs/npc/scarecrow/scarecrow.prefab" },
			{ NPCType.Scarecrow,"assets/prefabs/npc/scarecrow/scarecrow.prefab" },
		};

		public void Initialize(SerializedNPCSpawner serializedNPCSpawner0)
		{
			serializedNPCSpawner = serializedNPCSpawner0;
			type = (NPCType)serializedNPCSpawner.npcType;
			prefab = typeToPrefab[type];
			OldPos = serializedNPCSpawner.position;
		}

		public void SpawnNPC()
		{
			var module = GetModule<RustEditModule>();
			var settings = module.DataInstance.NpcSpawner;

			var position = (Vector3)serializedNPCSpawner.position;
			var num = 1;
			var npc = (ScientistNPC)null;
			var npc2 = (ScarecrowNPC)null;
			stationary = !module.IsOnGround(ref position, out num);

			if (settings.UseRustEditNPCLogic || type == NPCType.Scarecrow || type == NPCType.Murderer)
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(prefab, position, Quaternion.identity, false);
				if (baseEntity != null)
				{
					baseEntity.enableSaving = false;
					baseEntity.gameObject.AwakeFromInstantiate();
					baseEntity.EnableSaving(false);
					baseEntity.Spawn();
					Respawning = false;
					BOT = baseEntity;
					npc = baseEntity.GetComponent<ScientistNPC>();
					npc2 = baseEntity.GetComponent<ScarecrowNPC>();

					try
					{
						var navagent = baseEntity.GetComponent<NavMeshAgent>();
						if (navagent != null)
						{
							navagent.areaMask = num;
							navagent.agentTypeID = ((num == 25 || num == 8) ? 0 : -1372625422);
							if (stationary)
							{
								navagent.isStopped = true;
								navagent.enabled = false;
							}
						}
					}
					catch { }
				}
			}
			else
			{
				prefab = StringPool.Get(3763080634);
				var prefabvar = GameManager.server.FindPrefab(prefab);
				var go = Facepunch.Instantiate.GameObject(prefabvar);
				go.SetActive(false);
				go.name = prefab;
				go.transform.position = position;
				npc = go.GetComponent<ScientistNPC>();

				SceneManager.MoveGameObjectToScene(go, Rust.Server.EntityScene);
				if (npc != null)
				{
					npc.userID = (ulong)UnityEngine.Random.Range(0, 10000000);
					npc.UserIDString = npc.userID.ToString();
					npc.displayName = "[NPC]" + RandomUsernames.Get(npc.userID);
					npc.startHealth = 120;
					go.SetActive(true);
					npc.EnableSaving(false);
					npc.Spawn();
					Respawning = false;
					BOT = npc;

					try
					{
						var navagent = npc.GetComponent<NavMeshAgent>();
						if (navagent != null)
						{
							navagent.areaMask = num;
							navagent.agentTypeID = ((num == 25 || num == 8) ? 0 : -1372625422);
							if (stationary)
							{
								navagent.isStopped = true;
								navagent.enabled = false;
							}
						}
					}
					catch { }
				}
			}
			var player = (BasePlayer)null;

			if (npc != null) player = npc; else if (npc2 != null) player = npc2;

			switch (type)
			{
				case NPCType.Scientist:
					npc.InitializeHealth(module.DataInstance.NpcSpawner.ScientistNPCHealth, settings.ScientistNPCHealth);
					if (npc != null)
					{
						RoamDistance = settings.ScientistNPCRoamRange;
						npc.aimConeScale = settings.ScientistNPCAimMultiplyer;
						npc.damageScale = settings.ScientistNPCDamageScale;
						npc.Brain.CheckLOS = settings.ScientistNPCCheckLOS;
						npc.Brain.IgnoreSafeZonePlayers = settings.ScientistNPCIgnoreSafeZonePlayers;
						npc.Brain.SenseRange = settings.ScientistNPCSenseRange;
						npc.Brain.MemoryDuration = settings.ScientistNPCMemoryDuration; ;
						npc.Brain.TargetLostRange = settings.ScientistNPCTargetLostRange;
						npc.Brain.AttackRangeMultiplier = settings.ScientistNPCAttackRangeMultiplier;
						if (settings.UseRustEditNPCLogic) return;
						npc.inventory.Strip();
						module.DressNPC(npc, type);
					}
					break;
				case NPCType.Peacekeeper:
					player.InitializeHealth(settings.PeacekeeperNPCHealth, settings.PeacekeeperNPCHealth);
					if (npc != null)
					{
						RoamDistance = settings.PeacekeeperNPCRoamRange;
						npc.aimConeScale = settings.PeacekeeperNPCAimMultiplyer;
						npc.damageScale = settings.PeacekeeperNPCDamageScale;
						npc.Brain.CheckLOS = settings.PeacekeeperNPCCheckLOS;
						npc.Brain.IgnoreSafeZonePlayers = settings.PeacekeeperNPCIgnoreSafeZonePlayers;
						npc.Brain.SenseRange = settings.PeacekeeperNPCSenseRange;
						npc.Brain.MemoryDuration = settings.PeacekeeperNPCMemoryDuration; ;
						npc.Brain.TargetLostRange = settings.PeacekeeperNPCTargetLostRange;
						npc.Brain.AttackRangeMultiplier = settings.PeacekeeperNPCAttackRangeMultiplier;
						if (settings.UseRustEditNPCLogic) return;
						npc.inventory.Strip();
						module.DressNPC(npc, type);
					}
					break;
				case NPCType.HeavyScientist:
					player.InitializeHealth(settings.HeavyScientistNPCHealth, settings.HeavyScientistNPCHealth);
					if (npc != null)
					{
						RoamDistance = settings.HeavyScientistNPCRoamRange;
						npc.aimConeScale = settings.HeavyScientistNPCAimMultiplyer;
						npc.damageScale = settings.HeavyScientistNPCDamageScale;
						npc.Brain.CheckLOS = settings.HeavyScientistNPCCheckLOS;
						npc.Brain.IgnoreSafeZonePlayers = settings.HeavyScientistNPCIgnoreSafeZonePlayers;
						npc.Brain.SenseRange = settings.HeavyScientistNPCSenseRange;
						npc.Brain.MemoryDuration = settings.HeavyScientistNPCMemoryDuration; ;
						npc.Brain.TargetLostRange = settings.HeavyScientistNPCTargetLostRange;
						npc.Brain.AttackRangeMultiplier = settings.HeavyScientistNPCAttackRangeMultiplier;
						if (settings.UseRustEditNPCLogic) return;
						npc.inventory.Strip();
						module.DressNPC(npc, type);

					}
					break;
				case NPCType.JunkpileScientist:
					player.InitializeHealth(settings.JunkpileScientistNPCHealth, settings.JunkpileScientistNPCHealth);
					if (npc != null)
					{
						RoamDistance = settings.JunkpileScientistNPCRoamRange;
						npc.aimConeScale = settings.JunkpileScientistNPCAimMultiplyer;
						npc.damageScale = settings.JunkpileScientistNPCDamageScale;
						npc.Brain.CheckLOS = settings.JunkpileScientistNPCCheckLOS;
						npc.Brain.IgnoreSafeZonePlayers = settings.JunkpileScientistNPCIgnoreSafeZonePlayers;
						npc.Brain.SenseRange = settings.JunkpileScientistNPCSenseRange;
						npc.Brain.MemoryDuration = settings.JunkpileScientistNPCMemoryDuration; ;
						npc.Brain.TargetLostRange = settings.JunkpileScientistNPCTargetLostRange;
						npc.Brain.AttackRangeMultiplier = settings.JunkpileScientistNPCAttackRangeMultiplier;
						if (settings.UseRustEditNPCLogic) return;
						npc.inventory.Strip();
						module.DressNPC(npc, type);
					}
					break;
				case NPCType.Bandit:
					player.InitializeHealth(settings.BanditNPCHealth, settings.BanditNPCHealth);
					if (npc != null)
					{
						RoamDistance = settings.BanditNPCRoamRange;
						npc.aimConeScale = settings.BanditNPCAimMultiplyer;
						npc.damageScale = settings.BanditNPCDamageScale;
						npc.Brain.CheckLOS = settings.BanditNPCCheckLOS;
						npc.Brain.IgnoreSafeZonePlayers = settings.BanditNPCIgnoreSafeZonePlayers;
						npc.Brain.SenseRange = settings.BanditNPCSenseRange;
						npc.Brain.MemoryDuration = settings.BanditNPCMemoryDuration; ;
						npc.Brain.TargetLostRange = settings.BanditNPCTargetLostRange;
						npc.Brain.AttackRangeMultiplier = settings.BanditNPCAttackRangeMultiplier;

						if (settings.UseRustEditNPCLogic) return;

						npc.inventory.Strip();
						module.DressNPC(npc, type);
					}
					break;
				case NPCType.Murderer:
					player.InitializeHealth(settings.MurdererNPCHealth, settings.MurdererNPCHealth);
					if (npc2 != null)
					{
						RoamDistance = settings.MurdererNPCRoamRange;
						npc2.damageScale = settings.MurdererNPCDamageScale;
						npc2.Brain.CheckLOS = settings.MurdererNPCCheckLOS;
						npc2.Brain.IgnoreSafeZonePlayers = settings.MurdererNPCIgnoreSafeZonePlayers;
						npc2.Brain.SenseRange = settings.MurdererNPCSenseRange;
						npc2.Brain.MemoryDuration = settings.MurdererNPCMemoryDuration;
						npc2.Brain.TargetLostRange = settings.MurdererNPCTargetLostRange;
						npc2.Brain.AttackRangeMultiplier = settings.MurdererNPCAttackRangeMultiplier;
						npc2.inventory.Strip();
						module.DressNPC(npc2, type);
					}
					break;
				case NPCType.Scarecrow:
					player.InitializeHealth(settings.ScarecrowNPCHealth, settings.ScarecrowNPCHealth);
					if (npc2 != null)
					{
						RoamDistance = settings.ScarecrowNPCRoamRange;
						npc2.damageScale = settings.ScarecrowNPCDamageScale;
						npc2.Brain.CheckLOS = settings.ScarecrowNPCCheckLOS;
						npc2.Brain.IgnoreSafeZonePlayers = settings.ScarecrowNPCIgnoreSafeZonePlayers;
						npc2.Brain.SenseRange = settings.ScarecrowNPCSenseRange;
						npc2.Brain.MemoryDuration = settings.ScarecrowNPCMemoryDuration;
						npc2.Brain.TargetLostRange = settings.ScarecrowNPCTargetLostRange;
						npc2.Brain.AttackRangeMultiplier = settings.ScarecrowNPCAttackRangeMultiplier;
						npc2.inventory.Strip();
						module.DressNPC(npc2, type);
					}
					break;
			}
		}
	}

	private System.Collections.IEnumerator NPCAIFunction()
	{
		yield return CoroutineEx.waitForSeconds(5);

		var checks = 0;
		var _instruction = ConVar.FPS.limit > 30 ? CoroutineEx.waitForSeconds(0.01f) : CoroutineEx.waitForSeconds(0.001f);
		Puts($"AI thread running on {NPCsList.Count:n0} NPCs");

		while (NPCThread != null)
		{
			foreach (NPCSpawner npcs in NPCsList)
			{
				if (++checks >= 50)
				{
					checks = 0;
					yield return _instruction;
				}

				try
				{
					if (npcs == null) { continue; }
					//Respawn Logic
					if (npcs.Respawning)
					{
						if (npcs.Delay <= Time.time)
						{
							if (!AnyPlayersNearby(npcs.serializedNPCSpawner.position, DataInstance.NpcSpawner.SpawnBlockingDistance)) { npcs.SpawnNPC(); }
						}
						continue;
					}
					if ((npcs.BOT == null || npcs.BOT.IsDestroyed) && !npcs.Respawning)
					{
						npcs.Respawning = true;
						npcs.Delay = Time.time + (float)UnityEngine.Random.Range(npcs.serializedNPCSpawner.respawnMin, npcs.serializedNPCSpawner.respawnMax);
						continue;
					}
					if (DataInstance.NpcSpawner.UseRustEditNPCLogic || npcs.type == NPCType.Scarecrow || npcs.type == NPCType.Murderer)
					{
						//Rust Edit AI Logic and Scarecrow/murderer Fix
						ScarecrowNPC scnpc = npcs.BOT as ScarecrowNPC;
						if (scnpc != null)
						{
							if (npcs.target == null)
							{
								try { npcs.target = scnpc.Brain.Senses.GetNearestTarget(scnpc.Brain.SenseRange); }
								catch { npcs.target = null; }
							}
							if (npcs.target != null)
							{
								//If target out of range then clear target.
								if (Vector3.Distance(npcs.target.transform.position, scnpc.transform.position) >= scnpc.Brain.SenseRange)
								{
									scnpc.Brain.Events.Memory.Entity.Set(scnpc, 0);
									scnpc.Brain.Navigator.SetDestination(npcs.serializedNPCSpawner.position, BaseNavigator.NavigationSpeed.Normal);
									npcs.target = null;
								}
								else
								{
									scnpc.Brain.Events.Memory.Entity.Set(npcs.target, 0);
									float dist;
									if (scnpc.IsTargetInRange(npcs.target, out dist))
									{
										BaseMelee weapon = scnpc?.GetHeldEntity() as BaseMelee;
										if (weapon == null) { continue; }
										//Do melee slash if in its reach
										if (!weapon.HasAttackCooldown() && Vector3.Distance(npcs.target.transform.position, scnpc.transform.position) <= weapon.maxDistance)
										{
											//melee hit
											if (scnpc.MeleeAttack())
											{
												npcs.target.ToPlayer().Hurt(weapon.TotalDamage() * scnpc.damageScale, DamageType.Slash, scnpc);
												weapon.StartAttackCooldown(weapon.attackSpacing);
											}
										}
									}
								}
							}
							else
							{
								//No target so do random movement
								RandomMovement(npcs);
								npcs.target = null;
							}
						}
					}
					else
					{
						//bmgjet pirates logic
						ScientistNPC scinpc = npcs.BOT as ScientistNPC;
						if (scinpc == null) { continue; }
						try { if (scinpc.GetGun().primaryMagazine.contents < 1) { scinpc.AttemptReload(); } } catch { }
						if (npcs.target == null)
						{
							//Find targets in range if has no target
							try { npcs.target = scinpc.Brain.Senses.GetNearestTarget(scinpc.Brain.SenseRange); }
							catch { npcs.target = null; }
						}
						if (npcs.target != null)
						{
							//If taget is beyound sensor range return home and clear memory
							if (Vector3.Distance(npcs.target.transform.position, npcs.BOT.transform.position) > scinpc.Brain.SenseRange)
							{
								if (!npcs.cooldown)
								{
									npcs.target = null;
									scinpc.Brain.Events.Memory.Entity.Set(scinpc, 0);
								}
								if (!scinpc.Brain.Navigator.Moving)
								{
									scinpc.Brain.Navigator.SetDestination(npcs.serializedNPCSpawner.position, BaseNavigator.NavigationSpeed.Fast);
								}
							}
							else
							{
								//Chase target
								if (!npcs.cooldown)
								{
									scinpc.Brain.Events.Memory.Entity.Set(npcs.target, 0);
								}
								if (!scinpc.Brain.Navigator.Moving)
								{
									scinpc.Brain.Navigator.SetDestination(npcs.target.transform.position, BaseNavigator.NavigationSpeed.Normal);
								}
							}
						}
						else
						{
							//No target so do random movement
							RandomMovement(npcs);
							npcs.target = null;
						}
					}
				}
				catch { }
			}

			yield return CoroutineEx.waitForSeconds(DataInstance.NpcSpawner.AITickRate);
		}

		PutsWarn("Stopped AI processing.");
		yield break;
	}

	public bool AnyPlayersNearby(Vector3 position, float maxDist)
	{
		var list = Pool.GetList<BasePlayer>();
		Vis.Entities(position, maxDist, list, 131072);

		var result = false;
		foreach (BasePlayer basePlayer in list)
		{
			if (basePlayer.IsNpc || basePlayer.IsConnected)
			{
				continue;
			}
			if (!basePlayer.IsSleeping() && basePlayer.IsAlive())
			{
				result = true;
				break;
			}
		}
		Facepunch.Pool.FreeList(ref list);
		return result;
	}
	public void RandomMovement(NPCSpawner npcs)
	{
		var bn = (BaseNavigator)null;

		if (npcs.BOT is ScientistNPC scientist) { bn = scientist.Brain.Navigator; }
		if (npcs.BOT is ScarecrowNPC scarecrow) { bn = scarecrow.Brain.Navigator; }

		if (bn == null) return;

		if (!bn.Moving && !npcs.stationary)
		{
			var newPos = npcs.serializedNPCSpawner.position;
			var d = npcs.RoamDistance / 2f;
			var vector = UnityEngine.Random.insideUnitCircle * d;

			newPos += new Vector3(vector.x, 0, vector.y);
			bn.SetDestination(newPos, BaseNavigator.NavigationSpeed.Slow);
		}
	}

	public void DressNPC(BasePlayer npc, NPCType npctype)
	{
		if (npc == null) { return; }
		switch (npctype)
		{
			case NPCType.Scientist:
				CreateCloths(npc, "hazmatsuit_scientist_arctic");
				switch ((int)UnityEngine.Random.Range(0f, 2))
				{
					case 0:
						CreateGun(npc, "rifle.lr300", "weapon.mod.flashlight");
						break;
					case 1:
						CreateGun(npc, "rifle.ak", "weapon.mod.flashlight");
						break;
					case 2:
						CreateGun(npc, "pistol.m92", "weapon.mod.flashlight");
						break;
				}
				break;
			case NPCType.Peacekeeper:
				CreateCloths(npc, "hazmatsuit_scientist_peacekeeper");
				CreateGun(npc, "rifle.m39", "weapon.mod.flashlight");
				break;
			case NPCType.HeavyScientist:
				CreateCloths(npc, "scientistsuit_heavy");
				switch ((int)UnityEngine.Random.Range(0f, 2))
				{
					case 0:
						CreateGun(npc, "lmg.m249", "weapon.mod.flashlight");
						break;
					case 1:
						CreateGun(npc, "shotgun.spas12", "weapon.mod.flashlight");
						break;
					case 2:
						CreateGun(npc, "rifle.lr300", "weapon.mod.flashlight");
						break;
				}
				break;
			case NPCType.JunkpileScientist:
				CreateCloths(npc, "hazmatsuit_scientist");
				CreateGun(npc, "pistol.m92", "weapon.mod.flashlight");
				break;
			case NPCType.Bandit:
				CreateCloths(npc, "attire.banditguard");
				CreateGun(npc, "rifle.semiauto", "weapon.mod.flashlight");
				break;
			case NPCType.Murderer:
				CreateCloths(npc, "scarecrow.suit");
				CreateCloths(npc, "jacket");
				CreateGun(npc, "chainsaw");
				break;
			case NPCType.Scarecrow:
				CreateCloths(npc, "halloween.mummysuit");
				switch ((int)UnityEngine.Random.Range(0f, 2))
				{
					case 0:
						CreateGun(npc, "pitchfork");
						break;
					case 1:
						CreateGun(npc, "sickle");
						break;
					case 2:
						CreateGun(npc, "bone.clubsickle");
						break;
				}
				break;
		}
	}
	public void CreateCloths(BasePlayer npc, string itemName, ulong skin = 0)
	{
		if (npc == null) return;

		var item = ItemManager.CreateByName(itemName, 1, skin);
		if (item == null) return;

		if (!item.MoveToContainer(npc.inventory.containerWear, -1, false)) item.Remove();
	}
	public void CreateGun(BasePlayer npc, string itemName, string attachment = "", ulong skinid = 0)
	{
		var item = ItemManager.CreateByName(itemName, 1, skinid);
		if (item == null) return;

		if (!item.MoveToContainer(npc.inventory.containerBelt, -1, false)) { item.Remove(); return; }

		var be = item.GetHeldEntity();
		if (be != null && be is BaseProjectile)
		{
			if (!string.IsNullOrEmpty(attachment))
			{
				var moditem = ItemManager.CreateByName(attachment, 1, 0);
				if (moditem != null && item.contents != null)
				{
					if (!moditem.MoveToContainer(item.contents)) { item.contents.itemList.Add(moditem); }
				}
			}
		}

		DoFixes(npc, item);
	}
	public void DoFixes(BasePlayer npc, Item item)
	{
		//
		// Reload mods on guns
		//
		npc.Invoke(() =>
		{
			if (npc != null && item != null)
			{
				npc.UpdateActiveItem(item.uid);
			}
		}, 2f);

		//
		// Chainsaw fix
		//
		npc.Invoke(() =>
		{
			if (npc != null)
			{
				if (npc.GetHeldEntity() is Chainsaw chainsaw)
				{
					chainsaw.SetFlag(BaseEntity.Flags.On, true);
					chainsaw.SendNetworkUpdateImmediate();
				}
			}
		}, 4f);
	}

	public bool IsOnGround(ref Vector3 position, out int areamask)
	{
		NavMeshHit navMeshHit;
		bool result;
		if (NavMesh.SamplePosition(position, out navMeshHit, 8f, -1))
		{
			areamask = navMeshHit.mask;
			position = navMeshHit.position;
			result = true;
		}
		else
		{
			areamask = -1;
			result = false;
		}
		return result;
	}

	public byte[] GetData(string name)
	{
		foreach (MapData MD in World.Serialization.world.maps)
		{
			try
			{
				if (System.Text.Encoding.ASCII.GetString(MD.data).Contains(name))
				{
					return MD.data;
				}
			}
			catch { }
		}

		return null;
	}
	public byte[] Serialize<T>(T stream)
	{
		if (stream == null) return null;

		try
		{
			using (var memoryStream = new MemoryStream())
			{
				new XmlSerializer(typeof(T)).Serialize(memoryStream, stream);
				return memoryStream.ToArray();
			}
		}
		catch (Exception ex)
		{
			PutsError("Failed serialization.", ex);
		}

		return null;
	}
	public T Deserialize<T>(byte[] bytes)
	{
		try
		{
			using (var memoryStream = new MemoryStream(bytes))
			{
				var xmlSerializer = new XmlSerializer(typeof(T));
				return (T)xmlSerializer.Deserialize(memoryStream);
			}
		}
		catch (Exception ex)
		{
			PutsError("Failed deserialization.", ex);
		}

		return default;
	}

	#endregion
}

public class RustEditConfig
{
}
public class RustEditData
{
	public NpcSettings NpcSpawner = new NpcSettings();

	public class NpcSettings
	{
		public bool UseRustEditNPCLogic = false;
		public float AITickRate = 2f;
		public float SpawnBlockingDistance = 30f;
		public bool APCIgnoreNPCs = true;
		public bool NPCAutoTurretsIgnoreNPCs = true;
		public float ScientistNPCHealth = 120;
		public float ScientistNPCAimMultiplyer = 0.8f;
		public float ScientistNPCRoamRange = 50;
		public bool ScientistNPCIgnoreSafeZonePlayers = true;
		public bool ScientistNPCCheckLOS = true;
		public float ScientistNPCDamageScale = 0.4f;
		public float ScientistNPCSenseRange = 40f;
		public float ScientistNPCMemoryDuration = 1f;
		public float ScientistNPCTargetLostRange = 80f;
		public float ScientistNPCAttackRangeMultiplier = 1f;
		public float PeacekeeperNPCHealth = 120;
		public float PeacekeeperNPCAimMultiplyer = 0.8f;
		public float PeacekeeperNPCRoamRange = 30;
		public bool PeacekeeperNPCIgnoreSafeZonePlayers = true;
		public bool PeacekeeperNPCCheckLOS = true;
		public float PeacekeeperNPCDamageScale = 0.4f;
		public float PeacekeeperNPCSenseRange = 30f;
		public float PeacekeeperNPCMemoryDuration = 1f;
		public float PeacekeeperNPCTargetLostRange = 80f;
		public float PeacekeeperNPCAttackRangeMultiplier = 1f;
		public float HeavyScientistNPCHealth = 190;
		public float HeavyScientistNPCAimMultiplyer = 0.8f;
		public float HeavyScientistNPCRoamRange = 50;
		public bool HeavyScientistNPCIgnoreSafeZonePlayers = true;
		public bool HeavyScientistNPCCheckLOS = true;
		public float HeavyScientistNPCDamageScale = 0.6f;
		public float HeavyScientistNPCSenseRange = 30f;
		public float HeavyScientistNPCMemoryDuration = 1f;
		public float HeavyScientistNPCTargetLostRange = 80f;
		public float HeavyScientistNPCAttackRangeMultiplier = 1f;
		public float JunkpileScientistNPCHealth = 120;
		public float JunkpileScientistNPCAimMultiplyer = 0.8f;
		public float JunkpileScientistNPCRoamRange = 50;
		public bool JunkpileScientistNPCIgnoreSafeZonePlayers = true;
		public bool JunkpileScientistNPCCheckLOS = true;
		public float JunkpileScientistNPCDamageScale = 0.4f;
		public float JunkpileScientistNPCSenseRange = 20f;
		public float JunkpileScientistNPCMemoryDuration = 1f;
		public float JunkpileScientistNPCTargetLostRange = 80f;
		public float JunkpileScientistNPCAttackRangeMultiplier = 1f;
		public float BanditNPCHealth = 120;
		public float BanditNPCAimMultiplyer = 0.8f;
		public float BanditNPCRoamRange = 50;
		public bool BanditNPCIgnoreSafeZonePlayers = true;
		public bool BanditNPCCheckLOS = true;
		public float BanditNPCDamageScale = 0.4f;
		public float BanditNPCSenseRange = 40f;
		public float BanditNPCMemoryDuration = 1f;
		public float BanditNPCTargetLostRange = 80f;
		public float BanditNPCAttackRangeMultiplier = 1f;
		public float MurdererNPCHealth = 100;
		public float MurdererNPCRoamRange = 30;
		public bool MurdererNPCIgnoreSafeZonePlayers = true;
		public bool MurdererNPCCheckLOS = true;
		public float MurdererNPCDamageScale = 0.4f;
		public float MurdererNPCSenseRange = 20f;
		public float MurdererNPCMemoryDuration = 1f;
		public float MurdererNPCTargetLostRange = 40f;
		public float MurdererNPCAttackRangeMultiplier = 1f;
		public float ScarecrowNPCHealth = 60;
		public float ScarecrowNPCRoamRange = 30;
		public bool ScarecrowNPCIgnoreSafeZonePlayers = true;
		public bool ScarecrowNPCCheckLOS = true;
		public float ScarecrowNPCDamageScale = 0.4f;
		public float ScarecrowNPCSenseRange = 20f;
		public float ScarecrowNPCMemoryDuration = 1f;
		public float ScarecrowNPCTargetLostRange = 40f;
		public float ScarecrowNPCAttackRangeMultiplier = 1f;
	}
}
