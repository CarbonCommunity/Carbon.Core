using System;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

[Flags]
public enum HookFlags { None = 0, Static = 1, Hidden = 2, IgnoreChecksum = 4 }

public enum HookState { Inactive, Warning, Failure, Success }
