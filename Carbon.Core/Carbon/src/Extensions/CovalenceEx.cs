using Oxide.Core.Libraries.Covalence;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

public static class CovalenceEx
{
	public static RustPlayer AsIPlayer(this BasePlayer player)
	{
		var iplayer = default(RustPlayer);
		iplayer.Object = player;

		return iplayer;
	}
}
