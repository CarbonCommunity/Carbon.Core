using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Carbon.LoaderEx.ASM;
using Carbon.LoaderEx.Common;
using Carbon.LoaderEx.Utility;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.LoaderEx.Harmony;

internal sealed class HarmonyLoaderEx : Singleton<HarmonyLoaderEx>
{
	private HashSet<HarmonyPlugin> _loadedPlugins;

	static HarmonyLoaderEx() { }

	internal HarmonyLoaderEx()
	{
		_loadedPlugins = new HashSet<HarmonyPlugin>();
	}

	/// <summary>
	/// Loads assembly into AppDomain by always reading it back from disk.<br/>
	/// The process will automatically detect the type of file being loaded
	/// and use the appropriate loading method to deal with it.
	/// </summary>
	///
	/// <param name="path">Assembly name, dll filename or dll full path</param>
	internal Assembly Load(string path)
	{
		string file = Path.GetFileName(path);
		string location = Path.GetDirectoryName(path);

		Utility.Logger.Log($"Loading harmony plug-in '{file}' [{location}]..");

		// by default load harmony plugins
		// if (string.IsNullOrEmpty(location))
		// 	location = Context.Directories.CarbonHarmony;

		// validate boundaries
		// if (!location.StartsWith(Context.Directories.Carbon))
		// 	throw new FileNotFoundException("Out of boundaries");

		try
		{
			switch (Path.GetExtension(file))
			{
				case ".dll":
					HarmonyPlugin mod = new HarmonyPlugin(file, location);
					mod.Awake();
					mod.OnLoaded();

					_loadedPlugins.Add(mod);
					return mod.Assembly;

				// case ".drm"
				// 	LoadFromDRM();
				// 	break;

				default:
					throw new ArgumentOutOfRangeException("File extension not supported");
			}
		}
		catch (System.Exception e)
		{
			Utility.Logger.Error($"Failed loading '{path}'", e);
			throw;
		}
	}

	/// <summary>
	/// Calls the assembly OnUnloaded() and Dispose() methods to "unload".<br/>
	/// Due to the limitations of using only one AppDomain, the assembly will
	/// still be present on the loaded modules.
	/// </summary>
	///
	/// <param name="path">Assembly name, dll filename or dll full path</param>
	/// <param name="reload">Loads back the assembly</param>
	internal void Unload(string path, bool reload = false)
	{
		string file = Path.GetFileName(path);
		string location = Path.GetDirectoryName(path);
		string name = Path.GetFileNameWithoutExtension(path);

		Utility.Logger.Log($"Unloading harmony plug-in '{file}' [{location}]..");

		try
		{
			HarmonyPlugin mod = _loadedPlugins.SingleOrDefault(x => x.FileName == file);

			if (mod?.Assembly == null)
				throw new Exception($"Assembly '{name}' not loaded");

			AssemblyManager.GetInstance().RemoveCache(mod.Name);
			_loadedPlugins.Remove(mod);
			mod.Enabled = false;
			mod.OnUnloaded();
			mod.Dispose();
			mod = default;
		}
		catch (System.Exception e)
		{
			Utility.Logger.Warn($"Failed to unload '{file}' ({e.Message})");
		}

		if (reload)
		{
			Load(path);
		}
	}

	internal bool IsLoaded(string fileName)
		=> ((_loadedPlugins.SingleOrDefault(x => x.FileName == fileName) ?? null) != null);

	internal bool IsEnabled(string fileName)
		=> _loadedPlugins.SingleOrDefault(x => x.FileName == fileName).Enabled;
}
