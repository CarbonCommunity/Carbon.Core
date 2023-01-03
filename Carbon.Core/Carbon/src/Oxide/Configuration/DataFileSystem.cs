using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Oxide.Core.Configuration;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core;

public class DataFileSystem
{
	public string Directory { get; private set; }

	public DataFileSystem(string directory)
	{
		Directory = directory;
		_datafiles = new Dictionary<string, DynamicConfigFile>();
	}

	public DynamicConfigFile GetFile(string name)
	{
		name = DynamicConfigFile.SanitizeName(name);
		DynamicConfigFile dynamicConfigFile;
		if (_datafiles.TryGetValue(name, out dynamicConfigFile))
		{
			return dynamicConfigFile;
		}
		dynamicConfigFile = new DynamicConfigFile(Path.Combine(Directory, name + ".json"));
		_datafiles.Add(name, dynamicConfigFile);
		return dynamicConfigFile;
	}

	public bool ExistsDatafile(string name)
	{
		return GetFile(name).Exists(null);
	}

	public DynamicConfigFile GetDatafile(string name)
	{
		var file = GetFile(name);
		if (file.Exists(null))
		{
			file.Load(null);
		}
		else
		{
			file.Save(null);
		}
		return file;
	}

	public string[] GetFiles(string path = "", string searchPattern = "*")
	{
		return System.IO.Directory.GetFiles(Path.Combine(Directory, path), searchPattern);
	}

	public void SaveDatafile(string name)
	{
		GetFile(name).Save(null);
	}

	public T ReadObject<T>(string name)
	{
		if (!ExistsDatafile(name))
		{
			T t = Activator.CreateInstance<T>();
			WriteObject(name, t, false);
			return t;
		}
		return GetFile(name).ReadObject<T>(null);
	}

	public void WriteObject<T>(string name, T Object, bool sync = false)
	{
		GetFile(name).WriteObject<T>(Object, sync, null);
	}

	public void ForEachObject<T>(string name, Action<T> callback)
	{
		string folder = DynamicConfigFile.SanitizeName(name);
		foreach (DynamicConfigFile dynamicConfigFile in from d in _datafiles
														where d.Key.StartsWith(folder)
														select d into a
														select a.Value)
		{
			if (callback != null)
			{
				callback(dynamicConfigFile.ReadObject<T>(null));
			}
		}
	}

	private readonly Dictionary<string, DynamicConfigFile> _datafiles;
}
