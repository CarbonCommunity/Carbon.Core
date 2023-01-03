using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Newtonsoft.Json;
using Oxide.Core;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Base;

public class BaseHookable
{
	public List<string> Hooks { get; internal set; }
	public List<HookMethodAttribute> HookMethods { get; internal set; }
	public List<PluginReferenceAttribute> PluginReferences { get; internal set; }

	public Dictionary<string, List<MethodInfo>> HookCache { get; internal set; } = new Dictionary<string, List<MethodInfo>>();
	public Dictionary<string, List<MethodInfo>> HookMethodAttributeCache { get; internal set; } = new Dictionary<string, List<MethodInfo>>();
	public List<string> IgnoredHooks { get; internal set; } = new List<string>();

	[JsonProperty]
	public string Name { get; set; }

	[JsonProperty]
	public VersionNumber Version { get; set; }

	[JsonProperty]
	public double TotalHookTime { get; internal set; }

	public bool HasInitialized { get; set; }
	public Type Type { get; set; }

	#region Tracking

	internal Stopwatch _trackStopwatch = new Stopwatch();

	public virtual void TrackStart()
	{
		if (!Community.IsServerFullyInitialized)
		{
			return;
		}

		var stopwatch = _trackStopwatch;
		if (stopwatch.IsRunning)
		{
			return;
		}
		stopwatch.Start();
	}
	public virtual void TrackEnd()
	{
		if (!Community.IsServerFullyInitialized)
		{
			return;
		}

		var stopwatch = _trackStopwatch;
		if (!stopwatch.IsRunning)
		{
			return;
		}
		stopwatch.Stop();
		TotalHookTime += stopwatch.Elapsed.TotalSeconds;
		stopwatch.Reset();
	}

	#endregion

	public void Unsubscribe(string hook)
	{
		if (IgnoredHooks.Contains(hook)) return;

		IgnoredHooks.Add(hook);
	}
	public void Subscribe(string hook)
	{
		if (!IgnoredHooks.Contains(hook)) return;

		IgnoredHooks.Remove(hook);
	}
	public bool IsHookIgnored(string hook)
	{
		return IgnoredHooks == null || IgnoredHooks.Contains(hook);
	}

	public T To<T>()
	{
		if (this is T result)
		{
			return result;
		}

		return default;
	}
}
