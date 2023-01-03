using System;
using System.IO;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Utility;

internal static class Context
{
	public static readonly string Base, Carbon, Managed;

	private static readonly string[] Needles = {
		".", "..", "../.."
	};

	static Context()
	{
		foreach (string Needle in Needles)
		{
			string t = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Needle));
			if (!Directory.Exists(Path.Combine(t, "RustDedicated_Data"))) continue;
			Base = t;
			break;
		}

		if (Base == null)
			throw new System.Exception("Unable to find root folder");

		Carbon = Path.GetFullPath(Path.Combine(
			Base, "carbon"));

		Managed = Path.GetFullPath(Path.Combine(
			Base, "RustDedicated_Data", "Managed"));
	}
}
