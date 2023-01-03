
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

internal sealed class Updater
{
	private static readonly OsType Platform;
	private static readonly ReleaseType Release;
	private static readonly string Repository;

	public enum OsType { Windows, Linux }
	public enum ReleaseType { Develop, Staging, Production }

	static Updater()
	{
		Platform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) switch
		{
			true => OsType.Windows,
			false => OsType.Linux
		};

		Repository = @"CarbonCommunity/Carbon.Redist";

		Release =
#if DEBUG
		ReleaseType.Develop;
#else
		Release.Production;
#endif
	}

	private static string GithubReleaseUrl(string file)
	{
		string branch = string.Empty;
		string suffix = string.Empty;
		string target = string.Empty;

		switch (Release)
		{
			case ReleaseType.Develop:
				branch = "main";
				target = "Debug";
				break;

			case ReleaseType.Staging:
				branch = "main";
				target = "Debug";
				break;

			case ReleaseType.Production:
				branch = "main";
				target = "Release";
				break;
		}

		switch (Platform)
		{
			case OsType.Windows:
				suffix = string.Empty;
				break;

			case OsType.Linux:
				suffix = $"Unix";
				break;
		}

		return $"https://raw.githubusercontent.com/{Repository}/{branch}/Modules/{target}{suffix}/{file}";
	}

	internal static void DoUpdate(Action<bool> callback = null)
	{
		IReadOnlyList<string> files = new List<string>(){
			@"carbon/managed/hooks/Carbon.Hooks.Extended.dll"
		};

		bool retval = false;

		foreach (string file in files)
		{
			string url = GithubReleaseUrl(file);
			Logger.Warn($"Updating component '{Path.GetFileName(file)}' using the '{Release} [{Platform}]' branch");

			Carbon.Supervisor.ASM.Download(url, (string identifier, byte[] buffer) =>
			{
				if (buffer is { Length: > 0 })
				{
					Logger.Warn($"Patch downloaded [{Path.GetFileName(url)}], processing {buffer.Length} bytes from memory");
					string root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

					try
					{
						string destination = Path.GetFullPath(Path.Combine(root, file));
						File.WriteAllBytes(destination, buffer);
						retval = true;
					}
					catch (System.Exception e)
					{
						Logger.Error($"Error while updating component '{file}'", e);
						retval = false;
					}
				}

				callback?.Invoke(retval);
			});
		}
	}
}
