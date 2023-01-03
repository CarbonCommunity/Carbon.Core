using System;
using Carbon.Extensions;
using ConVar;
using Facepunch.Extend;
using Oxide.Core;
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
	public partial class Static_Chat
	{
		[HookAttribute.Patch("OnPlayerCommand", typeof(Chat), "sayAs", new System.Type[] { typeof(ConVar.Chat.ChatChannel), typeof(ulong), typeof(string), typeof(string), typeof(BasePlayer) })]
		[HookAttribute.Identifier("fbe2fbe2debc47448ce1c319d441203e")]
		[HookAttribute.Options(HookFlags.Static | HookFlags.IgnoreChecksum)]

		// Called before a world prefab is spawned.

		public class Static_Chat_SayAs_fbe2fbe2debc47448ce1c319d441203e
		{
			internal static string[] EmptyArgs = new string[0];

			public static bool Prefix(Chat.ChatChannel targetChannel, ulong userId, string username, string message, BasePlayer player = null)
			{
				if (Community.Runtime == null) return true;

				try
				{
					var fullString = message.Substring(1);
					var split = fullString.Split(ConsoleArgEx.CommandSpacing, StringSplitOptions.RemoveEmptyEntries);
					var command = split[0].Trim();
					var args = split.Length > 1 ? fullString.Substring(command.Length + 1).SplitQuotesStrings() : EmptyArgs;
					Facepunch.Pool.Free(ref split);

					if (Interface.CallHook("OnPlayerCommand", BasePlayer.FindByID(userId), command, args) != null)
					{
						return false;
					}

					foreach (var cmd in Community.Runtime?.AllChatCommands)
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
										player?.ConsoleMessage($"You don't have the minimum required auth level to execute this command.");
										continue;
									}
								}
							}

							try
							{
								cmd.Callback?.Invoke(player, command, args);
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
