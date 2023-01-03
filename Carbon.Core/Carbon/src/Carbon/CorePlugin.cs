using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Carbon.Base.Interfaces;
using Carbon.Components;
using Carbon.Extensions;
using Carbon.Hooks;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Plugins;
using UnityEngine;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Core;

public class CorePlugin : RustPlugin
{
	public static Dictionary<string, string> OrderedFiles { get; } = new Dictionary<string, string>();

	public static void RefreshOrderedFiles()
	{
		OrderedFiles.Clear();

		foreach (var file in OsEx.Folder.GetFilesWithExtension(Defines.GetPluginsFolder(), "cs"))
		{
			OrderedFiles.Add(Path.GetFileNameWithoutExtension(file), file);
		}
	}

	public static string GetPluginPath(string shortName)
	{
		foreach (var file in OrderedFiles)
		{
			if (file.Key == shortName) return file.Value;
		}

		return null;
	}

	public override void IInit()
	{
		foreach (var player in BasePlayer.activePlayerList)
		{
			permission.RefreshUser(player);
		}

		Community.Runtime.ModuleProcessor.Init();

		timer.Every(5f, () =>
		{
			if (!Logger._file._hasInit || Logger._file._buffer.Count == 0 || Community.Runtime.Config.LogFileMode != 1) return;
			Logger._file._flush();
		});
	}

	private void OnPluginLoaded(Plugin plugin)
	{
	}
	private void OnPluginUnloaded(Plugin plugin)
	{
	}
	private void OnPlayerConnected(BasePlayer player)
	{
		permission.RefreshUser(player);
	}
	private void OnEntitySpawned(BaseEntity entity)
	{
		Entities.AddMap(entity);
	}
	private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
	{
		Entities.RemoveMap(entity);
	}
	private void OnEntityKill(BaseEntity entity)
	{
		Entities.RemoveMap(entity);
	}

	internal static void Reply(object message, ConsoleSystem.Arg arg)
	{
		if (arg != null && arg.Player() != null)
		{
			arg.Player().SendConsoleCommand($"echo {message}");
			return;
		}
		Carbon.Logger.Log(message);
	}

	[ConsoleCommand("exit", "Completely unloads Carbon from the game, rendering it fully vanilla.")]
	private void Exit(ConsoleSystem.Arg arg)
	{
		Supervisor.ASM.UnloadModule("Carbon.dll", false);
	}

	[ConsoleCommand("reboot", "Unloads Carbon from the game and then loads it back again with the latest version changes (if any).")]
	private void Reboot(ConsoleSystem.Arg arg)
	{
		Supervisor.ASM.UnloadModule("Carbon.dll", true);
	}

	[ConsoleCommand("version", "Returns currently loaded version of Carbon.")]
	private void GetVersion(ConsoleSystem.Arg arg)
	{
		Reply($"Carbon v{Community.Version}", arg);
	}

	[ConsoleCommand("build", "Returns current version of Carbon's Assembly.")]
	private void GetBuild(ConsoleSystem.Arg arg)
	{
		Reply($"{Community.InformationalVersion}", arg);
	}

	[ConsoleCommand("plugins", "Prints the list of mods and their loaded plugins.")]
	private void Plugins(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		var mode = arg.HasArgs(1) ? arg.Args[0] : null;

		switch (mode)
		{
			case "-j":
			case "--j":
			case "-json":
			case "--json":
				Reply(JsonConvert.SerializeObject(Loader._loadedMods, Formatting.Indented), arg);
				break;

			default:
				var body = new StringTable("#", "Mod", "Author", "Version", "Core", "Hook Time", "Compile Time");
				var count = 1;

				foreach (var mod in Loader._loadedMods)
				{
					body.AddRow($"{count:n0}", $"{mod.Name}{(mod.Plugins.Count > 1 ? $" ({mod.Plugins.Count:n0})" : "")}", "", "", mod.IsCoreMod ? "Yes" : "No", "", "");

					foreach (var plugin in mod.Plugins)
					{
						body.AddRow($"", plugin.Name, plugin.Author, $"v{plugin.Version}", plugin.IsCorePlugin ? "Yes" : "No", $"{plugin.TotalHookTime:0.0}s", $"{plugin.CompileTime:0.0}ms");
					}

					count++;
				}

				Reply(body.ToStringMinimal(), arg);
				break;
		}
	}

