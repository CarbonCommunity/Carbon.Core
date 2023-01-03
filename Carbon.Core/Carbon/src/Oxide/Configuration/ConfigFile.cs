using System;
using System.IO;
using Newtonsoft.Json;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core.Configuration;

public abstract class ConfigFile
{
	[JsonIgnore]
	public string Filename { get; private set; }

	protected ConfigFile(string filename)
	{
		Filename = filename;
	}

	public static T Load<T>(string filename) where T : ConfigFile
	{
		T t = (T)((object)Activator.CreateInstance(typeof(T), new object[]
		{
			filename
		}));
		t.Load(null);
		return t;
	}

	public virtual void Load(string filename = null)
	{
		JsonConvert.PopulateObject(File.ReadAllText(filename ?? this.Filename), this);
	}

	public virtual void Save(string filename = null)
	{
		string contents = JsonConvert.SerializeObject(this, Formatting.Indented);
		File.WriteAllText(filename ?? this.Filename, contents);
	}
}
