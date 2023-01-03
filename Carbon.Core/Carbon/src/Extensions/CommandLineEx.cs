using System;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Extensions;

public static class CommandLineEx
{
	/// <summary>
	/// Returns the result after an argument in a args/string array.
	/// </summary>
	/// <param name="args">Targeted string array.</param>
	/// <param name="argument">Argument target to get from the string array.</param>
	/// <param name="Default">If returned no argument result/not found, return this Default string.</param>
	/// <returns></returns>
	public static string GetArgumentResult(this string[] args, string argument, string Default = null)
	{
		var result = string.Empty;


		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == argument)
			{
				return string.IsNullOrEmpty(args[i + 1]) ? result = Default : result = args[i + 1];
			}
		}

		if (string.IsNullOrEmpty(result))
		{
			result = Default;
		}

		return result;
	}

	/// <summary>
	/// Returns the result after an argument from the Environment.GetCommandLineArgs.
	/// </summary>
	/// <param name="argument">Argument target to get from the string array.</param>
	/// <param name="Default">If returned no argument result/not found, return this Default string.</param>
	/// <returns></returns>
	public static string GetArgumentResult(this string argument, string Default = null)
	{
		var args = Environment.GetCommandLineArgs();

		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == argument)
			{
				return string.IsNullOrEmpty(args[i + 1]) ? Default : args[i + 1];
			}
		}

		return Default;
	}

	/// <summary>
	/// Checks for a argument in the string array, if it is there, returns true.
	/// </summary>
	/// <param name="args">Targeted string array.</param>
	/// <param name="argument">Argument target to check if exists from the string array.</param>
	/// <returns></returns>
	public static bool GetArgumentExists(this string[] args, string argument)
	{
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == argument)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Checks for a argument from the Environment.GetCommandLineArgs, if it is there, returns true.
	/// </summary>
	/// <param name="argument">Argument target to check if exists from the string array.</param>
	/// <returns></returns>
	public static bool GetArgumentExists(this string argument)
	{
		var args = Environment.GetCommandLineArgs();

		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == argument)
			{
				return true;
			}
		}

		return false;
	}
}