	[ConsoleCommand("pluginsfailed", "Prints the list of plugins that failed to load (most likely due to compilation issues).")]
	private void PluginsFailed(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		var mode = arg.HasArgs(1) ? arg.Args[0] : null;

		switch (mode)
		{
			case "-j":
			case "--j":
			case "-json":
			case "--json":
				Reply(JsonConvert.SerializeObject(Loader._failedMods, Formatting.Indented), arg);
				break;

			default:
				var result = string.Empty;
				var count = 1;

				foreach (var mod in Loader._failedMods)
				{
					result += $"{count:n0}. {mod.File}\n";

					foreach (var error in mod.Errors)
					{
						result += $" {error}\n";
					}

					result += "\n";
					count++;
				}

				Reply(result, arg);
				break;
		}
	}

#if DEBUG
	[ConsoleCommand("assembly", "Debug stuff.")]
	private void AssemblyInfo(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		int count = 0;
		StringTable body = new StringTable("#", "Assembly", "Version", "Dynamic", "Location");
		foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			body.AddRow($"{count++:n0}", assembly.GetName().Name, assembly.GetName().Version, assembly.IsDynamic, (assembly.IsDynamic) ? string.Empty : assembly.Location);
		Reply(body.ToStringMinimal(), arg);
	}
#endif

