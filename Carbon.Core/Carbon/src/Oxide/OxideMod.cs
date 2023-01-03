using System;
using Carbon;
using Carbon.Core;
using Oxide.Core.Libraries;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core;

public class OxideMod
{
	public DataFileSystem DataFileSystem { get; private set; } = new DataFileSystem(Defines.GetDataFolder());

	public Permission Permission { get; private set; }

	public string RootDirectory { get; private set; }
	public string InstanceDirectory { get; private set; }
	public string PluginDirectory { get; private set; }
	public string ConfigDirectory { get; private set; }
	public string DataDirectory { get; private set; }
	public string LangDirectory { get; private set; }
	public string LogDirectory { get; private set; }
	public string TempDirectory { get; private set; }

	public bool IsShuttingDown { get; private set; }

	//public float Now => UnityEngine.Time.realtimeSinceStartup;

	public void Load()
	{
		InstanceDirectory = Defines.GetRootFolder();
		RootDirectory = Environment.CurrentDirectory;
		if (RootDirectory.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
			RootDirectory = AppDomain.CurrentDomain.BaseDirectory;

		ConfigDirectory = Defines.GetConfigsFolder();
		DataDirectory = Defines.GetDataFolder();
		LangDirectory = Defines.GetLangFolder();
		LogDirectory = Defines.GetLogsFolder();
		PluginDirectory = Defines.GetPluginsFolder();
		TempDirectory = Defines.GetTempFolder();

		DataFileSystem = new DataFileSystem(DataDirectory);

		Permission = new Permission();
	}

	public void NextTick(Action callback)
	{
		Community.Runtime.CarbonProcessor.OnFrameQueue.Enqueue(callback);
	}

	public void NextFrame(Action callback)
	{
		Community.Runtime.CarbonProcessor.OnFrameQueue.Enqueue(callback);
	}

	public void UnloadPlugin(string name)
	{

	}

	public void OnSave()
	{

	}

	public void OnShutdown()
	{
		if (!IsShuttingDown)
		{
			IsShuttingDown = true;
		}
	}

	public object CallHook(string hookName, params object[] args)
	{
		return HookCaller.CallStaticHook(hookName, args);
	}

	public object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, params object[] args)
	{
		return HookCaller.CallStaticDeprecatedHook(oldHook, newHook, expireDate, args);
	}

	public T GetLibrary<T>() where T : Library
	{
		return Activator.CreateInstance<T>();
	}

	#region Logging

	/// <summary>
	/// Outputs to the game's console a message with severity level 'NOTICE'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	public void LogInfo(string message)
		=> Carbon.Logger.Log(message);

	/// <summary>
	/// Outputs to the game's console a message with severity level 'WARNING'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	public void LogWarning(string message)
		=> Carbon.Logger.Warn(message);

	/// <summary>
	/// Outputs to the game's console a message with severity level 'ERROR'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="ex"></param>
	public void LogError(string message, Exception ex)
		=> Carbon.Logger.Error(message, ex);

	/// <summary>
	/// Outputs to the game's console a message with severity level 'ERROR'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	public void LogError(string message)
		=> Carbon.Logger.Error(message, null);

	/// <summary>
	/// Outputs to the game's console a message with severity level 'WARNING'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="args"></param>
	public void PrintWarning(string format, params object[] args)
		=> Carbon.Logger.Warn(string.Format(format, args));

	/// <summary>
	/// Outputs to the game's console a message with severity level 'ERROR'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="args"></param>
	public void PrintError(string format, params object[] args)
		=> Carbon.Logger.Error(string.Format(format, args));

	#endregion
}
