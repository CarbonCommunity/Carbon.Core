using System.Globalization;
using System.Text.RegularExpressions;
using Carbon.Oxide;
using Network;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Game.Rust.Libraries;

public class Player
{
	internal static readonly string ipPattern = ":{1}[0-9]{1}\\d*";

	public CultureInfo Language(BasePlayer player)
	{
		return CultureInfo.GetCultureInfo(player.net.connection.info.GetString("global.language") ?? "en");
	}

	public string Address(Connection connection)
	{
		return Regex.Replace(connection.ipaddress, ipPattern, "");
	}
	public string Address(BasePlayer player)
	{
		if (player?.net?.connection == null)
		{
			return null;
		}

		return Address(player.net.connection);
	}

	public int Ping(Connection connection)
	{
		return Net.sv.GetAveragePing(connection);
	}
	public int Ping(BasePlayer player)
	{
		return Ping(player.net.connection);
	}

	public void Message(BasePlayer player, string message, string prefix, ulong userId = 0uL, params object[] args)
	{
		if (!string.IsNullOrEmpty(message))
		{
			message = ((args.Length != 0) ? string.Format(Formatter.ToUnity(message), args) : Formatter.ToUnity(message));
			string text = ((prefix != null) ? (prefix + " " + message) : message);
			player.SendConsoleCommand("chat.add", 2, userId, text);
		}
	}
	public void Message(BasePlayer player, string message, ulong userId = 0uL)
	{
		Message(player, message, null, userId);
	}
	public void Command(BasePlayer player, string command, params object[] args)
	{
		player.SendConsoleCommand(command, args);
	}
}
