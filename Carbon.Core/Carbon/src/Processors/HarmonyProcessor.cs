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

public class HarmonyProcessor : BaseProcessor
{
	public override bool EnableWatcher => Community.IsConfigReady ? Community.Runtime.Config.HarmonyWatchers : true;
	public override string Folder => Defines.GetHarmonyFolder();
	public override string Extension => ".dll";
	public override Type IndexedType => typeof(Harmony);

	public class Harmony : Instance
	{
		public override void Dispose()
		{
			Loader.UnloadCarbonMod(Path.GetFileNameWithoutExtension(File));
		}
		public override void Execute()
		{
			try
			{
				Loader.LoadCarbonMod(File, true);
			}
			catch (Exception ex)
			{
				Carbon.Logger.Warn($"Failed processing {Path.GetFileNameWithoutExtension(File)}:\n{ex}");
			}
		}
	}
}
