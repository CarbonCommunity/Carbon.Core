
using System;
using System.Reflection;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

internal struct HookRuntime
{
	public Exception LastError;
	//public List<string> subscribers;
	public HookState Status;

	// Harmony
	public HarmonyLib.Harmony HarmonyHandler;
	public MethodInfo Prefix, Postfix, Transpiler;
}

internal struct Subscription
{
	public string HookName, Subscriber;

	public Subscription(string name, string sub)
	{ HookName = name; Subscriber = sub; }
}

internal readonly struct Payload
{
	// A hook may need more than one patch applied, hookName is mandatory,
	// identifier is optional and should only be sent when a specific patch
	// needs to be applied.
	public readonly string HookName, Identifier, Requester;

	// Payload does not cary the action (install/uninstall) as this is will
	// be automatically sorted out by the Update() method based on the subscribers.
	public Payload(string name, string id, string req)
	{ HookName = name; Identifier = id; Requester = req; }
}
