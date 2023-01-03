using System;
using System.Collections.Generic;
using System.IO;
using Carbon.Core;
using Carbon.Extensions;
using Facepunch;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon;

public class FileLogger : IDisposable
{
	public string Name { get; set; } = "default";

	/// <summary>
	/// By default, each log file gets split when it reaches exactly 2.5MB in file size and sent in the archive folder.
	/// </summary>
	public int SplitSize { get; set; } = (int)(2.5f * 1000000f);

	internal bool _hasInit;
	internal List<string> _buffer = new();
	internal StreamWriter _file;

	public FileLogger() { }
	public FileLogger(string name)
	{
		Name = name;
	}

	public virtual void Init(bool archive = false)
	{
		if (_hasInit) return;

		var path = Path.Combine(Defines.GetLogsFolder(), $"{Name}.log");

		try
		{
			File.Delete(path);
			File.Delete(Harmony.FileLog.logPath);
			File.Delete(HarmonyLib.FileLog.LogPath);
		}
		catch { }

		_hasInit = true;

		if (archive)
		{
			if (OsEx.File.Exists(path))
			{
				OsEx.File.Move(path, Path.Combine(Defines.GetLogsFolder(), "archive", $"{Name}.{DateTime.Now:yyyy.MM.dd.HHmmss}.log"));
			}
		}

		_file = new StreamWriter(path, append: true);
	}
	public virtual void Dispose()
	{
		_file.Flush();
		_file.Close();
		_file.Dispose();

		_hasInit = false;
	}
	public virtual void _flush()
	{
		var buffer = Pool.GetList<string>();
		buffer.AddRange(_buffer);

		foreach (var line in buffer)
		{
			_file?.WriteLine(line);
		}

		_file.Flush();
		_buffer.Clear();
		Pool.FreeList(ref buffer);

		if (_file.BaseStream.Length > SplitSize)
		{
			Dispose();
			Init(archive: true);
		}
	}
	internal void _queueLog(string message)
	{
		if (Community.IsConfigReady && Community.Runtime.Config.LogFileMode == 0) return;

		_buffer.Add($"[{Logger.GetDate()}] {message}");
		if (Community.IsConfigReady && Community.Runtime.Config.LogFileMode == 2) _flush();
	}
}
