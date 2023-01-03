using UnityEngine;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Extensions;

public static class ConsoleArgEx
{
	public static char[] CommandSpacing = new char[] { ' ' };

	public static bool IsPlayerCalledAndAdmin(this ConsoleSystem.Arg arg)
	{
		return arg.Player() == null || arg.IsAdmin;
	}
}
