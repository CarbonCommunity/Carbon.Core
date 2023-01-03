using System;
using System.Linq;
using System.Reflection;
using Carbon.Extensions;
using Oxide.Core;
using UnityEngine;
using static ServerUsers;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

[AttributeUsage(AttributeTargets.Class)]
public class InfoAttribute : Attribute
{
	public string Title { get; }
	public string Author { get; }
	public VersionNumber Version { get; private set; }
	public int ResourceId { get; set; }

	public InfoAttribute(string Title, string Author, string Version)
	{
		this.Title = Title;
		this.Author = Author;
		SetVersion(Version);
	}

	private void SetVersion(string version)
	{
		ushort result;
		var list = (from part in version.Split('.')
					select (ushort)(ushort.TryParse(part, out result) ? result : 0)).ToList();
		while (list.Count < 3)
		{
			list.Add(0);
		}

		if (list.Count > 3)
		{
			Debug.LogWarning("Version `" + version + "` is invalid for " + Title + ", should be `major.minor.patch`");
		}

		Version = new VersionNumber(list[0], list[1], list[2]);
	}
}

[AttributeUsage(AttributeTargets.Class)]
public class DescriptionAttribute : Attribute
{
	public string Description { get; }

	public DescriptionAttribute(string description)
	{
		Description = description;
	}
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field)]
public class PluginReferenceAttribute : Attribute
{
	public string Name { get; set; }

	public FieldInfo Field { get; set; }

	public PluginReferenceAttribute(string name = null)
	{
		Name = name;
	}
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CommandAttribute : Attribute
{
	public string[] Names { get; } = new string[1];
	public string Help { get; }

	public CommandAttribute(string name, string help = null)
	{
		Names[0] = name;
		Help = help;
	}

	public CommandAttribute(params string[] commands)
	{
		Names = commands;
	}
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ChatCommandAttribute : Attribute
{
	public string Name { get; }
	public string Help { get; }

	public ChatCommandAttribute(string name, string help = null)
	{
		Name = name;
		Help = help;
	}
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ConsoleCommandAttribute : Attribute
{
	public string Name { get; }
	public string Help { get; }

	public ConsoleCommandAttribute(string name)
	{
		Name = name;
	}

	public ConsoleCommandAttribute(string name, string help)
	{
		Name = name;
		Help = help;
	}
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class UiCommandAttribute : Attribute
{
	public string Name { get; }
	public string Help { get; }

	public static string Uniquify(string name)
	{
		var id = RelationshipManager.ServerInstance.net.ID;
		return RandomEx.GetRandomString(16, name + id.ToString(), name.Length + (int)id);
	}

	public UiCommandAttribute(string name)
	{
		Name = name;
	}

	public UiCommandAttribute(string name, string help)
	{
		Name = name;
		Help = help;
	}
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class CommandVarAttribute : Attribute
{
	public string Name { get; }
	public string Help { get; }
	public bool AdminOnly { get; set; }

	public CommandVarAttribute(string name, bool adminOnly = false)
	{
		Name = name;
		AdminOnly = adminOnly;
	}
	public CommandVarAttribute(string name, string help, bool adminOnly = false)
	{
		Name = name;
		Help = help;
		AdminOnly = adminOnly;
	}
}

[AttributeUsage(AttributeTargets.Method)]
public class HookMethodAttribute : Attribute
{
	public string Name { get; set; }
	public MethodInfo Method { get; set; }

	public HookMethodAttribute() { }
	public HookMethodAttribute(string name)
	{
		Name = name;
	}
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PermissionAttribute : Attribute
{
	public string Name { get; }

	public PermissionAttribute(string permission)
	{
		Name = permission;
	}
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class GroupAttribute : Attribute
{
	public string Name { get; }

	public GroupAttribute(string group)
	{
		Name = group;
	}
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class AuthLevelAttribute : Attribute
{
	public UserGroup Group { get; } = UserGroup.None;

	public AuthLevelAttribute(UserGroup group)
	{
		Group = group;
	}

	public AuthLevelAttribute(int group)
	{
		Group = (UserGroup)group;
	}
}
