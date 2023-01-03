using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Carbon.LoaderEx.Context;
using Carbon.LoaderEx.Utility;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.LoaderEx.ASM;

internal sealed class Item : IDisposable
{
	private List<string> _aliases;

	internal byte[] Bytes
	{ get; private set; }

	internal string Location
	{ get; private set; }

	internal AssemblyName Name
	{ get; private set; }

	public void Dispose()
	{
		_aliases.Clear();
		_aliases = default;
		Bytes = null;
	}

	internal Item()
	{
		_aliases = new List<string>(1);
	}

	internal Item(string name, string location = null) : this()
	{
		Name = new AssemblyName(name);
		string file = (name.EndsWith(".dll")) ? $"{Name.Name}" : $"{Name.Name}.dll";
		Location = (string.IsNullOrEmpty(location)) ? FindFile(file) : Path.Combine(location, file);
		Bytes = ReadFile(Location);

#if DEBUG
		Logger.Debug($" - Created new cached instance of '{Name.Name}'");
#endif
	}

	public void AddAlias(string name)
	{
		if (_aliases.Contains(name))
		{
			return;
		}

		_aliases.Add(name);
#if DEBUG
		Logger.Debug($" - Added new alias '{name}' for '{Name.Name}");
#endif
	}

	public void RemoveAlias(string name)
	{
		if (!_aliases.Contains(name))
		{
			return;
		}

		_aliases.Remove(name);
#if DEBUG
		Logger.Debug($" - Removed alias '{name}' for '{Name.Name}");
#endif
	}

	public bool IsMatch(string needle)
	{
		if (Name == null) return false;
		AssemblyName name = new AssemblyName(needle);

		if (Name.FullName == name.FullName) return true;
		else if (Name.Name == name.Name) return true;
		else if (_aliases.Contains(name.Name)) return true;
		else return false;
	}

	private static string FindFile(string file)
	{
#if DEBUG
		Logger.Debug($" - FindFile: {file}");
#endif

		string location = file switch
		{
			"Carbon.dll" => Directories.CarbonManaged,
			"Carbon.Loader.dll" => Directories.CarbonManaged,
			"Carbon.Doorstop.dll" => Directories.CarbonManaged,

			"Carbon.Hooks.Base.dll" => Directories.CarbonHooks,
			"Carbon.Hooks.Extended.dll" => Directories.CarbonHooks,

			_ => null
		};

		if (location != null)
			return Path.Combine(location, file);

		if (location == null) // Module search
		{
			string needle = Path.Combine(Directories.CarbonModules, file);
			if (File.Exists(needle)) return needle;
		}

		if (location == null) // Carbon reference search
		{
			string needle = Path.Combine(Directories.CarbonLib, file);
			if (File.Exists(needle)) return needle;
		}

		if (location == null) // Game reference search
		{
			string needle = Path.Combine(Directories.GameManaged, file);
			if (File.Exists(needle)) return needle;
		}

		throw new FileNotFoundException(file);
	}

	private byte[] ReadFile(string file)
	{
		byte[] raw = default;

#if DEBUG
		Logger.Debug($" - ReadFile: {file}");
#endif

		// TODO: let's think about packaging instead
		if (file.EndsWith("Carbon.dll"))
		{
			string name = $"Carbon_{Guid.NewGuid():N}";
			AddAlias(name);

			using Sandbox<Editor> isolated = new Sandbox<Editor>();
			isolated.Do.SetAssemblyName(Path.GetFileName(file), Path.GetDirectoryName(file), name);
		}

		try
		{
			if (!File.Exists(file)) throw new FileNotFoundException();
			raw = File.ReadAllBytes(file);

			if (IndexOf(raw, new byte[4] { 0x01, 0xdc, 0x7f, 0x01 }) == 0)
			{
				byte[] tmp = raw, sha1 = new byte[20];
				Buffer.BlockCopy(tmp, 4, sha1, 0, 20);
				raw = Package(sha1, tmp, 24);
			}
		}
		catch (System.Exception e)
		{
			Logger.Error($" - Unable to load file '{Path.GetFileName(file)}' [{e.GetType()}]");
		}
		finally
		{
#if DEBUG
			Logger.Debug($" - Loading file '{Path.GetFileName(file)}', read {raw.Length} bytes from disk");
#endif
		}

		if (file.Contains("Carbon.Hooks."))
		{
			string name = $"Carbon.Hooks_{Guid.NewGuid():N}";
			AddAlias(name);

			using Sandbox<Editor> isolated = new Sandbox<Editor>();
			raw = isolated.Do.SetAssemblyName(raw, name);
		}

		return raw;
	}

	private static byte[] Package(IReadOnlyList<byte> a, IReadOnlyList<byte> b, int c = 0)
	{
		byte[] retvar = new byte[b.Count - c];
		for (int i = c; i < b.Count; i++)
			retvar[i - c] = (byte)(b[i] ^ a[(i - c) % a.Count]);
		return retvar;
	}

	private static int IndexOf(IReadOnlyList<byte> haystack, IReadOnlyList<byte> needle)
	{
		int len = needle.Count;
		int limit = haystack.Count - len;

		for (int i = 0; i <= limit; i++)
		{
			int k = 0;
			for (; k < len; k++)
				if (needle[k] != haystack[i + k]) break;
			if (k == len) return i;
		}
		return -1;
	}
}
