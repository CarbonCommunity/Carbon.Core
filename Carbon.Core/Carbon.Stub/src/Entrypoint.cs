using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Stub;

internal sealed class Entrypoint : IHarmonyModHooks
{
	public Entrypoint()
	{
		string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
		Console.WriteLine($"Facepunch baptized us as '{assemblyName}', látom.");
	}

	public void OnLoaded(OnHarmonyModLoadedArgs args)
	{
		MoveHarmonyPlugins();
		DisableResolver();
		DisableHarmony();
		InitLoader();
	}

	public void OnUnloaded(OnHarmonyModUnloadedArgs args)
	{

	}

	private void MoveHarmonyPlugins()
	{
		try
		{
			string source = Path.Combine(
				AppDomain.CurrentDomain.BaseDirectory, "HarmonyMods");
			if (!Directory.Exists(source)) throw new Exception("Unable to find the HarmonyMods folder");

			string target = Path.Combine(
				AppDomain.CurrentDomain.BaseDirectory, "carbon", "harmony");
			if (!Directory.Exists(target)) Directory.CreateDirectory(target);

			int count = 0;
			foreach (string file in Directory.EnumerateFiles(source, "*.dll"))
			{
				try
				{
					string name = Path.GetFileName(file);
					if (name.Equals("Carbon.Stub.dll")) continue;

					File.Copy(file, Path.Combine(target, name), true);
					File.Delete(file);
				}
				catch (System.Exception)
				{
					continue;
				}
				count++;
			}

			if (count != 0)
			{
				Console.WriteLine("Application will now exit");
				Process.GetCurrentProcess().Kill();
			}
		}
		catch (Exception e)
		{
			Console.WriteLine("Error while moving files", e);
		}
	}

	private void DisableResolver()
	{
		try
		{
			Console.WriteLine("Cleaning AssemblyResolve event handler");

			Type appdomain = typeof(AppDomain);

			EventInfo eventInfo = appdomain.GetEvent("AssemblyResolve",
				BindingFlags.Public | BindingFlags.Instance);

			FieldInfo fieldInfo = appdomain.GetField(eventInfo.Name,
				BindingFlags.NonPublic | BindingFlags.Instance);

			Delegate eventDelegate = (Delegate)fieldInfo.GetValue(AppDomain.CurrentDomain) ?? null;

			MethodInfo nfo;
			foreach (Delegate evh in eventDelegate?.GetInvocationList())
			{
				try
				{
					nfo = evh.GetMethodInfo();
					eventInfo.RemoveEventHandler(AppDomain.CurrentDomain, evh);
					Console.WriteLine($" - Removed {nfo.Name} [{nfo.Module}]");

				}
				catch (System.Exception)
				{
					continue;
				}
			}
		}
		catch (System.Exception e)
		{
			Console.WriteLine("Error cleaning event handler", e);
		}
	}

	private void DisableHarmony()
	{
		// this is patched by Carbon.Loader for now
	}

	private void InitLoader()
	{
		string location = Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory, "carbon", "managed");

		Assembly.LoadFile(Path.Combine(location, "lib", "1Harmony.dll"));
		Assembly assembly = Assembly.LoadFile(Path.Combine(location, "Carbon.Loader.dll"));

		Type loader = assembly.GetType("Carbon.LoaderEx.Program");

		MethodInfo instance = loader
			.GetMethod("GetInstance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

		MethodInfo initialize = loader
			.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);

		initialize.Invoke(instance.Invoke(null, null), null);
	}
}
