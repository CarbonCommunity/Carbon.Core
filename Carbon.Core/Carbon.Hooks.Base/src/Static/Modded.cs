using System;
using Carbon.Extensions;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Static
{
	public partial class Static_ServerMgr
	{
		[HookAttribute.Patch("IServerInfoUpdate", typeof(ServerMgr), "UpdateServerInformation", new System.Type[] { })]
		[HookAttribute.Identifier("aaa38191cc9f4f6f911df9742d552a99")]
		[HookAttribute.Options(HookFlags.Static | HookFlags.Hidden | HookFlags.IgnoreChecksum)]

		public class Static_ServerMgr_UpdateServerInformation_aaa38191cc9f4f6f911df9742d552a99
		{
			public static void Postfix()
			{
				if (Community.Runtime == null || Community.Runtime.Config == null) return;

				try
				{
					if (Community.Runtime.Config.CarbonTag)
					{
						ServerTagEx.SetRequiredTag("carbon");
					}
					else
					{
						ServerTagEx.UnsetRequiredTag("carbon");
					}

					if (Community.Runtime.Config.IsModded)
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
}
