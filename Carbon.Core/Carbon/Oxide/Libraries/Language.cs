﻿///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 

using System.Collections.Generic;
using Oxide.Plugins;

namespace Oxide.Core.Libraries
{
	public class Language
	{
		public void RegisterMessages(Dictionary<string, string> messages, RustPlugin plugin, string lang = "en")
		{

		}

		public string GetMessage(string name, RustPlugin plugin, string player = null)
		{
			return name;
		}
	}
}
