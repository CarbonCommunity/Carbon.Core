using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Carbon.Components;
using Carbon.Core;
using Carbon.Extensions;
using Facepunch;
using Oxide.Plugins;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon;

public class Report : IDisposable
{
	public static Action<string> OnPluginAdded;
	public static Action<Plugin, List<string>> OnPluginCompiled;
	public static Action OnProcessEnded;

	public Dictionary<string, Result> Results;

	public void Init()
	{
		Logger.Log($" Initializing report...");

		Results = new Dictionary<string, Result>(200);

		OnPluginCompiled = (plugin, incompatibleHooks) =>
		{
			if (Results.ContainsKey(plugin.FilePath)) return;

			Results.Add(plugin.FilePath, new Result
			{
				FilePath = plugin.FilePath,
				FileName = plugin.FileName,
				Plugin = plugin,
				IncompatibleHooks = incompatibleHooks.ToArray()
			});
		};
		OnProcessEnded = () =>
		{
			var report = string.Empty;

			using (var builder = new StringTable("#", "Name", "Author", "Version", "File", "Compile Time", "Hooks", "[HookMethod]s", "[PluginReference]s", "Incompatible Hooks"))
			{
				var counter = 1;
				foreach (var value in Results)
				{
					var result = value.Value;

					builder.AddRow($"{counter:n0}", result.Plugin.Name, result.Plugin.Author, result.Plugin.Version, result.FileName, $"{result.Plugin.CompileTime:0}ms", $"{result.Plugin.Hooks.ToArray().ToString(", ", " and ").Trim()}", $"{result.Plugin.HookMethods.Select(x => $"{x.Name}").ToArray().ToString(", ", " and ").Trim()}", $"{result.Plugin.PluginReferences.Select(x => $"{x.Field.FieldType.Name} {x.Field.Name}").ToArray().ToString(", ", " and ").Trim()}", $"{result.IncompatibleHooks.ToString(", ", " and ").Trim()}");
					counter++;
				}

				report += $"PLUGIN REPORT:\n{builder.ToStringMinimal()}\n\n";
			}

			using (var builder = new StringTable("#", "Name", "Uses", "Files Affected"))
			{
				var counter = 1;
				var hooks = Pool.GetList<string>();
				foreach (var result in Results) foreach (var hook in result.Value.IncompatibleHooks) if (!hooks.Contains(hook)) hooks.Add(hook);

				foreach (var hook in hooks)
				{
					builder.AddRow($"{counter:n0}", hook, $"{Results.Count(x => x.Value.IncompatibleHooks.Contains(hook)):n0}", $"{Results.Where(x => x.Value.IncompatibleHooks.Contains(hook)).Select(x => $"{x.Value.FileName}").ToArray().ToString(", ", " and ")}");
					counter++;
				}

				Pool.FreeList(ref hooks);

				report += $"INCOMPATIBLE HOOK REPORT:\n{builder.ToStringMinimal()}\n\n";
			}

			// Failed plugins
			{
				var result = "";
				var count = 1;

				foreach (var mod in Loader._failedMods)
				{
					result += $"{count:n0}. {mod.File}\n";

					foreach (var error in mod.Errors)
					{
						result += $" {error}\n";
					}

					result += "\n";
					count++;
				}

				report += $"COMPILATION FAILED PLUGINS:\n{result}\n\n";
			}

			var path = Path.Combine(Defines.GetReportsFolder(), $"pluginreport_{DateTime.UtcNow:ddMMyyyyhhmmss}.txt");
			OsEx.File.Create(path, report.Trim());
			Logger.Log($" Report generated with {Results.Count:n0} results at '{path}'");

			Dispose();
		};

		Community.ReloadPlugins();
	}
	public void Dispose()
	{
		OnPluginAdded = null;
		OnPluginCompiled = null;
		OnProcessEnded = null;

		foreach (var result in Results)
		{
			Array.Clear(result.Value.IncompatibleHooks, 0, result.Value.IncompatibleHooks.Length);
		}

		Results.Clear();
		Results = null;
	}

	public struct Result
	{
		public string FilePath;
		public string FileName;
		public Plugin Plugin;
		public string[] IncompatibleHooks;
	}
}
