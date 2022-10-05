﻿///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 

using System;
using Carbon.Core.Extensions;
using Harmony;

namespace Carbon.Core.Oxide.Hooks
{
	[HarmonyPatch(typeof(ServerMgr), "UpdateServerInformation")]
	public class ServerMgr_UpdateServerInformation
	{
		public static void Postfix()
		{
			if (CarbonCore.Instance == null || CarbonCore.Instance.Config == null) return;

			try
			{
				if (CarbonCore.Instance.Config.CarbonTag)
				{
					ServerTagEx.SetRequiredTag("carbon");
				}
				else
				{
					ServerTagEx.UnsetRequiredTag("carbon");
				}

				if (CarbonCore.Instance.Config.IsModded)
				{
					ServerTagEx.SetRequiredTag("modded");
				}
				else
				{
					ServerTagEx.UnsetRequiredTag("modded");
				}
			}
			catch (Exception ex)
			{
				Carbon.Logger.Error($"Couldn't patch UpdateServerInformation.", ex);
			}
		}
	}
}
