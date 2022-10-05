﻿///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 

using System.Collections.Generic;
using System.Reflection;

namespace Carbon.Core.Modules
{
	public interface IHookableModule
	{
		Dictionary<string, List<MethodInfo>> HookCache { get; }
	}
}
