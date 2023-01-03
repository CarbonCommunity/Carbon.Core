using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core.Configuration;

public class DynamicConfigFile : ConfigFile, IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
	public JsonSerializerSettings Settings { get; set; } = new JsonSerializerSettings();

	public DynamicConfigFile(string filename) : base(filename)
	{
		_keyvalues = new Dictionary<string, object>();
		_settings = new JsonSerializerSettings();
		_settings.Converters.Add(new KeyValuesConverter());
		_chroot = Interface.Oxide.InstanceDirectory;
	}

	public override void Load(string filename = null)
	{
		filename = CheckPath(filename ?? Filename);
		string value = File.ReadAllText(filename);
		_keyvalues = JsonConvert.DeserializeObject<Dictionary<string, object>>(value, _settings);
	}

	public T ReadObject<T>(string filename = null)
	{
		filename = CheckPath(filename ?? Filename);
		T t;
		if (Exists(filename))
		{
			t = JsonConvert.DeserializeObject<T>(File.ReadAllText(filename), Settings);
		}
		else
		{
			t = Activator.CreateInstance<T>();
			WriteObject<T>(t, false, filename);
		}
		return t;
	}

	public override void Save(string filename = null)
	{
		filename = CheckPath(filename ?? Filename);
		string directoryName = Utility.GetDirectoryName(filename);
		if (directoryName != null && !Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		File.WriteAllText(filename, JsonConvert.SerializeObject(_keyvalues, Formatting.Indented, _settings));
	}

	public void WriteObject<T>(T config, bool sync = false, string filename = null)
	{
		if (config == null) config = Activator.CreateInstance<T>();

		filename = CheckPath(filename ?? Filename);
		var directoryName = Utility.GetDirectoryName(filename);
		if (directoryName != null && !Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}

		var data = JsonConvert.SerializeObject(config, Formatting.Indented, Settings);
		File.WriteAllText(filename, data);
		if (sync)
		{
			_keyvalues = JsonConvert.DeserializeObject<Dictionary<string, object>>(data, _settings);
		}
	}

	public bool Exists(string filename = null)
	{
		filename = CheckPath(filename ?? Filename);
		string directoryName = Utility.GetDirectoryName(filename);
		return (directoryName == null || Directory.Exists(directoryName)) && File.Exists(filename);
	}

	private string CheckPath(string filename)
	{
		filename = SanitizeName(filename);
		string fullPath = Path.GetFullPath(filename);
		if (!fullPath.StartsWith(_chroot, StringComparison.Ordinal))
		{
			Carbon.Logger.Log($"{filename} ||| {fullPath}");
			throw new Exception("Only access to Carbon directory!\nPath: " + fullPath);
		}
		return fullPath;
	}

	public static string SanitizeName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		name = name.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
		name = Regex.Replace(name, "[" + Regex.Escape(new string(Path.GetInvalidPathChars())) + "]", "_");
		name = Regex.Replace(name, "\\.+", ".");
		return name.TrimStart(new char[]
		{
			'.'
		});
	}

	public static string SanitiseName(string name)
	{
		return SanitizeName(name);
	}

	public void Clear()
	{
		_keyvalues.Clear();
	}

	public void Remove(string key)
	{
		_keyvalues.Remove(key);
	}

	public object this[string key]
	{
		get
		{
			object result;
			if (!_keyvalues.TryGetValue(key, out result))
			{
				return null;
			}
			return result;
		}
		set
		{
			_keyvalues[key] = value;
		}
	}

	public object this[string keyLevel1, string keyLevel2]
	{
		get
		{
			return Get(new string[]
			{
				keyLevel1,
				keyLevel2
			});
		}
		set
		{
			Set(new object[]
			{
				keyLevel1,
				keyLevel2,
				value
			});
		}
	}

	public object this[string keyLevel1, string keyLevel2, string keyLevel3]
	{
		get
		{
			return Get(new string[]
			{
				keyLevel1,
				keyLevel2,
				keyLevel3
			});
		}
		set
		{
			Set(new object[]
			{
				keyLevel1,
				keyLevel2,
				keyLevel3,
				value
			});
		}
	}

	public object ConvertValue(object value, Type destinationType)
	{
		if (!destinationType.IsGenericType)
		{
			return Convert.ChangeType(value, destinationType);
		}
		if (destinationType.GetGenericTypeDefinition() == typeof(List<>))
		{
			Type conversionType = destinationType.GetGenericArguments()[0];
			IList list = (IList)Activator.CreateInstance(destinationType);
			foreach (object value2 in ((IList)value))
			{
				list.Add(Convert.ChangeType(value2, conversionType));
			}
			return list;
		}
		if (destinationType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
		{
			Type conversionType2 = destinationType.GetGenericArguments()[0];
			Type conversionType3 = destinationType.GetGenericArguments()[1];
			IDictionary dictionary = (IDictionary)Activator.CreateInstance(destinationType);
			foreach (object obj in ((IDictionary)value).Keys)
			{
				dictionary.Add(Convert.ChangeType(obj, conversionType2), Convert.ChangeType(((IDictionary)value)[obj], conversionType3));
			}
			return dictionary;
		}
		throw new InvalidCastException("Generic types other than List<> and Dictionary<,> are not supported");
	}

	public T ConvertValue<T>(object value)
	{
		return (T)((object)ConvertValue(value, typeof(T)));
	}

	public object Get(params string[] path)
	{
		if (path.Length < 1)
		{
			throw new ArgumentException("path must not be empty");
		}
		object obj;
		if (!_keyvalues.TryGetValue(path[0], out obj))
		{
			return null;
		}
		for (int i = 1; i < path.Length; i++)
		{
			Dictionary<string, object> dictionary = obj as Dictionary<string, object>;
			if (dictionary == null || !dictionary.TryGetValue(path[i], out obj))
			{
				return null;
			}
		}
		return obj;
	}

	public T Get<T>(params string[] path)
	{
		return ConvertValue<T>(Get(path));
	}

	public void Set(params object[] pathAndTrailingValue)
	{
		if (pathAndTrailingValue.Length < 2)
		{
			throw new ArgumentException("path must not be empty");
		}
		string[] array = new string[pathAndTrailingValue.Length - 1];
		for (int i = 0; i < pathAndTrailingValue.Length - 1; i++)
		{
			array[i] = (string)pathAndTrailingValue[i];
		}
		object value = pathAndTrailingValue[pathAndTrailingValue.Length - 1];
		if (array.Length == 1)
		{
			_keyvalues[array[0]] = value;
			return;
		}
		object obj;
		if (!_keyvalues.TryGetValue(array[0], out obj))
		{
			obj = (_keyvalues[array[0]] = new Dictionary<string, object>());
		}
		for (int j = 1; j < array.Length - 1; j++)
		{
			if (!(obj is Dictionary<string, object>))
			{
				throw new ArgumentException("path is not a dictionary");
			}
			Dictionary<string, object> dictionary = (Dictionary<string, object>)obj;
			if (!dictionary.TryGetValue(array[j], out obj))
			{
				obj = (dictionary[array[j]] = new Dictionary<string, object>());
			}
		}
		((Dictionary<string, object>)obj)[array[array.Length - 1]] = value;
	}

	public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
	{
		return _keyvalues.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _keyvalues.GetEnumerator();
	}

	private Dictionary<string, object> _keyvalues;

	private readonly JsonSerializerSettings _settings;

	private readonly string _chroot;
}
