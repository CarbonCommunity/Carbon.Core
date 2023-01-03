using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Carbon.Base;
using Carbon.Core;
using Carbon.Extensions;
using Carbon.Processors;
using Newtonsoft.Json;
using Oxide.Core.Libraries;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Modules;

public class DRMModule : CarbonModule<DRMConfig, DRMData>
{
	public override string Name => "DRM";
	public override Type Type => typeof(DRMModule);
	public override bool EnabledByDefault => true;

	public override void Init()
	{
		base.Init();

		foreach (var processor in Config.DRMs)
		{
			processor.Initialize();
		}
	}
	public override void Dispose()
	{
		foreach (var processor in Config.DRMs)
		{
			processor.Uninitialize();
		}

		base.Dispose();
	}

	[ConsoleCommand("drmtest")]
	private void GenerateTest(ConsoleSystem.Arg args)
	{
		if (!args.IsPlayerCalledAndAdmin() || !args.HasArgs(1)) return;

		CorePlugin.Reply($"{JsonConvert.SerializeObject(new DownloadResponse().WithFileType(DownloadResponse.FileTypes.Script).WithDataFile(args.Args[0]), Formatting.Indented)}", args);
	}

	[ConsoleCommand("drmreboot")]
	private void Reboot(ConsoleSystem.Arg args)
	{
		if (!args.IsPlayerCalledAndAdmin()) return;

		foreach (var processor in Config.DRMs)
		{
			processor.Uninitialize();
			processor.Initialize();
		}
	}

	public class Processor
	{
		public string Name { get; set; }
		public string ValidationEndpoint { get; set; }
		public string DownloadEndpoint { get; set; }
		public string PublicKey { get; set; }
		public List<Entry> Entries { get; set; } = new List<Entry>();

		[JsonIgnore]
		public bool IsOnline { get; internal set; }

		[JsonIgnore]
		public Loader.CarbonMod Mod { get; } = new Loader.CarbonMod();

		[JsonIgnore]
		public List<BaseProcessor.Instance> ProcessorInstances { get; } = new List<BaseProcessor.Instance>();

		#region Logging

		protected void Puts(object message)
			=> Logger.Log($"[{Name}] {message}");
		protected void PutsError(object message, Exception ex = null)
			=> Logger.Error($"[{Name}] {message}", ex);
		protected void PutsWarn(object message)
			=> Logger.Warn($"[{Name}] {message}");

		#endregion

		public WebRequests.WebRequest Enqueue(string url, string body, Action<int, string> callback, RequestMethod method = RequestMethod.GET, Dictionary<string, string> headers = null, float timeout = 0f, Action<int, string, Exception> onException = null)
		{
			return new WebRequests.WebRequest(url, callback, null)
			{
				Method = method.ToString(),
				RequestHeaders = headers,
				Timeout = timeout,
				Body = body,
				ErrorCallback = onException

			}.Start();
		}

		public void Validate()
		{
			if (string.IsNullOrEmpty(ValidationEndpoint))
			{
				PutsWarn("Not set up.");
				return;
			}

			Puts($"Validating...");

			Enqueue(string.Format(ValidationEndpoint, PublicKey), null, (code, data) =>
			{
				IsOnline = code == 200;

				if (IsOnline)
				{
					Puts($"Success!");

					Launch();
				}
				else PutsError($"Failed to validate.");
			}, onException: (code, data, exception) =>
			{
				PutsError($"Failed with '{code}' code.");
			});
		}

		public void Initialize()
		{
			Validate();

			Mod.Name = $"{Name} DRM";
			Loader._loadedMods.Add(Mod);
		}
		public void Uninitialize()
		{
			foreach (var entry in Entries)
			{
				DisposeEntry(entry);
			}

			ProcessorInstances.Clear();

			Loader._loadedMods.Remove(Mod);
		}

		public void Launch()
		{
			foreach (var entry in Entries)
			{
				RequestEntry(entry);
			}
		}

		public void RequestEntry(Entry entry)
		{
			DisposeEntry(entry);

			PutsWarn($"Loading '{entry.Id}' entry...");
			Enqueue(string.Format(DownloadEndpoint, PublicKey, entry.Id, entry.PrivateKey), null, (code, data) =>
			{
				Logger.Debug($"{entry.Id} DRM", $"Got response code '{code}' with {ByteEx.Format(data.Length).ToUpper()} of data");
				if (code != 200) return;

				try
				{
					var response = JsonConvert.DeserializeObject<DownloadResponse>(data);
					Logger.Debug($"{entry.Id} DRM", $"Deserialized response type '{response.FileType}'. Processing...");

					switch (response.FileType)
					{
						case DownloadResponse.FileTypes.Script:
							var instance = new ScriptInstance
							{
								File = entry.Id,
								_mod = Mod,
								_source = DecodeBase64(response.Data)
							};
							ProcessorInstances.Add(instance);
							instance.Execute();
							break;

						case DownloadResponse.FileTypes.DLL:
							var source = Convert.FromBase64String(response.Data);
							var assembly = Assembly.Load(source);

							foreach (var type in assembly.GetTypes())
							{
								Loader.InitializePlugin(type, out var plugin, Mod);
							}
							break;
					}
				}
				catch (Exception ex)
				{
					PutsError($"Failed loading '{entry.Id}'", ex);
				}
			});
		}
		public void DisposeEntry(Entry entry)
		{
			var alreadyProcessedInstance = ProcessorInstances.FirstOrDefault(x => x.File == entry.Id);

			if (alreadyProcessedInstance != null)
			{
				try { alreadyProcessedInstance.Dispose(); } catch { }
				ProcessorInstances.Remove(alreadyProcessedInstance);
				PutsWarn($"Unloading '{entry.Id}' entry");
			}
		}

		public static string EncodeBase64(string value)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
		}
		public static string DecodeBase64(string value)
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(value));
		}

		public class ScriptInstance : ScriptProcessor.Script
		{
			internal Loader.CarbonMod _mod;
			internal string _source;

			public override void Dispose()
			{
				foreach (var plugin in _loader.Scripts)
				{
					plugin.Dispose();
					_mod.Plugins.Remove(plugin.Instance);
				}

				base.Dispose();
			}
			public override void Execute()
			{
				try
				{
					_loader = new ScriptLoader();
					_loader.Parser = Parser;
					_loader.File = File;
					_loader.Source = _source;
					_loader.Mod = _mod;
					_loader.Instance = this;
					_loader.Load();
				}
				catch (Exception ex)
				{
					Carbon.Logger.Warn($"Failed processing {File}:\n{ex}");
				}
			}
		}
	}
	public class Entry
	{
		public string Id { get; set; }
		public string PrivateKey { get; set; }
	}
	public class DownloadResponse
	{
		public FileTypes FileType { get; set; }
		public string Data { get; set; }

		public DownloadResponse WithFileType(FileTypes type)
		{
			FileType = type;
			return this;
		}
		public DownloadResponse WithData(string source)
		{
			Data = Processor.EncodeBase64(source);
			return this;
		}
		public DownloadResponse WithDataFile(string file)
		{
			return WithData(OsEx.File.ReadText(file));
		}

		public enum FileTypes
		{
			Script = 512,
			DLL = 1024
		}
	}
}

public class DRMConfig
{
	public List<DRMModule.Processor> DRMs { get; set; } = new List<DRMModule.Processor>();
}
public class DRMData
{

}