	[ConsoleCommand("hooks", "Prints the list of all hooks that have been called at least once.")]
	private void HookInfo(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		StringTable body = new StringTable("#", "Hook", "Identifier", "Type", "Status", "Current", "Total", "Subscribers");
		int count = 0, success = 0, warning = 0, failure = 0;

		string option1 = arg.GetString(0, null);
		string option2 = arg.GetString(1, null);

		switch (option1)
		{
			case "enable":
				Community.Runtime.HookProcessorEx.enabled = true;
				break;

			case "disable":
				// FIXME : Currently all static hooks go away which means
				// no moar console commands can be issued :rocket:
				Community.Runtime.HookProcessorEx.enabled = false;
				break;

			case "reload":
				Community.Runtime.HookProcessorEx.Reload();
				break;

			case "update":
				try
				{
					Carbon.Hooks.Updater.DoUpdate((bool result) =>
					{
						if (!result)
						{
							Logger.Error($"Unknown error while updating hooks");
							return;
						}
						HookCaller.CallStaticHook("OnServerSave");
						Community.Runtime.HookProcessorEx.Reload();
					});
				}
				catch (System.Exception e)
				{
					Logger.Error($"Error while updating hooks", e);
				}
				break;

			case "loaded":
				{
					IEnumerable<HookEx> hooks;

					switch (option2)
					{
						case "--static":
							hooks = Community.Runtime.HookProcessorEx.LoadedStaticHooks.Where(x => !x.IsHidden);
							break;

						case "--dynamic":
							hooks = Community.Runtime.HookProcessorEx.LoadedDynamicHooks.Where(x => !x.IsHidden);
							break;

						case "--failed":
							hooks = Community.Runtime.HookProcessorEx.LoadedStaticHooks
								.Where(x => !x.IsHidden && x.Status == HookState.Failure);
							hooks = hooks.Concat(Community.Runtime.HookProcessorEx.LoadedDynamicHooks
								.Where(x => !x.IsHidden && x.Status == HookState.Failure));
							break;

						case "--warning":
							hooks = Community.Runtime.HookProcessorEx.LoadedStaticHooks
								.Where(x => !x.IsHidden && x.Status == HookState.Warning);
							hooks = hooks.Concat(Community.Runtime.HookProcessorEx.LoadedDynamicHooks
								.Where(x => !x.IsHidden && x.Status == HookState.Warning));
							break;

						case "--success":
							hooks = Community.Runtime.HookProcessorEx.LoadedStaticHooks
								.Where(x => !x.IsHidden && x.Status == HookState.Success);
							hooks = hooks.Concat(Community.Runtime.HookProcessorEx.LoadedDynamicHooks
								.Where(x => !x.IsHidden && x.Status == HookState.Success));
							break;

						default:
							hooks = Community.Runtime.HookProcessorEx.LoadedStaticHooks.Where(x => !x.IsHidden);
							hooks = hooks.Concat(Community.Runtime.HookProcessorEx.LoadedDynamicHooks.Where(x => !x.IsHidden));
							break;
					}

					foreach (var mod in hooks.OrderBy(x => x.HookName))
					{
						if (mod.Status == HookState.Failure) failure++;
						if (mod.Status == HookState.Success) success++;
						if (mod.Status == HookState.Warning) warning++;

						body.AddRow($"{count++:n0}", mod.HookName, mod.Identifier.Substring(0, 6), mod.IsStaticHook ? "Static" : "Dynamic", mod.Status, $"{HookCaller.GetHookTime(mod.HookName)}ms",
							$"{HookCaller.GetHookTotalTime(mod.HookName)}ms", $"{Community.Runtime.HookProcessorEx.GetHookSubscriberCount(mod.HookName)}");
					}

					Reply(body.ToStringMinimal(), arg);
					Reply($"total:{count} success:{success} warning:{warning} failed:{failure}", arg);
					break;
				}

			default: // list installed
				{
					IEnumerable<HookEx> hooks;

					switch (option1)
					{
						case "--static":
							hooks = Community.Runtime.HookProcessorEx.InstalledStaticHooks.Where(x => !x.IsHidden);
							break;

						case "--dynamic":
							hooks = Community.Runtime.HookProcessorEx.InstalledDynamicHooks.Where(x => !x.IsHidden);
							break;

						case "--warning":
							hooks = Community.Runtime.HookProcessorEx.InstalledStaticHooks
								.Where(x => !x.IsHidden && x.Status == HookState.Warning);
							hooks = hooks.Concat(Community.Runtime.HookProcessorEx.InstalledDynamicHooks
								.Where(x => !x.IsHidden && x.Status == HookState.Warning));
							break;

						case "--success":
							hooks = Community.Runtime.HookProcessorEx.InstalledStaticHooks
								.Where(x => !x.IsHidden && x.Status == HookState.Success);
							hooks = hooks.Concat(Community.Runtime.HookProcessorEx.InstalledDynamicHooks
								.Where(x => !x.IsHidden && x.Status == HookState.Success));
							break;

						default:
							hooks = Community.Runtime.HookProcessorEx.InstalledStaticHooks.Where(x => !x.IsHidden);
							hooks = hooks.Concat(Community.Runtime.HookProcessorEx.InstalledDynamicHooks.Where(x => !x.IsHidden));
							break;
					}

					foreach (var mod in hooks.OrderBy(x => x.HookName))
					{
						if (mod.Status == HookState.Failure) failure++;
						if (mod.Status == HookState.Success) success++;
						if (mod.Status == HookState.Warning) warning++;

						body.AddRow($"{count++:n0}", mod.HookName, mod.Identifier.Substring(0, 6), mod.IsStaticHook ? "Static" : "Dynamic", mod.Status, $"{HookCaller.GetHookTime(mod.HookName)}ms",
							$"{HookCaller.GetHookTotalTime(mod.HookName)}ms", $"{Community.Runtime.HookProcessorEx.GetHookSubscriberCount(mod.HookName)}");
					}

					Reply(body.ToStringMinimal(), arg);
					Reply($"total:{count} success:{success} warning:{warning} failed:{failure}", arg);
					break;
				}
		}
	}

	[ConsoleCommand("update", "Downloads, updates, saves the server and patches Carbon at runtime. (Eg. c.update win develop, c.update unix prod)")]
	private void Update(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		Carbon.Core.Updater.DoUpdate((bool result) =>
		{
			if (!result)
			{
				Logger.Error($"Unknown error while updating Carbon");
				return;
			}
			HookCaller.CallStaticHook("OnServerSave");
			Supervisor.ASM.UnloadModule("Carbon.dll", true);
		});
	}

