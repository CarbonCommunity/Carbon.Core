using System;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

[AttributeUsage(System.AttributeTargets.Class)]
public class HookAttribute : Attribute
{
	[AttributeUsage(AttributeTargets.Class)]
	public class Patch : Attribute
	{
		public string Name
		{ get; }

		public Type Target
		{ get; }

		public string Method
		{ get; }

		public Type[] MethodArgs
		{ get; }

		public Patch(string name, Type target, string method)
		{
			Name = name;
			Target = target;
			Method = method;
		}

		public Patch(string name, Type target, string method, Type[] args) : this(name, target, method)
			=> MethodArgs = args;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class Options : Attribute
	{
		public HookFlags Value
		{ get; }

		public Options(HookFlags value)
			=> Value = value;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class Identifier : Attribute
	{
		public string Value
		{ get; }

		public Identifier(string value)
			=> Value = value;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class Dependencies : Attribute
	{
		public string[] Value
		{ get; }

		public Dependencies(string[] value)
			=> Value = value;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class Checksum : Attribute
	{
		public string Value
		{ get; }

		public Checksum(string value)
			=> Value = value;
	}
}
