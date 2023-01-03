using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Carbon.Core;
using Facepunch;
using UnityEngine;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Base;

public class BaseProcessor : FacepunchBehaviour, IDisposable
{
	public Dictionary<string, Instance> InstanceBuffer { get; private set; }
	public List<string> IgnoreList { get; private set; }

	public virtual bool EnableWatcher => true;
	public virtual string Folder => string.Empty;
	public virtual string Extension => string.Empty;
	public virtual float Rate => 0.2f;
	public virtual Type IndexedType => null;
	public FileSystemWatcher Watcher { get; private set; }

	internal WaitForSeconds _wfsInstance;

	public bool IsInitialized { get; set; }

	public virtual void Start()
	{
		if (IsInitialized) return;

		InstanceBuffer = new Dictionary<string, Instance>();
		IgnoreList = new List<string>();

		DontDestroyOnLoad(gameObject);

		IsInitialized = true;

		_wfsInstance = new WaitForSeconds(Rate);

		StopAllCoroutines();
		StartCoroutine(Run());

		Watcher?.Dispose();
		Watcher = null;
		if (!string.IsNullOrEmpty(Extension) && !string.IsNullOrEmpty(Folder))
		{
			Watcher = new FileSystemWatcher(Folder)
			{
				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.FileName,
				Filter = $"*{Extension}"
			};
			Watcher.Created += _onCreated;
			Watcher.Changed += _onChanged;
			Watcher.Renamed += _onRenamed;
			Watcher.Deleted += _onRemoved;
			Watcher.IncludeSubdirectories = true;
			Watcher.EnableRaisingEvents = true;
		}

		Carbon.Logger.Log($" Initialized {IndexedType?.Name} processor...");
	}
	public virtual void OnDestroy()
	{
		IsInitialized = false;

		Carbon.Logger.Log($"{IndexedType?.Name} processor has been unloaded.");
	}
	public virtual void Dispose()
	{
		Clear();
	}

	public virtual IEnumerator Run()
	{
		var _tempBuffer = new Dictionary<string, Instance>();

		while (true)
		{
			if (!EnableWatcher)
			{
				yield return null;
				continue;
			}

			yield return _wfsInstance;

			foreach (var element in InstanceBuffer) _tempBuffer.Add(element.Key, element.Value);

			foreach (var element in _tempBuffer)
			{
				if (element.Value == null)
				{
					var instance = Activator.CreateInstance(IndexedType) as Instance;
					instance.File = Path.Combine(Defines.GetPluginsFolder(), $"{element.Key}{Extension}");
					instance.Execute();

					InstanceBuffer[element.Key] = instance;
					continue;
				}

				if (element.Value.IsRemoved)
				{
					Clear(element.Key, element.Value);
					yield return null;
					break;
				}

				if (element.Value.IsDirty)
				{
					Process(element.Key, element.Value);
					yield return null;
				}
			}

			_tempBuffer.Clear();

			yield return null;
		}
	}

	public virtual void Prepare(string file)
	{
		if (file.StartsWith("http"))
		{
			var filename = Path.GetFileName(file.Replace("http://", string.Empty).Replace("https://", string.Empty));
			Prepare(filename, file);
		}
		else Prepare(Path.GetFileNameWithoutExtension(file), file);
	}
	public virtual void Prepare(string id, string file)
	{
		if (IgnoreList.Contains(file)) return;

		Carbon.Logger.Log($" Loading plugin '{id}'...");

		Remove(id);

		var instance = Activator.CreateInstance(IndexedType) as Instance;
		InstanceBuffer.Add(id, instance);

		instance.File = file;
		instance.Execute();
	}
	public virtual void Remove(string id)
	{
		var existent = !InstanceBuffer.ContainsKey(id) ? null : InstanceBuffer[id];
		existent?.Dispose();

		if (InstanceBuffer.ContainsKey(id)) InstanceBuffer.Remove(id);
	}
	public virtual void Clear()
	{
		foreach (var item in InstanceBuffer)
		{
			try
			{
				item.Value?.Dispose();
			}
			catch (Exception ex) { Carbon.Logger.Error($" Processor error: '{item.Key}'", ex); }
		}

		InstanceBuffer.Clear();
	}
	public virtual void Ignore(string file)
	{
		if (!IgnoreList.Contains(file)) IgnoreList.Add(file);
	}
	public virtual void ClearIgnore(string file)
	{
		IgnoreList.RemoveAll(x => x == file);
	}
	public T Get<T>(string id) where T : Instance
	{
		if (InstanceBuffer.TryGetValue(id, out var instance))
		{
			return (T)instance;
		}

		return default;
	}

	public virtual void Clear(string id, Instance instance)
	{
		instance?.Dispose();
		Pool.Free(ref instance);
		Remove(id);
	}
	public virtual void Process(string id, Instance instance)
	{
		var file = instance.File;

		Clear(id, instance);
		Prepare(id, file);
	}

	internal void _onCreated(object sender, FileSystemEventArgs e)
	{
		InstanceBuffer.Add(Path.GetFileNameWithoutExtension(e.Name), null);
	}
	internal void _onChanged(object sender, FileSystemEventArgs e)
	{
		if (InstanceBuffer.TryGetValue(Path.GetFileNameWithoutExtension(e.Name), out var mod)) mod.SetDirty();
	}
	internal void _onRenamed(object sender, RenamedEventArgs e)
	{
		if (InstanceBuffer.TryGetValue(Path.GetFileNameWithoutExtension(e.OldName), out var mod)) mod.MarkDeleted();
		InstanceBuffer.Add(Path.GetFileNameWithoutExtension(e.Name), null);
	}
	internal void _onRemoved(object sender, FileSystemEventArgs e)
	{
		if (InstanceBuffer.TryGetValue(Path.GetFileNameWithoutExtension(e.Name), out var mod)) mod.MarkDeleted();
	}

	public class Instance : IDisposable
	{
		public virtual Parser Parser { get; }

		public string File { get; set; }

		internal bool _hasChanged;
		internal bool _hasRemoved;

		public virtual void Dispose() { }
		public virtual void Execute() { }

		public bool HasSucceeded { get; set; }
		public bool IsDirty => _hasChanged;
		public bool IsRemoved => _hasRemoved;

		public void SetDirty()
		{
			_hasChanged = true;
		}
		public void MarkDeleted()
		{
			_hasRemoved = true;
		}
	}
	public class Parser
	{
		public virtual void Process(string input, out string output)
		{
			output = null;
		}
	}
}