	#region Config

	[ConsoleCommand("loadconfig", "Loads Carbon config from file.")]
	private void CarbonLoadConfig(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin() || Community.Runtime == null) return;

		Community.Runtime.LoadConfig();

		Reply("Loaded Carbon config.", arg);
	}

	[ConsoleCommand("saveconfig", "Saves Carbon config to file.")]
	private void CarbonSaveConfig(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin() || Community.Runtime == null) return;

		Community.Runtime.SaveConfig();

		Reply("Saved Carbon config.", arg);
	}

	[CommandVar("modding", "Mark this server as modded or not.", true)]
	private bool Modding { get { return Community.Runtime.Config.IsModded; } set { Community.Runtime.Config.IsModded = value; Community.Runtime.SaveConfig(); } }

	[CommandVar("tag", "Displays this server in the browser list with the 'carbon' tag.", true)]
	private bool CarbonTag { get { return Community.Runtime.Config.CarbonTag; } set { Community.Runtime.Config.CarbonTag = value; Community.Runtime.SaveConfig(); } }

	[CommandVar("debug", "The level of debug logging for Carbon. Helpful for very detailed logs in case things break. (Set it to -1 to disable debug logging.)", true)]
	private int CarbonDebug { get { return Community.Runtime.Config.LogVerbosity; } set { Community.Runtime.Config.LogVerbosity = value; Community.Runtime.SaveConfig(); } }

	[CommandVar("logfiletype", "The mode for writing the log to file. (0=disabled, 1=saves updates every 5 seconds, 2=saves immediately)", true)]
	private int LogFileType { get { return Community.Runtime.Config.LogFileMode; } set { Community.Runtime.Config.LogFileMode = Mathf.Clamp(value, 0, 2); Community.Runtime.SaveConfig(); } }

	[CommandVar("hooktimetracker", "For debugging purposes, this will track the time of hooks and gives a total.", true)]
	private bool HookTimeTracker { get { return Community.Runtime.Config.HookTimeTracker; } set { Community.Runtime.Config.HookTimeTracker = value; Community.Runtime.SaveConfig(); } }

	[CommandVar("hookvalidation", "Prints a warning when plugins contain Oxide hooks that aren't available yet in Carbon.", true)]
	private bool HookValidation { get { return Community.Runtime.Config.HookValidation; } set { Community.Runtime.Config.HookValidation = value; Community.Runtime.SaveConfig(); } }

	[CommandVar("entitymapbuffersize", "The entity map buffer size. Gets applied on Carbon reboot.", true)]
	private int EntityMapBufferSize { get { return Community.Runtime.Config.EntityMapBufferSize; } set { Community.Runtime.Config.EntityMapBufferSize = value; Community.Runtime.SaveConfig(); } }

	[CommandVar("language", "Server language used by the Language API.", true)]
	private string Language { get { return Community.Runtime.Config.Language; } set { Community.Runtime.Config.Language = value; Community.Runtime.SaveConfig(); } }

#if WIN
	[CommandVar("consoleinfo", "Show the Windows-only Carbon information at the bottom of the console.", true)]
	private bool ConsoleInfo
	{
		get { return Community.Runtime.Config.ShowConsoleInfo; }
		set
		{
			Community.Runtime.Config.ShowConsoleInfo = value;

			if (value)
			{
				Community.Runtime.RefreshConsoleInfo();
			}
			else
			{
				if (ServerConsole.Instance != null && ServerConsole.Instance.input != null)
				{
					ServerConsole.Instance.input.statusText = new string[3];
				}
			}
		}
	}
