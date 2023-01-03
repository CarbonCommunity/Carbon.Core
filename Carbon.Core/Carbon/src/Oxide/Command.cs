using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Carbon;
using Carbon.Base;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Plugins;
using static ConsoleSystem;
using Pool = Facepunch.Pool;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Game.Rust.Libraries
{
	public class Command
	{
		public static bool FromRcon { get; set; }

		public void AddChatCommand(string command, BaseHookable plugin, Action<BasePlayer, string, string[]> callback, bool skipOriginal = true, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1)
		{
			if (Community.Runtime.AllChatCommands.Count(x => x.Command == command) == 0)
			{
				Community.Runtime.AllChatCommands.Add(new OxideCommand
				{
					Command = command,
					Plugin = plugin,
					SkipOriginal = skipOriginal,
					Callback = (player, cmd, args) =>
					{
						try { callback.Invoke(player, cmd, args); }
						catch (Exception ex) { if (plugin is RustPlugin rustPlugin) rustPlugin.LogError("Error", ex.InnerException ?? ex); }
					},
					Help = help,
					Reference = reference,
					Permissions = permissions,
					Groups = groups,
					AuthLevel = authLevel
				});
			}
			else Carbon.Logger.Warn($"Chat command '{command}' already exists.");
		}
		public void AddChatCommand(string command, BaseHookable plugin, string method, bool skipOriginal = true, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1)
		{
			AddChatCommand(command, plugin, (player, cmd, args) =>
			{
				var argData = Pool.GetList<object>();
				var result = (object[])null;
				try
				{
					var m = plugin.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					var ps = m.GetParameters();
					switch (ps.Length)
					{
						case 1:
							{
								if (ps.ElementAt(0).ParameterType == typeof(IPlayer)) argData.Add(player.AsIPlayer()); else argData.Add(player);
								result = argData.ToArray();
								break;
							}

						case 2:
							{
								if (ps.ElementAt(0).ParameterType == typeof(IPlayer)) argData.Add(player.AsIPlayer()); else argData.Add(player);
								argData.Add(cmd);
								result = argData.ToArray();
								break;
							}

						case 3:
							{
								if (ps.ElementAt(0).ParameterType == typeof(IPlayer)) argData.Add(player.AsIPlayer()); else argData.Add(player);
								argData.Add(cmd);
								argData.Add(args);
								result = argData.ToArray();
								break;
							}
					}

					m?.Invoke(plugin, result);
				}
				catch (Exception ex) { if (plugin is RustPlugin rustPlugin) rustPlugin.LogError("Error", ex.InnerException ?? ex); }

				if (argData != null) Pool.FreeList(ref argData);
				if (result != null) Pool.Free(ref result);
			}, skipOriginal, help, reference, permissions, groups, authLevel);
		}
		public void AddConsoleCommand(string command, BaseHookable plugin, Action<BasePlayer, string, string[]> callback, bool skipOriginal = true, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1)
		{
			if (Community.Runtime.AllConsoleCommands.Count(x => x.Command == command) == 0)
			{
				Community.Runtime.AllConsoleCommands.Add(new OxideCommand
				{
					Command = command,
					Plugin = plugin,
					SkipOriginal = skipOriginal,
					Callback = callback,
					Help = help,
					Reference = reference,
					Permissions = permissions,
					Groups = groups,
					AuthLevel = authLevel
				});
			}
			else Carbon.Logger.Warn($"Console command '{command}' already exists.");
		}
		public void AddConsoleCommand(string command, BaseHookable plugin, string method, bool skipOriginal = true, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1)
		{
			AddConsoleCommand(command, plugin, (player, cmd, args) =>
			{
				var arguments = Pool.GetList<object>();
				var result = (object[])null;

				try
				{
					var fullString = args == null || args.Length == 0 ? string.Empty : string.Join(" ", args);
					var client = player == null ? Option.Unrestricted : Option.Client;
					var arg = FormatterServices.GetUninitializedObject(typeof(Arg)) as Arg;
					if (player != null) client = client.FromConnection(player.net.connection);
					client.FromRcon = FromRcon;
					arg.Option = client;
					arg.FullString = fullString;
					arg.Args = args;

					arguments.Add(arg);

					try
					{
						var methodInfo = plugin.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						var parameters = methodInfo.GetParameters();

						if (parameters.Length > 0)
						{
							for (int i = 1; i < parameters.Length; i++)
							{
								arguments.Add(null);
							}
						}

						result = arguments.ToArray();

						if (Interface.CallHook("OnCarbonCommand", arg) == null)
						{
							methodInfo?.Invoke(plugin, result);
						}
					}
					catch (Exception ex) { if (plugin is RustPlugin rustPlugin) rustPlugin.LogError("Error", ex.InnerException ?? ex); }
				}
				catch (TargetParameterCountException) { }
				catch (Exception ex) { if (plugin is RustPlugin rustPlugin) rustPlugin.LogError("Error", ex.InnerException ?? ex); }

				Pool.FreeList(ref arguments);
				if (result != null) Pool.Free(ref result);
			}, skipOriginal, help, reference, permissions, groups, authLevel);
		}
		public void AddConsoleCommand(string command, BaseHookable plugin, Func<Arg, bool> callback, bool skipOriginal = true, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1)
		{
			AddConsoleCommand(command, plugin, (player, cmd, args) =>
			{
				var arguments = Pool.GetList<object>();
				var result = (object[])null;

				try
				{
					var fullString = args == null || args.Length == 0 ? string.Empty : string.Join(" ", args);
					var client = player == null ? Option.Unrestricted : Option.Client;
					var arg = FormatterServices.GetUninitializedObject(typeof(Arg)) as Arg;
					if (player != null) client = client.FromConnection(player.net.connection);
					client.FromRcon = FromRcon;
					arg.Option = client;
					arg.FullString = fullString;
					arg.Args = args;

					arguments.Add(arg);
					result = arguments.ToArray();

					if (Interface.CallHook("OnCarbonCommand", arg) == null)
					{
						callback.Invoke(arg);
					}
				}
				catch (TargetParameterCountException) { }
				catch (Exception ex) { if (plugin is RustPlugin rustPlugin) rustPlugin.LogError("Error", ex.InnerException ?? ex); }

				Pool.FreeList(ref arguments);
				if (result != null) Pool.Free(ref result);
			}, skipOriginal, help, reference, permissions, groups, authLevel);
		}
		public void AddCovalenceCommand(string command, BaseHookable plugin, string method, bool skipOriginal = true, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1)
		{
			AddChatCommand(command, plugin, method, skipOriginal, help, reference, permissions, groups, authLevel);
			AddConsoleCommand(command, plugin, method, skipOriginal, help, reference, permissions, groups, authLevel);
		}
		public void AddCovalenceCommand(string command, BaseHookable plugin, Action<BasePlayer, string, string[]> callback, bool skipOriginal = true, string help = null, object reference = null, string[] permissions = null, string[] groups = null, int authLevel = -1)
		{
			AddChatCommand(command, plugin, callback, skipOriginal, help, reference, permissions, groups, authLevel);
			AddConsoleCommand(command, plugin, callback, skipOriginal, help, reference, permissions, groups, authLevel);
		}
	}
}
