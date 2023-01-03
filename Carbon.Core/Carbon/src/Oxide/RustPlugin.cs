using System;
using System.IO;
using Carbon.Core;
using Carbon.Extensions;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries;
using Oxide.Game.Rust.Libraries;
using UnityEngine;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Plugins;

public class RustPlugin : Plugin
{
	public PluginManager Manager { get; set; }

	public Permission permission { get; set; }
	public Language lang { get; set; }
	public Command cmd { get; set; }
	public Server server { get; set; }
	public Plugins plugins { get; set; }
	public Timers timer { get; set; }
	public OxideMod mod { get; set; }
	public WebRequests webrequest { get; set; }
	public Oxide.Game.Rust.Libraries.Rust rust { get; set; }
	public Persistence persistence { get; set; }
	public CovalencePlugin.Covalence covalence { get; set; }

	public DynamicConfigFile Config { get; private set; }

	public Player Player { get { return rust.Player; } private set { } }
	public Server Server { get { return rust.Server; } private set { } }

	public RustPlugin()
	{
		Setup($"Core Plugin {RandomEx.GetRandomString(5)}", "Carbon Community", new VersionNumber(1, 0, 0), string.Empty);
	}

	public void SetupMod(Loader.CarbonMod mod, string name, string author, VersionNumber version, string description)
	{
		_carbon = mod;
		Setup(name, author, version, description);
	}
	public void Setup(string name, string author, VersionNumber version, string description)
	{
		Name = name;
		Version = version;
		Author = author;
		Description = description;

		permission = Interface.Oxide.Permission;
		cmd = new Command();
		server = new Server();
		Manager = new PluginManager();
		plugins = new Plugins();
		timer = new Timers(this);
		lang = new Language(this);
		mod = new OxideMod();
		rust = new Game.Rust.Libraries.Rust();
		webrequest = new WebRequests();
		persistence = new GameObject($"Script_{name}").AddComponent<Persistence>();
		UnityEngine.Object.DontDestroyOnLoad(persistence.gameObject);
		covalence = new CovalencePlugin.Covalence();

		Type = GetType();

		mod.Load();
	}
	public override void Dispose()
	{
		permission.UnregisterPermissions(this);

		timer.Clear();

		if (persistence != null)
		{
			var go = persistence.gameObject;
			UnityEngine.Object.DestroyImmediate(persistence);
			UnityEngine.Object.Destroy(go);
		}

		base.Dispose();
	}

	/// <summary>
	/// Outputs to the game's console a message with severity level 'NOTICE'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	public void Puts(object message)
		=> Carbon.Logger.Log($"[{Name}] {message}");

	/// <summary>
	/// Outputs to the game's console a message with severity level 'NOTICE'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="args"></param>
	public void Puts(string message, params object[] args)
		=> Carbon.Logger.Log($"[{Name}] {(args == null ? message : string.Format(message, args))}");

	/// <summary>
	/// Outputs to the game's console a message with severity level 'NOTICE'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	public void Log(string message)
		=> Carbon.Logger.Log($"[{Name}] {message}");

	/// <summary>
	/// Outputs to the game's console a message with severity level 'WARNING'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	public void LogWarning(string message)
		=> Carbon.Logger.Warn($"[{Name}] {message}");

	/// <summary>
	/// Outputs to the game's console a message with severity level 'ERROR'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="ex"></param>
	public void LogError(string message, Exception ex)
		=> Carbon.Logger.Error($"[{Name}] {message}", ex);

	/// <summary>
	/// Outputs to the game's console a message with severity level 'ERROR'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	public void LogError(string message)
		=> Carbon.Logger.Error($"[{Name}] {message}", null);

	/// <summary>
	/// Outputs to the game's console a message with severity level 'WARNING'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="args"></param>
	public void PrintWarning(string format, params object[] args)
		=> Carbon.Logger.Warn($"[{Name}] {(args == null ? format : string.Format(format, args))}");

	/// <summary>
	/// Outputs to the game's console a message with severity level 'ERROR'.
	/// NOTE: Oxide compatibility layer.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="args"></param>
	public void PrintError(string format, params object[] args)
		=> Carbon.Logger.Error($"[{Name}] {(args == null ? format : string.Format(format, args))}");

