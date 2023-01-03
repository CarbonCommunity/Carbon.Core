using System;
using System.Reflection;
using Carbon.LoaderEx.ASM;
using Carbon.LoaderEx.Common;
using Carbon.LoaderEx.Utility;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.LoaderEx;

internal sealed class Program : Singleton<Program>, IDisposable
{
	private static readonly string identifier;
	private static readonly string assemblyName;

	private HarmonyLib.Harmony _harmonyInstance;
	private UnityEngine.GameObject _gameObject;


	internal string Name
	{ get => assemblyName; }

	internal HarmonyLib.Harmony Harmony
	{ get => _harmonyInstance; }

	internal DownloadManager Downloader
	{ get => _gameObject.GetComponent<DownloadManager>(); }

	static Program()
	{
		identifier = $"{Guid.NewGuid():N}";
		Logger.Warn($"Using '{identifier}' as runtime namespace");
		assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
		Logger.Warn($"Runtime baptized us as '{assemblyName}', látom.");
		AssemblyManager.GetInstance().Register(AppDomain.CurrentDomain);
	}

	internal Program()
	{
		_harmonyInstance = new HarmonyLib.Harmony(identifier);
		_gameObject = new UnityEngine.GameObject(identifier);
		_gameObject.AddComponent<DownloadManager>();
		UnityEngine.Object.DontDestroyOnLoad(_gameObject);
	}

	internal void Initialize()
	{
		Logger.None(
			@"                                               " + Environment.NewLine +
			@"  ______ _______ ______ ______ _______ _______ " + Environment.NewLine +
			@" |      |   _   |   __ \   __ \       |    |  |" + Environment.NewLine +
			@" |   ---|       |      <   __ <   -   |       |" + Environment.NewLine +
			@" |______|___|___|___|__|______/_______|__|____|" + Environment.NewLine +
			@"                         discord.gg/eXPcNKK4yd " + Environment.NewLine +
			@"                                               " + Environment.NewLine
		);

		try
		{
			Logger.Log("Patching Facepunch's harmony loader");
			Harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
		catch (Exception e)
		{
			Logger.Error("Unable to apply all Harmony patches", e);
		}
	}

	public void Dispose()
	{
		try
		{
			Harmony.UnpatchAll(identifier);
			Logger.Log("Removed all Harmony patches");
		}
		catch (Exception e)
		{
			Logger.Error("Unable to remove all Harmony patches", e);
		}

		_harmonyInstance = default;
		_gameObject = default;
	}
}
