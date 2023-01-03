using System;
using Carbon.Extensions;
using Facepunch.Extend;
using Oxide.Game.Rust.Libraries;
using Oxide.Plugins;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Static
{
	public partial class Static_ConsoleSystem
	{
		[HookAttribute.Patch("OnCarbonCommand", typeof(ConsoleSystem), "Run", new System.Type[] { typeof(ConsoleSystem.Option), typeof(string), typeof(object[]) })]
		[HookAttribute.Identifier("4be71c5d077949cdb88438ec6dabac24")]
		[HookAttribute.Options(HookFlags.Static | HookFlags.IgnoreChecksum)]

		// Called whenever a Carbon server command is called.

		public class Static_ConsoleSystem_Run_4be71c5d077949cdb88438ec6dabac24
		{
			internal static string[] EmptyArgs = new string[0];

			public static bool Prefix(ConsoleSystem.Option options, string strCommand, object[] args)
			{
				if (Community.Runtime == null) return true;

				try
				{
					var split = strCommand.Split(ConsoleArgEx.CommandSpacing, StringSplitOptions.RemoveEmptyEntries);
					var command = split[0].Trim();
					var args2 = split.Length > 1 ? strCommand.Substring(command.Length + 1).SplitQuotesStrings() : EmptyArgs;
					Facepunch.Pool.Free(ref split);

					var player = options.Connection?.player as BasePlayer;

					foreach (var cmd in Community.Runtime.AllConsoleCommands)
					{
						if (cmd.Command == command)
						{
							if (player != null)
							{
								if (cmd.Permissions != null)
								{
									var hasPerm = false;
									foreach (var permission in cmd.Permissions)
									{
										if (cmd.Plugin is RustPlugin rust && rust.permission.UserHasPermission(player.UserIDString, permission))
										{
											hasPerm = true;
											break;
										}
									}

									if (!hasPerm)
									{
										player?.ConsoleMessage($"You don't have any of the required permissions to run this command.");
										continue;
									}
								}

								if (cmd.Groups != null)
								{
									var hasGroup = false;
									foreach (var group in cmd.Groups)
									{
										if (cmd.Plugin is RustPlugin rust && rust.permission.UserHasGroup(player.UserIDString, group))
										{
											hasGroup = true;
											break;
										}
									}

									if (!hasGroup)
									{
										player?.ConsoleMessage($"You aren't in any of the required groups to run this command.");
										continue;
									}
								}

								if (cmd.AuthLevel != -1)
								{
									var hasAuth = !ServerUsers.users.ContainsKey(player.userID) ? player.Connection.authLevel >= cmd.AuthLevel : (int)ServerUsers.Get(player.userID).group >= cmd.AuthLevel;

									if (!hasAuth)
									{
										player?.ConsoleMessage($"You don't have the minimum auth level required to execute this command.");
										continue;
									}
								}
							}

							try
							{
								Command.FromRcon = false;
								cmd.Callback?.Invoke(player, command, args2);
							}
							catch (Exception ex)
							{
								Carbon.Logger.Error("ConsoleSystem_Run", ex);
							}

							return false;
						}
					}
				}
				catch { }

				return true;
			}
		}
	}
}