	protected void LogToFile(string filename, string text, Plugin plugin, bool timeStamp = true)
	{
		var logFolder = Path.Combine(Defines.GetLogsFolder(), plugin.Name);

		if (!Directory.Exists(logFolder))
		{
			Directory.CreateDirectory(logFolder);
		}

		filename = plugin.Name.ToLower() + "_" + filename.ToLower() + (timeStamp ? $"-{DateTime.Now:yyyy-MM-dd}" : "") + ".txt";

		File.AppendAllText(Path.Combine(logFolder, Utility.CleanPath(filename)), (timeStamp ? $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}" : text) + Environment.NewLine);
	}

	public void ILoadConfig()
	{
		LoadConfig();
	}
	public void ILoadDefaultMessages()
	{
		LoadDefaultMessages();
	}

	protected virtual void LoadConfig()
	{
		Config = new DynamicConfigFile(Path.Combine(Manager.ConfigPath, Name + ".json"));

		if (!Config.Exists(null))
		{
			LoadDefaultConfig();
			SaveConfig();
		}
		try
		{
			Config.Load(null);
		}
		catch (Exception ex)
		{
			Carbon.Logger.Error("Failed to load config file (is the config file corrupt?) (" + ex.Message + ")");
		}
	}
	protected virtual void LoadDefaultConfig()
	{
		//CallHook ( "LoadDefaultConfig" );
	}
	protected virtual void SaveConfig()
	{
		if (Config == null)
		{
			return;
		}
		try
		{
			Config.Save(null);
		}
		catch (Exception ex)
		{
			Carbon.Logger.Error("Failed to save config file (does the config have illegal objects in it?) (" + ex.Message + ")", ex);
		}
	}

	protected virtual void LoadDefaultMessages()
	{

	}

	public new string ToString()
	{
		return $"{Name} v{Version} by {Author}";
	}

	protected void PrintToConsole(BasePlayer player, string format, params object[] args)
	{
		if (((player != null) ? player.net : null) != null)
		{
			player.SendConsoleCommand("echo " + ((args.Length != 0) ? string.Format(format, args) : format));
		}
	}
	protected void PrintToConsole(string format, params object[] args)
	{
		if (BasePlayer.activePlayerList.Count >= 1)
		{
			ConsoleNetwork.BroadcastToAllClients("echo " + ((args.Length != 0) ? string.Format(format, args) : format));
		}
	}
	protected void PrintToChat(BasePlayer player, string format, params object[] args)
	{
		if (((player != null) ? player.net : null) != null)
		{
			player.SendConsoleCommand("chat.add", 2, 0, (args.Length != 0) ? string.Format(format, args) : format);
		}
	}
	protected void PrintToChat(string format, params object[] args)
	{
		if (BasePlayer.activePlayerList.Count >= 1)
		{
			ConsoleNetwork.BroadcastToAllClients("chat.add", 2, 0, (args.Length != 0) ? string.Format(format, args) : format);
		}
	}

	protected void SendReply(ConsoleSystem.Arg arg, string format, params object[] args)
	{
		var connection = arg.Connection;
		var basePlayer = connection?.player as BasePlayer;
		var text = (args != null && args.Length != 0) ? string.Format(format, args) : format;

		if (((basePlayer != null) ? basePlayer.net : null) != null)
		{
			basePlayer.SendConsoleCommand($"echo {text}");
			return;
		}

		Puts(text, null);
	}
	protected void SendReply(BasePlayer player, string format, params object[] args)
	{
		PrintToChat(player, format, args);
	}
	protected void SendWarning(ConsoleSystem.Arg arg, string format, params object[] args)
	{
		var connection = arg.Connection;
		var basePlayer = connection?.player as BasePlayer;
		var text = (args != null && args.Length != 0) ? string.Format(format, args) : format;

		if (((basePlayer != null) ? basePlayer.net : null) != null)
		{
			basePlayer.SendConsoleCommand($"echo {text}");
			return;
		}

		Debug.LogWarning(text);
	}
	protected void SendError(ConsoleSystem.Arg arg, string format, params object[] args)
	{
		var connection = arg.Connection;
		var basePlayer = connection?.player as BasePlayer;
		var text = (args != null && args.Length != 0) ? string.Format(format, args) : format;

		if (((basePlayer != null) ? basePlayer.net : null) != null)
		{
			basePlayer.SendConsoleCommand($"echo {text}");
			return;
		}

		Debug.LogError(text);
	}

	protected void ForcePlayerPosition(BasePlayer player, Vector3 destination)
	{
		player.MovePosition(destination);

		if (!player.IsSpectating() || (double)Vector3.Distance(player.transform.position, destination) > 25.0)
		{
			player.ClientRPCPlayer(null, player, "ForcePositionTo", destination);
			return;
		}

		player.SendNetworkUpdate(BasePlayer.NetworkQueue.UpdateDistance);
	}

	#region Covalence

	protected void AddCovalenceCommand(string command, string callback, string[] perms = null)
	{
		cmd.AddCovalenceCommand(command, this, callback, permissions: perms);

		if (perms != null)
		{
			foreach (var permission in perms)
			{
				if (!this.permission.PermissionExists(permission))
				{
					this.permission.RegisterPermission(permission, this);
				}
			}
		}
	}
	protected void AddUniversalCommand(string command, string callback, string[] perms = null)
	{
		cmd.AddCovalenceCommand(command, this, callback, permissions: perms);

		if (perms != null)
		{
			foreach (var permission in perms)
			{
				if (!this.permission.PermissionExists(permission))
				{
					this.permission.RegisterPermission(permission, this);
				}
			}
		}
	}

	#endregion

	public class Persistence : FacepunchBehaviour { }
}
