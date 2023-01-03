using System;
using System.Collections.Generic;
using Carbon.Base;
using Carbon.Base.Interfaces;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Processors;

public class ModuleProcessor : IDisposable
{
	public List<BaseHookable> Modules { get; set; } = new List<BaseHookable>(50);

	public void Init()
	{
		foreach (var type in typeof(ModuleProcessor).Assembly.GetTypes())
		{
			if (type.BaseType == null || !type.BaseType.Name.Contains("CarbonModule")) continue;

			Setup(Activator.CreateInstance(type) as BaseHookable);
		}
	}
	public void Setup(BaseHookable module)
	{
		if (module is IModule hookable)
		{
			hookable.Init();
			Modules.Add(module);
			hookable.InitEnd();
		}
	}

	public void Save()
	{
		foreach (var hookable in Modules)
		{
			var module = hookable.To<IModule>();

			module.Save();
		}
	}
	public void Load()
	{
		foreach (var hookable in Modules)
		{
			var module = hookable.To<IModule>();

			module.Load();
			module.OnEnableStatus();
		}
	}

	public void Dispose()
	{
		foreach (var hookable in Modules)
		{
			var module = hookable.To<IModule>();

			module.Dispose();
		}

		Modules.Clear();
	}
}
