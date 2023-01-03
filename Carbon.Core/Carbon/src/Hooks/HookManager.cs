
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Carbon.Core;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

internal sealed class HookManager : FacepunchBehaviour, IDisposable
{
	private bool _doReload;
	private Queue<Payload> _workQueue;
	private List<HookEx> _staticHooks;
	private List<HookEx> _dynamicHooks;
	private List<Subscription> _subscribers;

	private static readonly string[] Files =
	{
		Path.Combine(Defines.GetManagedFolder(), "hooks", "Carbon.Hooks.Base.dll"),
		Path.Combine(Defines.GetManagedFolder(), "hooks", "Carbon.Hooks.Extended.dll"),
	};

	public void Reload()
	{
		_doReload = true;
		enabled = false;
	}

	public void Dispose()
	{
		List<HookEx> hooks = _staticHooks.Concat(_dynamicHooks).ToList();
		foreach (HookEx hook in hooks) hook.Dispose();

		_workQueue = default;
		_subscribers = default;
		_staticHooks = _dynamicHooks = default;
	}

	internal void Awake()
	{
		Logger.Log(" Initialized hook processor...");

		_workQueue = new Queue<Payload>();
		_staticHooks = new List<HookEx>();
		_dynamicHooks = new List<HookEx>();
		_subscribers = new List<Subscription>();

		Logger.Log(" Updating hooks...");
		enabled = false;

		Carbon.Hooks.Updater.DoUpdate((bool result) =>
		{
			if (!result)
				Logger.Error($" Unable to update the hooks at this time, please try again later");

			enabled = true;
		});
	}

	internal void OnEnable()
	{
		_staticHooks.Clear();
		_dynamicHooks.Clear();

		foreach (string file in Files)
		{
			if (Supervisor.ASM.IsLoaded(Path.GetFileName(file)))
				Supervisor.ASM.UnloadModule(file, false);
			LoadHooksFromFile(file);
		}

		if (_staticHooks.Count > 0)
		{
			Logger.Log($" - Installing static hooks");
			// this is based on the assumption that a static hook will never have
			// a dependency on another hook thus it will be always applied first
			foreach (HookEx hook in _staticHooks.Where(x => !x.IsInstalled))
				hook.ApplyPatch();
		}

		if (_dynamicHooks.Count > 0)
		{
			Logger.Log($" - Installing dynamic hooks");
			foreach (HookEx hook in _dynamicHooks.Where(x => HookHasSubscribers(x.HookName)))
				_workQueue.Enqueue(item: new Payload(hook.HookName, null, "Carbon.Core"));
		}

		try
		{
			if (_subscribers.Count == 0) return;

			// the code block bellow is ugly but the idea is to update the oxide
			// hook list and reload the running plugins when HookManager "restarts".

			Carbon.Core.HookValidator.Refresh();

			foreach (Subscription item in _subscribers.ToList())
			{
				_subscribers.Remove(item);
				Subscribe(item.HookName, item.Subscriber);
			}
		}
		catch (System.Exception e)
		{
			Logger.Error("Couldn't refresh HookValidator", e);
		}
	}

	internal void OnDisable()
	{
		Logger.Log(" Stopping hook processor...");

		if (_dynamicHooks.Count > 0)
		{
			Logger.Log($" - Uninstalling dynamic hooks");
			// the disable event will make sure the patches are removed but the
			// subscriber list is kept unchanged. this will be used on hot reloads.
			foreach (HookEx hook in _dynamicHooks.Where(x => x.IsInstalled))
				hook.RemovePatch();
		}

		if (_staticHooks.Count > 0)
		{
			Logger.Log($" - Uninstalling static hooks");
			// reverse order, dynamics get removed first, then statics.
			foreach (HookEx hook in _staticHooks.Where(x => x.IsInstalled))
				hook.RemovePatch();
		}

		if (!_doReload) return;

		Logger.Log(" Reloading hook processor...");
		_doReload = false;
		enabled = true;
	}

	internal void OnDestroy()
		=> Dispose();

