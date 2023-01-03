using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SharpCompress.Common;
using SharpCompress.Readers;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Core;

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

		Repository = @"CarbonCommunity/Carbon.Core";

		Release =
#if DEBUG
		ReleaseType.Develop;
#else
		Release.Production;
#endif
	}

	private static string GithubReleaseUrl()
	{
		string tag = string.Empty;
		string file = string.Empty;
		string target = string.Empty;

		switch (Release)
		{
			case ReleaseType.Develop:
				tag = "develop_build";
				target = "Debug";
				break;

			case ReleaseType.Staging:
				tag = "staging_build";
				target = "Debug";
				break;

			case ReleaseType.Production:
				tag = "production_build";
				target = "Release";
				break;
		}

		switch (Platform)
		{
			case OsType.Windows:
				file = $"Carbon.{target}.zip";
				break;

			case OsType.Linux:
				file = $"Carbon.{target}Unix.tar.gz";
				break;
		}

		return $"https://github.com/{Repository}/releases/download/{tag}/{file}";
	}

	internal static void DoUpdate(Action<bool> callback = null)
	{
		IReadOnlyList<string> files = new List<string>(){
			@"carbon/managed/Carbon.dll",
			@"carbon/managed/Carbon.Doorstop.dll",
			@"carbon/managed/Carbon.Loader.dll",
			@"harmonymods/Carbon.Stub.dll"
		};

		bool retval = false;

		string url = GithubReleaseUrl();
		Logger.Warn($"Updating component 'Carbon' using the '{Release} [{Platform}]' branch");

		Carbon.Supervisor.ASM.Download(url, (string identifier, byte[] buffer) =>
		{
			if (buffer is { Length: > 0 })
			{
				Logger.Warn($"Patch downloaded [{Path.GetFileName(url)}], processing {buffer.Length} bytes from memory");
				string root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);

				try
				{
					using MemoryStream archive = new MemoryStream(buffer);
					{
						using IReader reader = ReaderFactory.Open(archive);
						while (reader.MoveToNextEntry())
						{
							if (reader.Entry.IsDirectory || !files.Contains(reader.Entry.Key, StringComparer.OrdinalIgnoreCase)) continue;
							string destination = Path.GetFullPath(Path.Combine(root, reader.Entry.Key));

							using EntryStream entry = reader.OpenEntryStream();
							using var fs = new FileStream(destination, FileMode.OpenOrCreate);
							Logger.Debug($" - Updated {destination}");
							entry.CopyTo(fs);
						}
					}
					retval = true;
				}
				catch (System.Exception e)
				{
					Logger.Error($"Error while updating 'Carbon.Core [{Platform}]'", e);
					retval = false;
				}
			}

			callback?.Invoke(retval);
		});
	}
}
