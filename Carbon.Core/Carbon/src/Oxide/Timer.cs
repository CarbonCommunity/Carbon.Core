using System;
using System.Collections.Generic;
using Facepunch;
using static Oxide.Plugins.RustPlugin;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Plugins;

public class Timers
{
	public RustPlugin Plugin { get; }
	internal List<Timer> _timers { get; set; } = new List<Timer>();

	public Timers() { }
	public Timers(RustPlugin plugin)
	{
		Plugin = plugin;
	}

	public bool IsValid()
	{
		return Plugin != null && Plugin.persistence != null;
	}
	public void Clear()
	{
		foreach (var timer in _timers)
		{
			timer.Destroy();
		}

		_timers.Clear();
		_timers = null;
	}

	public Persistence Persistence => Plugin.persistence;

	public Timer In(float time, Action action)
	{
		if (!IsValid()) return null;

		var timer = new Timer(Persistence, action, Plugin);
		var activity = new Action(() =>
		{
			try
			{
				action?.Invoke();
				timer.TimesTriggered++;
			}
			catch (Exception ex) { Plugin.LogError($"Timer {time}s has failed:", ex); }

			timer.Destroy();
			Pool.Free(ref timer);
		});

		timer.Callback = activity;
		Persistence.Invoke(activity, time);
		return timer;
	}
	public Timer Once(float time, Action action)
	{
		return In(time, action);
	}
	public Timer Every(float time, Action action)
	{
		if (!IsValid()) return null;

		var timer = new Timer(Persistence, action, Plugin);
		var activity = new Action(() =>
		{
			try
			{
				action?.Invoke();
				timer.TimesTriggered++;
			}
			catch (Exception ex)
			{
				Plugin.LogError($"Timer {time}s has failed:", ex);

				timer.Destroy();
				Pool.Free(ref timer);
			}
		});

		timer.Callback = activity;
		Persistence.InvokeRepeating(activity, time, time);
		return timer;
	}
	public Timer Repeat(float time, int times, Action action)
	{
		if (!IsValid()) return null;

		var timer = new Timer(Persistence, action, Plugin);
		var activity = new Action(() =>
		{
			try
			{
				action?.Invoke();
				timer.TimesTriggered++;

				if (timer.TimesTriggered >= times)
				{
					timer.Dispose();
					Pool.Free(ref timer);
				}
			}
			catch (Exception ex)
			{
				Plugin.LogError($"Timer {time}s has failed:", ex);

				timer.Destroy();
				Pool.Free(ref timer);
			}
		});

		timer.Callback = activity;
		Persistence.InvokeRepeating(activity, time, time);
		return timer;
	}
}

public class Timer : IDisposable
{
	public RustPlugin Plugin { get; set; }

	public Action Activity { get; set; }
	public Action Callback { get; set; }
	public Persistence Persistence { get; set; }
	public int Repetitions { get; set; }
	public int TimesTriggered { get; set; }
	public bool Destroyed { get; set; }

	public Timer() { }
	public Timer(Persistence persistence, Action activity, RustPlugin plugin = null)
	{
		Persistence = persistence;
		Activity = activity;
		Plugin = plugin;
	}

	public void Reset(float delay = -1f, int repetitions = 1)
	{
		Repetitions = repetitions;

		if (Destroyed)
		{
			Carbon.Logger.Warn($"You cannot restart a timer that has been destroyed.");
			return;
		}

		if (Persistence != null)
		{
			Persistence.CancelInvoke(Callback);
			Persistence.CancelInvokeFixedTime(Callback);
		}

		TimesTriggered = 0;

		if (Repetitions == 1)
		{
			Callback = new Action(() =>
			{
				try
				{
					Activity?.Invoke();
					TimesTriggered++;
				}
				catch (Exception ex) { Plugin.LogError($"Timer {delay}s has failed:", ex); }

				Destroy();
			});

			Persistence.Invoke(Callback, delay);
		}
		else
		{
			Callback = new Action(() =>
			{
				try
				{
					Activity?.Invoke();
					TimesTriggered++;

					if (TimesTriggered >= Repetitions)
					{
						Dispose();
					}
				}
				catch (Exception ex)
				{
					Plugin.LogError($"Timer {delay}s has failed:", ex);

					Destroy();
				}
			});

			Persistence.InvokeRepeating(Callback, delay, delay);
		}
	}
	public void Destroy()
	{
		if (Destroyed) return;
		Destroyed = true;

		if (Persistence != null)
		{
			Persistence.CancelInvoke(Callback);
			Persistence.CancelInvokeFixedTime(Callback);
		}

		if (Callback != null)
		{
			Callback = null;
		}
	}
	public void Dispose()
	{
		Destroy();
	}
}
