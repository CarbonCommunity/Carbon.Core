extern alias MonoCecilStandalone;

using System;
using System.IO;
using Carbon.LoaderEx.Context;
using MonoCecilStandalone::Mono.Cecil;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.LoaderEx.ASM;

public sealed class Editor : MarshalByRefObject
{
	private readonly DefaultAssemblyResolver _resolver;

	public Editor()
	{
		_resolver = new DefaultAssemblyResolver();
		_resolver.AddSearchDirectory(Context.Directories.CarbonLib);
		_resolver.AddSearchDirectory(Context.Directories.CarbonModules);
		_resolver.AddSearchDirectory(Context.Directories.CarbonManaged);
		_resolver.AddSearchDirectory(Context.Directories.GameManaged);
	}

	public void SetAssemblyName(string file, string location, string name)
	{
		try
		{
			byte[] raw = File.ReadAllBytes(Path.Combine(location, file));
			if (raw == null) throw new Exception("Unable to read file");

			raw = SetAssemblyName(raw, name);

			using FileStream disk = new FileStream(Path.Combine(location, file), FileMode.Truncate);
			disk.Write(raw, 0, raw.Length);
		}
		catch (System.Exception) { throw; }
	}

	public byte[] SetAssemblyName(byte[] raw, string name)
	{
		try
		{
			using MemoryStream input = new MemoryStream(raw);
			AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(
				input, parameters: new ReaderParameters { AssemblyResolver = _resolver, InMemory = true });

			string assemblyName = assemblyDefinition.Name.Name;
			if (assemblyName.Equals(name, StringComparison.InvariantCultureIgnoreCase)) return raw;
			assemblyDefinition.Name = new AssemblyNameDefinition(name, assemblyDefinition.Name.Version);

			using MemoryStream output = new MemoryStream();
			assemblyDefinition.Write(output);
			return output.ToArray();
		}
		catch (System.Exception) { throw; }
	}

	public string GetAssemblyName(string file, string location)
	{
		try
		{
			byte[] raw = File.ReadAllBytes(Path.Combine(location, file));
			if (raw == null) throw new Exception("Unable to read file");
			return GetAssemblyName(raw);
		}
		catch (System.Exception) { throw; }
	}

	public string GetAssemblyName(byte[] raw)
	{
		try
		{
			using MemoryStream input = new MemoryStream(raw);
			AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(
				input, parameters: new ReaderParameters { AssemblyResolver = _resolver, InMemory = true });

			return assemblyDefinition.Name.Name;
		}
		catch (System.Exception) { throw; }
	}

	public Version GetAssemblyVersion(string file, string location)
	{
		try
		{
			byte[] raw = File.ReadAllBytes(Path.Combine(location, file));
			if (raw == null) throw new Exception("Unable to read file");
			return GetAssemblyVersion(raw);
		}
		catch (System.Exception) { throw; }
	}

	public Version GetAssemblyVersion(byte[] raw)
	{
		try
		{
			using MemoryStream input = new MemoryStream(raw);
			AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(
				input, parameters: new ReaderParameters { AssemblyResolver = _resolver, InMemory = true });

			return assemblyDefinition.Name.Version;
		}
		catch (System.Exception) { throw; }
	}
}