	internal void Update()
	{
		// get the fuck out as fast as possible
		if (_workQueue.Count == 0) return;

		try
		{
			Payload payload = _workQueue.Dequeue();
			List<HookEx> hooks = GetHookByName(payload.HookName).ToList();

			if (payload.Identifier != null)
				hooks.RemoveAll(x => x.Identifier != payload.Identifier);

			foreach (HookEx hook in hooks)
			{
				int subscribers = GetHookSubscriberCount(hook.HookName);
				Logger.Debug($"Hook '{hook.HookName}[{hook.Identifier}]' has {subscribers} subscriber(s)");

				// static hooks are a special case
				if (hook.IsStaticHook) return;

				bool hasSubscribers = HookHasSubscribers(hook.HookName);
				bool isInstalled = hook.IsInstalled;

				switch (hasSubscribers)
				{
					// Not installed but has subs, install
					case true when !isInstalled:
						Install(hook, payload.Requester);
						break;
					// Installed but no subs found, uninstall
					case false when isInstalled:
						Uninstall(hook, payload.Requester);
						break;
				}
			}
		}
		catch (System.Exception e)
		{
			Logger.Error("HookManager.Update() failed", e);
		}
	}

	private void LoadHooksFromFile(string fileName)
	{
		Assembly hooks;

		try
		{
			// delegates asm loading to Carbon.Loader 
			hooks = Supervisor.ASM.LoadModule(fileName);

			if (hooks == null)
				throw new Exception($"External hooks module '{fileName}' not found");
		}
		catch (System.Exception e)
		{
			Logger.Error(e.Message, e);
			return;
		}

		Type t = hooks.GetType("Carbon.Hooks.HookAttribute.Patch")
			?? typeof(HookAttribute.Patch);

		IEnumerable<TypeInfo> types = hooks.DefinedTypes
			.Where(x => Attribute.IsDefined(x, t)).ToList();

		int x = 0, y = 0;
		foreach (TypeInfo type in types)
		{
			try
			{
				HookEx hook = new HookEx(type);

				if (hook is null)
					throw new Exception($"Hook is null, this is a bug");

				if (IsHookLoaded(hook))
					throw new Exception($"Found duplicated hook '{hook.HookName}'");

				if (hook.IsStaticHook)
				{
					y++;
					_staticHooks.Add(hook);
					Logger.Debug($"Loaded static hook '{hook.HookName}'", 3);
				}
				else
				{
					x++;
					_dynamicHooks.Add(hook);
					Logger.Debug($"Loaded dynamic hook '{hook.HookName}'", 3);
				}
			}
			catch (System.Exception e)
			{
				Logger.Error($"Error while parsing '{type.Name}'", e);
				continue;
			}
		}

		Logger.Log($" - Successfully loaded static:{y} dynamic:{x} ({types.Count()}) hooks from assembly '{fileName}'");
	}

	internal void Subscribe(string hookName, string requester)
	{
		try
		{
			List<HookEx> hooks = GetHookByName(hookName).ToList();
			if (hooks.Count == 0) throw new Exception($"Hook fileName not found");

			foreach (HookEx hook in hooks.Where(hook => !HookIsSubscribedBy(hook.HookName, requester)))
			{
				AddSubscriber(hook.HookName, requester);
				_workQueue.Enqueue(item: new Payload(hook.HookName, hook.Identifier, requester));
				Logger.Debug($"Subscribe to '{hook.HookName}[{hook.Identifier}]' by '{requester}'");
			}
		}
		catch (System.Exception e)
		{
			Logger.Error($"Error while subscribing hook '{hookName}'", e);
			return;
		}
	}

	internal void Unsubscribe(string hookName, string requester)
	{
		try
		{
			List<HookEx> hooks = GetHookByName(hookName).ToList();
			if (hooks.Count == 0) throw new Exception($"Hook fileName not found");

			foreach (HookEx hook in hooks.Where(hook => HookIsSubscribedBy(hook.HookName, requester)))
			{
				RemoveSubscriber(hook.HookName, requester);
				_workQueue.Enqueue(item: new Payload(hook.HookName, hook.Identifier, requester));
				Logger.Debug($"Unsubscribe from '{hook.HookName}[{hook.Identifier}]' by '{requester}'");
			}
		}
		catch (System.Exception e)
		{
			Logger.Error($"Error while unsubscribing hook '{hookName}'", e);
			return;
		}
	}

