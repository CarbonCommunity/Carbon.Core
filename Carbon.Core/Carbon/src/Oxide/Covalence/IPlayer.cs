using System;
using System.Globalization;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core.Libraries.Covalence;

public interface IPlayer
{
	object Object { get; }
	CommandType LastCommand { get; set; }
	string Name { get; set; }
	string Id { get; }
	string Address { get; }
	int Ping { get; }
	CultureInfo Language { get; }
	bool IsConnected { get; }
	bool IsSleeping { get; }
	bool IsServer { get; }
	bool IsAdmin { get; }
	bool IsBanned { get; }
	void Ban(string reason, TimeSpan duration = default(TimeSpan));
	TimeSpan BanTimeRemaining { get; }
	void Heal(float amount);
	float Health { get; set; }
	void Hurt(float amount);
	void Kick(string reason);
	void Kill();
	float MaxHealth { get; set; }
	void Rename(string name);
	void Teleport(float x, float y, float z);
	void Unban();
	void Position(out float x, out float y, out float z);
	void Message(string message, string prefix, params object[] args);
	void Message(string message);
	void Reply(string message, string prefix, params object[] args);
	void Reply(string message);
	void Command(string command, params object[] args);
	bool HasPermission(string perm);
	void GrantPermission(string perm);
	void RevokePermission(string perm);
	bool BelongsToGroup(string group);
	void AddToGroup(string group);
	void RemoveFromGroup(string group);
}

public struct RustPlayer : IPlayer
{
	public object Object { get; set; }

	public BasePlayer BasePlayer => Object as BasePlayer;

	public CommandType LastCommand { get; set; }
	public string Name { get; set; }

	public string Id => BasePlayer?.UserIDString;

	public string Address => BasePlayer?.Connection?.ipaddress;

	public int Ping => BasePlayer == null ? 0 : Network.Net.sv.GetAveragePing(BasePlayer.Connection);

	public CultureInfo Language => CultureInfo.GetCultureInfo(BasePlayer.net.connection.info.GetString("global.language", "") ?? "en");

	public bool IsConnected => BasePlayer.IsConnected;

	public bool IsSleeping => BasePlayer.IsSleeping();

	public bool IsServer => true;

	public bool IsAdmin => !ulong.TryParse(Id, out var id) ? false : ServerUsers.Is(id, ServerUsers.UserGroup.Owner);

	public bool IsBanned => !ulong.TryParse(Id, out var id) ? false : ServerUsers.Is(id, ServerUsers.UserGroup.Banned);

	public TimeSpan BanTimeRemaining
	{
		get
		{
			if (!IsBanned)
			{
				return TimeSpan.Zero;
			}

			return TimeSpan.MaxValue;
		}
	}

	public float Health
	{
		get
		{
			return BasePlayer.health;
		}
		set
		{
			BasePlayer.health = value;
		}
	}
	public float MaxHealth
	{
		get
		{
			return BasePlayer.MaxHealth();
		}
		set
		{
			BasePlayer._maxHealth = value;
		}
	}

	public void AddToGroup(string group)
	{

	}

	public void Ban(string reason, TimeSpan duration = default)
	{

	}

	public bool BelongsToGroup(string group)
	{
		return false;
	}

	public void Command(string command, params object[] args)
	{

	}

	public void GrantPermission(string perm)
	{

	}

	public bool HasPermission(string perm)
	{
		return false;
	}

	public void Heal(float amount)
	{

	}

	public void Hurt(float amount)
	{

	}

	public void Kick(string reason)
	{

	}

	public void Kill()
	{

	}

	public void Message(string message, string prefix, params object[] args)
	{

	}

	public void Message(string message)
	{

	}

	public void Position(out float x, out float y, out float z)
	{
		x = y = z = 0;
	}

	public void RemoveFromGroup(string group)
	{

	}

	public void Rename(string name)
	{

	}

	public void Reply(string message, string prefix, params object[] args)
	{

	}

	public void Reply(string message)
	{

	}

	public void RevokePermission(string perm)
	{

	}

	public void Teleport(float x, float y, float z)
	{

	}

	public void Unban()
	{

	}
}
