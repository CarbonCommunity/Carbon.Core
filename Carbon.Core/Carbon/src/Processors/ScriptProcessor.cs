using System;
using System.IO;
using Carbon.Base;
using Carbon.Core;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Processors;

public class ScriptProcessor : BaseProcessor
{
	public override bool EnableWatcher => Community.IsConfigReady ? Community.Runtime.Config.ScriptWatchers : true;
	public override string Folder => Defines.GetPluginsFolder();
	public override string Extension => ".cs";
	public override Type IndexedType => typeof(Script);

	public bool AllPendingScriptsComplete()
	{
		foreach (var instance in InstanceBuffer)
		{
			if (instance.Value is Script script)
			{
				if (script._loader != null && !script._loader.HasFinished) return false;
			}
		}

		return true;
	}
	public bool AllNonRequiresScriptsComplete()
	{
		foreach (var instance in InstanceBuffer)
		{
			if (instance.Value is Script script)
			{
				if (script._loader != null && !script._loader.HasRequires && !script._loader.HasFinished) return false;
			}
		}

		return true;
	}

	public class Script : Instance
	{
		internal ScriptLoader _loader;

		public override Parser Parser => new ScriptParser();

		public override void Dispose()
		{
			try
			{
				_loader?.Clear();
			}
			catch (Exception ex)
			{
				Carbon.Logger.Error($"Error disposing {File}", ex);
			}

			_loader = null;
		}
		public override void Execute()
		{
			try
			{
				Loader._failedMods.RemoveAll(x => x.File == File);

				_loader = new ScriptLoader();
				_loader.Parser = Parser;
				_loader.File = File;
				_loader.Mod = Community.Runtime.Plugins;
				_loader.Instance = this;
				_loader.Load();
			}
			catch (Exception ex)
			{
				Carbon.Logger.Warn($"Failed processing {Path.GetFileNameWithoutExtension(File)}:\n{ex}");
			}
		}
	}

	public class ScriptParser : Parser
	{
		public bool IsLineValid(string line)
		{
			return !line.Contains(".splashThreshold");
		}

		public override void Process(string input, out string output)
		{
			output = input
				.Replace(".IPlayer", ".AsIPlayer()")
				.Replace("using Harmony;", "using HarmonyLib;")
				.Replace("HarmonyInstance.Create", "new HarmonyLib.Harmony")
				.Replace("HarmonyInstance", "HarmonyLib.Harmony")
				.Replace("PluginTimers", "Timers")
				.Replace("protected override void PostSpawnProcess", "public override void PostSpawnProcess")
				.Replace("protected override bool IsClipping", "public override bool IsClipping");

			var newOutput = string.Empty;
			var split = output.Split('\n');

			foreach (var line in split)
			{
				if (!IsLineValid(line)) continue;

				newOutput += line + "\n";
			}

			output = newOutput;
			Array.Clear(split, 0, split.Length);
			split = null;
		}
	}
}
