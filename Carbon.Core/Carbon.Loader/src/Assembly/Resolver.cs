using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Carbon.LoaderEx.Utility;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.LoaderEx.ASM;

internal sealed class ResolverEx : IDisposable
{
	private List<Item> _cache;
	private AppDomain _domain;

	public void Dispose()
	{
		foreach (Item item in _cache)
			item.Dispose();

		_cache.Clear();
		_cache = default;

		_domain.AssemblyResolve -= ResolveAssembly;
	}

	internal void RegisterDomain(AppDomain domain)
	{
		_cache = new List<Item>();
		_domain = domain;
		_domain.AssemblyResolve += ResolveAssembly;
		Logger.Log($"Resolver attached to '{_domain.FriendlyName}'");
	}

	internal Assembly ResolveAssembly(object sender, ResolveEventArgs args)
	{
		string name = args.Name;
		string requester = args.RequestingAssembly?.GetName().Name ?? "unknown";

		Item retvar = ResolveAssembly(name, requester);
		return (retvar.Bytes == null) ? default : Assembly.Load(retvar.Bytes);
	}

	internal Item ResolveAssembly(string name, string requester)
	{
#if DEBUG
		Logger.Debug($"Resolve '{name}' requested by '{requester}'");
#endif

		Item retvar = null;
		AssemblyName assemblyName = Normalize(name);

		if (_cache.Count > 0)
		{
#if DEBUG
			Logger.Debug($" - Searching {_cache.Count} cached items");
#endif
			retvar = _cache.SingleOrDefault<Item>(x => x.IsMatch(assemblyName.Name)) ?? null;
		}

		if (retvar == null)
		{
			Logger.Debug($" - Cache miss");
			retvar = new Item(assemblyName.Name, Path.GetDirectoryName(name));
			if (retvar.Bytes != null) _cache.Add(retvar);
		}
		else Logger.Debug($" - Cache hit");

		if (retvar.Bytes == null)
		{
			Logger.Debug($"Unresolved: {name}");
			return default;
		}
		else
		{
			Logger.Debug($"Resolved: {retvar.Name.FullName}");
			return retvar;
		}
	}

	internal bool RemoveCache(string name)
	{
#if DEBUG
		Logger.Debug($"Remove from cache '{name}'");
#endif

		Item retvar = null;
		AssemblyName assemblyName = Normalize(name);

		if (_cache.Count > 0)
		{
#if DEBUG
			Logger.Debug($" - Searching {_cache.Count} cached items");
#endif
			retvar = _cache.SingleOrDefault<Item>(x => x.IsMatch(assemblyName.Name)) ?? null;
		}

		if (retvar != null)
		{
			Logger.Debug($" - Cache hit");
			return _cache.Remove(retvar);
		}

		return false;
	}

	private AssemblyName Normalize(string name)
	{
		// deals with full paths /foo/bar/foobar.dll
		if (name.Contains(Path.DirectorySeparatorChar))
			name = Path.GetFileNameWithoutExtension(name);

		// deals with short paths foobar.dll
		name = name.Replace(".dll", string.Empty);

		// deals with aliases foo and foo_xxxxx
		const string pattern = @"(?i)^((?:\w+)(?:\.(?:\w+))?)_([0-9a-f]+)\.dll$";
		Match match = Regex.Match(name, pattern, RegexOptions.Compiled);
		if (match.Success) name = match.Groups[1].Value;

		return new AssemblyName(name);
	}
}
