﻿///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Humanlights.Extensions;
using UnityEngine;

namespace Carbon.Core
{
	[Serializable]
	public class CarbonDefines
	{
		public static Assembly Carbon { get; internal set; }

		public static List<Hook> Hooks { get; internal set; }

		public static void Initialize()
		{
			Hooks?.Clear();
			Hooks = new List<Hook>();

			Carbon = typeof(CarbonCore).Assembly;

			foreach (var type in Carbon.GetTypes())
			{
				var hook = type.GetCustomAttribute<Hook>();
				if (hook == null) continue;

				Hooks.Add(hook);
			}

			GetRootFolder();
			GetConfigsFolder();
			GetModulesFolder();
			GetDataFolder();
			GetPluginsFolder();
			GetHarmonyFolder();
			GetLogsFolder();
			GetLangFolder();
			GetReportsFolder();
			OsEx.Folder.DeleteContents(GetTempFolder());
			Logger.Log("Loaded folders");
		}

		#region Paths

		public const string Name =
#if WIN
			"Carbon";
#elif UNIX
	"Carbon-Unix";
#endif
		public const string DllName =
#if WIN
			"Carbon.dll";
#elif UNIX
			"Carbon-Unix.dll";
#endif
		public static string DllPath => Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HarmonyMods",
#if WIN
			"Carbon.dll"
#elif UNIX
			"Carbon-Unix.dll"
#endif
	));

		public static string GetConfigFile()
		{
			return Path.Combine(GetRootFolder(), "config.json");
		}

		public static string GetRootFolder()
		{
			var folder = Path.GetFullPath(Path.Combine($"{Application.dataPath}/..", "carbon"));
			Directory.CreateDirectory(folder);

			return folder;
		}
		public static string GetConfigsFolder()
		{
			var folder = Path.Combine($"{GetRootFolder()}", "configs");
			Directory.CreateDirectory(folder);

			return folder;
		}
		public static string GetModulesFolder()
		{
			var folder = Path.Combine($"{GetRootFolder()}", "modules");
			Directory.CreateDirectory(folder);

			return folder;
		}
		public static string GetDataFolder()
		{
			var folder = Path.Combine($"{GetRootFolder()}", "data");
			Directory.CreateDirectory(folder);

			return folder;
		}
		public static string GetPluginsFolder()
		{
			var folder = Path.Combine($"{GetRootFolder()}", "plugins");
			Directory.CreateDirectory(folder);

			return folder;
		}
		public static string GetHarmonyFolder()
		{
			var folder = Path.Combine($"{GetRootFolder()}", "harmony");
			Directory.CreateDirectory(folder);

			return folder;
		}
		public static string GetLogsFolder()
		{
			var folder = Path.Combine($"{GetRootFolder()}", "logs");
			Directory.CreateDirectory(folder);

			return folder;
		}
		public static string GetLangFolder()
		{
			var folder = Path.Combine($"{GetRootFolder()}", "lang");
			Directory.CreateDirectory(folder);

			return folder;
		}
		public static string GetTempFolder()
		{
			var folder = Path.Combine($"{GetRootFolder()}", "temp");
			Directory.CreateDirectory(folder);

			return folder;
		}
		public static string GetReportsFolder()
		{
			var folder = Path.Combine($"{GetRootFolder()}", "reports");
			Directory.CreateDirectory(folder);

			return folder;
		}

		#endregion
	}
}