#endif

	#endregion

	#region Commands

	[ConsoleCommand("find", "Searches through Carbon-processed console commands.")]
	private void Find(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		var body = new StringTable("Command", "Value", "Help");
		var filter = arg.Args != null && arg.Args.Length > 0 ? arg.Args[0] : null;

		foreach (var command in Community.Runtime.AllConsoleCommands)
		{
			if (!string.IsNullOrEmpty(filter) && !command.Command.Contains(filter)) continue;

			var value = " ";

			if (command.Reference != null)
			{
				if (command.Reference is FieldInfo field) value = field.GetValue(command.Plugin)?.ToString();
				else if (command.Reference is PropertyInfo property) value = property.GetValue(command.Plugin)?.ToString();
			}

			body.AddRow(command.Command, value, command.Help);
		}

		Reply($"Console Commands:\n{body.ToStringMinimal()}", arg);
	}

	[ConsoleCommand("findchat", "Searches through Carbon-processed chat commands.")]
	private void FindChat(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		var body = new StringTable("Command", "Help");
		var filter = arg.Args != null && arg.Args.Length > 0 ? arg.Args[0] : null;

		foreach (var command in Community.Runtime.AllChatCommands)
		{
			if (!string.IsNullOrEmpty(filter) && !command.Command.Contains(filter)) continue;

			body.AddRow(command.Command, command.Help);
		}

		Reply($"Chat Commands:\n{body.ToStringMinimal()}", arg);
	}

	#endregion

	#region Report

	[ConsoleCommand("report", "Reloads all current plugins, and returns a report based on them at the output path.")]
	private void Report(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		new Report().Init();
	}

	#endregion

	#region Modules

	[ConsoleCommand("setmodule", "Enables or disables Carbon modules. Visit root/carbon/modules and use the config file names as IDs.")]
	private void SetModule(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin() || !arg.HasArgs(2)) return;

		var hookable = Community.Runtime.ModuleProcessor.Modules.FirstOrDefault(x => x.Name == arg.Args[0]);
		var module = hookable.To<IModule>();

		if (module == null)
		{
			Reply($"Couldn't find that module.", arg);
			return;
		}

		var previousEnabled = module.GetEnabled();
		var newEnabled = arg.Args[1].ToBool();

		if (previousEnabled != newEnabled)
		{
			module.SetEnabled(newEnabled);
			module.Save();
		}

		Reply($"{module.Name} marked {(module.GetEnabled() ? "enabled" : "disabled")}.", arg);
	}

	[ConsoleCommand("saveallmodules", "Saves the configs and data files of all available modules.")]
	private void SaveAllModules(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		foreach (var hookable in Community.Runtime.ModuleProcessor.Modules)
		{
			var module = hookable.To<IModule>();
			module.Save();
		}

		Reply($"Saved {Community.Runtime.ModuleProcessor.Modules.Count:n0} module configs and data files.", arg);
	}

	[ConsoleCommand("savemoduleconfig", "Saves Carbon module config & data file.")]
	private void SaveModuleConfig(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin() || !arg.HasArgs(1)) return;

		var hookable = Community.Runtime.ModuleProcessor.Modules.FirstOrDefault(x => x.Name == arg.Args[0]);
		var module = hookable.To<IModule>();

		if (module == null)
		{
			Reply($"Couldn't find that module.", arg);
			return;
		}

		module.Save();

		Reply($"Saved '{module.Name}' module config & data file.", arg);
	}

	[ConsoleCommand("loadmoduleconfig", "Loads Carbon module config & data file.")]
	private void LoadModuleConfig(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin() || !arg.HasArgs(1)) return;

		var hookable = Community.Runtime.ModuleProcessor.Modules.FirstOrDefault(x => x.Name == arg.Args[0]);
		var module = hookable.To<IModule>();

		if (module == null)
		{
			Reply($"Couldn't find that module.", arg);
			return;
		}

		if (module.GetEnabled()) module.SetEnabled(false);
		module.Load();
		if (module.GetEnabled()) module.OnEnableStatus();

		Reply($"Reloaded '{module.Name}' module config.", arg);
	}

	#endregion

	#region Mod & Plugin Loading

	[ConsoleCommand("reload", "Reloads all or specific mods / plugins. E.g 'c.reload *' to reload everything.")]
	private void Reload(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin() || !arg.HasArgs(1)) return;

		RefreshOrderedFiles();

		var name = arg.Args[0];
		switch (name)
		{
			case "*":
				Community.ClearPlugins();
				Community.ReloadPlugins();
				break;

			default:
				var path = GetPluginPath(name);

				if (!string.IsNullOrEmpty(path))
				{
					Community.Runtime.HarmonyProcessor.Prepare(name, path);
					Community.Runtime.ScriptProcessor.Prepare(name, path);
					return;
				}

				foreach (var mod in Loader._loadedMods)
				{
					var plugins = Pool.GetList<RustPlugin>();
					plugins.AddRange(mod.Plugins);

					foreach (var plugin in plugins)
					{
						if (plugin.Name == name)
						{
							plugin._processor_instance.Dispose();
							plugin._processor_instance.Execute();
							mod.Plugins.Remove(plugin);
						}
					}

					Pool.FreeList(ref plugins);
				}
				break;
		}
	}

	[ConsoleCommand("load", "Loads all mods and/or plugins. E.g 'c.load *' to load everything you've unloaded.")]
	private void LoadPlugin(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin() || !arg.HasArgs(1)) return;

		RefreshOrderedFiles();

		var name = arg.Args[0];
		switch (name)
		{
			case "*":
				//
				// Mods
				//
				{
					var tempList = Pool.GetList<string>();
					tempList.AddRange(Community.Runtime.HarmonyProcessor.IgnoreList);
					Community.Runtime.HarmonyProcessor.IgnoreList.Clear();

					foreach (var plugin in tempList)
					{
						Community.Runtime.HarmonyProcessor.Prepare(plugin, plugin);
					}
					Pool.FreeList(ref tempList);
				}

				//
				// Scripts
				//
				{
					var tempList = Pool.GetList<string>();
					tempList.AddRange(Community.Runtime.ScriptProcessor.IgnoreList);
					Community.Runtime.ScriptProcessor.IgnoreList.Clear();

					foreach (var plugin in tempList)
					{
						Community.Runtime.ScriptProcessor.Prepare(plugin, plugin);
					}
					Pool.FreeList(ref tempList);
				}
				break;

			default:
				var path = GetPluginPath(name);
				if (!string.IsNullOrEmpty(path))
				{
					Community.Runtime.HarmonyProcessor.ClearIgnore(path);
					Community.Runtime.ScriptProcessor.ClearIgnore(path);

					Community.Runtime.HarmonyProcessor.Prepare(path);
					Community.Runtime.ScriptProcessor.Prepare(path);
					return;
				}

				/*var module = BaseModule.GetModule<DRMModule>();
				foreach (var drm in module.Config.DRMs)
				{
					foreach (var entry in drm.Entries)
					{
						if (entry.Id == name) drm.RequestEntry(entry);
					}
				}*/

				break;
		}
	}

	[ConsoleCommand("unload", "Unloads all mods and/or plugins. E.g 'c.unload *' to unload everything. They'll be marked as 'ignored'.")]
	private void UnloadPlugin(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin() || !arg.HasArgs(1)) return;

		RefreshOrderedFiles();

		var name = arg.Args[0];
		switch (name)
		{
			case "*":
				//
				// Mods
				//
				{
					foreach (var plugin in Community.Runtime.HarmonyProcessor.InstanceBuffer)
					{
						Community.Runtime.HarmonyProcessor.Ignore(plugin.Value.File);
					}
					Community.Runtime.HarmonyProcessor.Clear();
				}

				//
				// Scripts
				//
				{
					var tempList = Pool.GetList<string>();
					tempList.AddRange(Community.Runtime.ScriptProcessor.IgnoreList);
					Community.Runtime.ScriptProcessor.IgnoreList.Clear();
					Community.Runtime.ScriptProcessor.Clear();

					foreach (var plugin in tempList)
					{
						Community.Runtime.ScriptProcessor.Ignore(plugin);
					}
					Pool.FreeList(ref tempList);
				}

				//
				// Web-Scripts
				//
				{
					var tempList = Pool.GetList<string>();
					tempList.AddRange(Community.Runtime.WebScriptProcessor.IgnoreList);
					Community.Runtime.WebScriptProcessor.IgnoreList.Clear();
					Community.Runtime.WebScriptProcessor.Clear();

					foreach (var plugin in tempList)
					{
						Community.Runtime.WebScriptProcessor.Ignore(plugin);
					}
					Pool.FreeList(ref tempList);
				}
				break;

			default:
				var path = GetPluginPath(name);
				if (!string.IsNullOrEmpty(path))
				{
					Community.Runtime.HarmonyProcessor.Ignore(path);
					Community.Runtime.ScriptProcessor.Ignore(path);
					Community.Runtime.WebScriptProcessor.Ignore(path);
				}

				foreach (var mod in Loader._loadedMods)
				{
					var plugins = Pool.GetList<RustPlugin>();
					plugins.AddRange(mod.Plugins);

					foreach (var plugin in plugins)
					{
						if (plugin.Name == name)
						{
							plugin._processor_instance.Dispose();
							mod.Plugins.Remove(plugin);
						}
					}

					Pool.FreeList(ref plugins);
				}
				break;
		}
	}

	#endregion

	#region Permissions

	[ConsoleCommand("grant", "Grant one or more permissions to users or groups. Do 'c.grant' for syntax info.")]
	private void Grant(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		void PrintWarn()
		{
			Reply($"Syntax: c.grant <user|group> <name|id> <perm>", arg);
		}

		if (!arg.HasArgs(3))
		{
			PrintWarn();
			return;
		}

		var action = arg.Args[0];
		var name = arg.Args[1];
		var perm = arg.Args[2];
		var user = permission.FindUser(name);

		switch (action)
		{
			case "user":
				if (permission.GrantUserPermission(user.Key, perm, null))
				{
					Reply($"Granted user '{user.Value.LastSeenNickname}' permission '{perm}'", arg);
				}
				break;

			case "group":
				if (permission.GrantGroupPermission(name, perm, null))
				{
					Reply($"Granted group '{name}' permission '{perm}'", arg);
				}
				break;

			default:
				PrintWarn();
				break;
		}
	}

	[ConsoleCommand("revoke", "Revoke one or more permissions from users or groups. Do 'c.revoke' for syntax info.")]
	private void Revoke(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		void PrintWarn()
		{
			Reply($"Syntax: c.revoke <user|group> <name|id> <perm>", arg);
		}

		if (!arg.HasArgs(3))
		{
			PrintWarn();
			return;
		}

		var action = arg.Args[0];
		var name = arg.Args[1];
		var perm = arg.Args[2];
		var user = permission.FindUser(name);

		switch (action)
		{
			case "user":
				if (permission.RevokeUserPermission(user.Key, perm))
				{
					Reply($"Revoked user '{user.Value?.LastSeenNickname}' permission '{perm}'", arg);
				}
				break;

			case "group":
				if (permission.RevokeGroupPermission(name, perm))
				{
					Reply($"Revoked group '{name}' permission '{perm}'", arg);
				}
				break;

			default:
				PrintWarn();
				break;
		}
	}

	[ConsoleCommand("show", "Displays information about a specific player or group (incl. permissions, groups and user list). Do 'c.show' for syntax info.")]
	private void Show(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		void PrintWarn()
		{
			Reply($"Syntax: c.show <user|group> <name|id>", arg);
		}

		if (!arg.HasArgs(2))
		{
			PrintWarn();
			return;
		}

		var action = arg.Args[0];
		var name = arg.Args[1];

		switch (action)
		{
			case "user":
				var user = permission.FindUser(name);
				if (user.Value == null)
				{
					Reply($"Couldn't find that user.", arg);
					return;
				}

				Reply($"User {user.Value.LastSeenNickname}[{user.Key}] found in {user.Value.Groups.Count:n0} groups:\n  {user.Value.Groups.Select(x => x).ToArray().ToString(", ", " and ")}", arg);
				Reply($"and has {user.Value.Perms.Count:n0} permissions:\n  {user.Value.Perms.Select(x => x).ToArray().ToString(", ", " and ")}", arg);
				break;

			case "group":
				if (!permission.GroupExists(name))
				{
					Reply($"Couldn't find that group.", arg);
					return;
				}

				var users = permission.GetUsersInGroup(name);
				var permissions = permission.GetGroupPermissions(name, false);
				Reply($"Group {name} has {users.Length:n0} users:\n  {users.Select(x => x).ToArray().ToString(", ", " and ")}", arg);
				Reply($"and has {permissions.Length:n0} permissions:\n  {permissions.Select(x => x).ToArray().ToString(", ", " and ")}", arg);
				break;

			default:
				PrintWarn();
				break;
		}
	}

	[ConsoleCommand("usergroup", "Adds or removes a player from a group. Do 'c.usergroup' for syntax info.")]
	private void UserGroup(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		void PrintWarn()
		{
			Reply($"Syntax: c.usergroup <add|remove> <player> <group>", arg);
		}

		if (!arg.HasArgs(3))
		{
			PrintWarn();
			return;
		}

		var action = arg.Args[0];
		var player = arg.Args[1];
		var group = arg.Args[2];

		var user = permission.FindUser(player);

		if (user.Value == null)
		{
			Reply($"Couldn't find that player.", arg);
			return;
		}

		if (!permission.GroupExists(group))
		{
			Reply($"Group '{group}' could not be found.", arg);
			return;
		}

		switch (action)
		{
			case "add":
				if (permission.UserHasGroup(user.Key, group))
				{
					Reply($"{user.Value.LastSeenNickname}[{user.Key}] is already in '{group}' group.", arg);
					return;
				}

				permission.AddUserGroup(user.Key, group);
				Reply($"Added {user.Value.LastSeenNickname}[{user.Key}] to '{group}' group.", arg);
				break;

			case "remove":
				if (!permission.UserHasGroup(user.Key, group))
				{
					Reply($"{user.Value.LastSeenNickname}[{user.Key}] isn't in '{group}' group.", arg);
					return;
				}

				permission.RemoveUserGroup(user.Key, group);
				Reply($"Removed {user.Value.LastSeenNickname}[{user.Key}] from '{group}' group.", arg);
				break;

			default:
				PrintWarn();
				break;
		}
	}

	[ConsoleCommand("group", "Adds or removes a group. Do 'c.group' for syntax info.")]
	private void Group(ConsoleSystem.Arg arg)
	{
		if (!arg.IsPlayerCalledAndAdmin()) return;

		void PrintWarn()
		{
			Reply($"Syntax: c.group add <group> [<displayName>] [<rank>]", arg);
			Reply($"Syntax: c.group remove <group>", arg);
			Reply($"Syntax: c.group set <group> <title|rank> <value>", arg);
			Reply($"Syntax: c.group parent <group> [<parent>]", arg);
		}

		if (!arg.HasArgs(1)) { PrintWarn(); return; }

		var action = arg.Args[0];

		switch (action)
		{
			case "add":
				{
					if (!arg.HasArgs(2)) { PrintWarn(); return; }

					var group = arg.Args[1];

					if (permission.GroupExists(group))
					{
						Reply($"Group '{group}' already exists. To set any values for this group, use 'c.group set'.", arg);
						return;
					}

					if (permission.CreateGroup(group, arg.HasArgs(3) ? arg.Args[2] : group, arg.HasArgs(4) ? arg.Args[3].ToInt() : 0))
					{
						Reply($"Created '{group}' group.", arg);
					}
				}
				break;

			case "set":
				{
					if (!arg.HasArgs(2)) { PrintWarn(); return; }

					var group = arg.Args[1];

					if (!permission.GroupExists(group))
					{
						Reply($"Group '{group}' does not exists.", arg);
						return;
					}

					if (arg.HasArgs(3)) permission.SetGroupTitle(group, arg.Args[2]);
					if (arg.HasArgs(4)) permission.SetGroupTitle(group, arg.Args[3]);

					Reply($"Set '{group}' group.", arg);
				}
				break;
			case "remove":
				{
					if (!arg.HasArgs(2)) { PrintWarn(); return; }

					var group = arg.Args[1];

					if (permission.RemoveGroup(group)) Reply($"Removed '{group}' group.", arg);
					else Reply($"Couldn't remove '{group}' group.", arg);
				}
				break;

			default:
				PrintWarn();
				break;
		}
	}

	#endregion
}