	private void Install(HookEx hook, string requester)
	{
		try
		{
			if (hook.HasDependencies())
			{
				foreach (string item in hook.Dependencies)
				{
					List<HookEx> dependencies = GetHookByName(item).ToList();

					foreach (HookEx dependency in dependencies)
					{
						if (dependency is null)
							throw new Exception($"Dependency '{item}' is null, this is a bug");

						if (dependency.IsInstalled) continue;

						if (!dependency.ApplyPatch())
							throw new Exception($"Dependency '{dependency.HookName}[{dependency.Identifier}]' installation failed");

						AddSubscriber(dependency.HookName, requester);
						Logger.Log($"Installed dependency '{dependency.HookName}[{dependency.Identifier}]'");
					}
				}
			}

			if (!hook.ApplyPatch())
				throw new Exception($"Hook '{hook.HookName}[{hook.Identifier}]' installation failed");
			Logger.Log($"Installed hook '{hook.HookName}'[{hook.Identifier}]");
		}
		catch (System.Exception e)
		{
			hook.LastError = e;
			hook.Status = HookState.Failure;
			Logger.Error($"Install hook '{hook.HookName}[{hook.Identifier}]' failed", e);
		}
	}

	private void Uninstall(HookEx hook, string requester)
	{
		try
		{
			if (!hook.RemovePatch())
				throw new Exception($"Hook '{hook.HookName}[{hook.Identifier}]' uninstallation failed");
			Logger.Log($"Uninstalled hook '{hook.HookName}[{hook.Identifier}]'");

			if (!hook.HasDependencies()) return;

			foreach (string item in hook.Dependencies)
			{
				List<HookEx> dependencies = GetHookByName(item).ToList();

				foreach (HookEx dependency in dependencies)
				{
					if (dependency is null)
						throw new Exception($"Dependency '{item}' is null, this is a bug");

					if (dependency.IsInstalled) continue;

					if (!dependency.RemovePatch())
						throw new Exception($"Dependency '{dependency.HookName}[{dependency.Identifier}]' uninstallation failed");

					RemoveSubscriber(dependency.HookName, requester);
					Logger.Log($"Uninstalled dependency '{dependency.HookName}[{dependency.Identifier}]'");
				}
			}
		}
		catch (System.Exception e)
		{
			hook.LastError = e;
			hook.Status = HookState.Failure;
			Logger.Error($"Install hook '{hook.HookName}[{hook.Identifier}]' failed", e);
		}
	}

	private IEnumerable<HookEx> GetHookByName(string name)
		=> _dynamicHooks.Where(x => x.HookName == name) ?? null;

	private HookEx GetHookById(string identifier)
		=> _dynamicHooks.FirstOrDefault(x => x.Identifier == identifier) ?? null;

	private bool IsHookLoaded(HookEx hook)
		=> _dynamicHooks.Any(x => x.PatchMethodName == hook.PatchMethodName);

	internal bool IsHookLoaded(string hookName)
	{
		List<HookEx> hooks = GetHookByName(hookName).ToList();
		return hooks.Count != 0 && hooks.Any(IsHookLoaded);
	}

	internal IEnumerable<string> LoadedStaticHooksName
	{ get => _staticHooks.Where(x => x.IsLoaded).Select(x => x.HookName); }

	internal IEnumerable<string> LoadedDynamicHooksName
	{ get => _dynamicHooks.Where(x => x.IsLoaded).Select(x => x.HookName); }


	internal IEnumerable<HookEx> LoadedStaticHooks
	{ get => _staticHooks.Where(x => x.IsLoaded); }

	internal IEnumerable<HookEx> LoadedDynamicHooks
	{ get => _dynamicHooks.Where(x => x.IsLoaded); }

	internal IEnumerable<HookEx> InstalledStaticHooks
	{ get => _staticHooks.Where(x => x.IsInstalled); }

	internal IEnumerable<HookEx> InstalledDynamicHooks
	{ get => _dynamicHooks.Where(x => x.IsInstalled); }


	internal bool HookIsSubscribedBy(string hookName, string subscriber)
		=> _subscribers?.Where(x => x.HookName == hookName).Any(x => x.Subscriber == subscriber) ?? false;

	internal bool HookHasSubscribers(string hookName)
		=> _subscribers?.Any(x => x.HookName == hookName) ?? false;

	internal int GetHookSubscriberCount(string hookName)
		=> _subscribers.Where(x => x.HookName == hookName).ToList().Count;


	internal void AddSubscriber(string hookName, string subscriber)
		=> _subscribers.Add(new Subscription { HookName = hookName, Subscriber = subscriber });

	internal void RemoveSubscriber(string hookName, string subscriber)
		=> _subscribers.RemoveAll(x => x.HookName == hookName && x.Subscriber == subscriber);

}
