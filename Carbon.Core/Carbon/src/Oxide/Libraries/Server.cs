using Carbon.Oxide;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Game.Rust.Libraries;

public class Server
{
	public void Broadcast(string message, string prefix, ulong userId = 0uL, params object[] args)
	{
		if (!string.IsNullOrEmpty(message))
		{
			message = ((args.Length != 0) ? string.Format(Formatter.ToUnity(message), args) : Formatter.ToUnity(message));
			string text = ((prefix != null) ? (prefix + ": " + message) : message);
			ConsoleNetwork.BroadcastToAllClients("chat.add", 2, userId, text);
		}
	}
	public void Broadcast(string message, ulong userId = 0uL)
	{
		Broadcast(message, null, userId);
	}

	public void Command(string command, params object[] args)
	{
		ConsoleSystem.Run(ConsoleSystem.Option.Server, command, args);
	}
}
