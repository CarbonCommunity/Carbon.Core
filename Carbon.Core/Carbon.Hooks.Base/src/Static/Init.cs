/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Hooks;

public partial class Category_Static
{
	public partial class Static_Bootstrap
	{
		[HookAttribute.Patch("IInit", typeof(Bootstrap), "StartupShared", new System.Type[] { })]
		[HookAttribute.Identifier("e3acad59160c4885bf724620db14c7e3")]
		[HookAttribute.Options(HookFlags.Static | HookFlags.Hidden | HookFlags.IgnoreChecksum)]

		public class Static_Bootstrap_StartupShared_e3acad59160c4885bf724620db14c7e3
		{
			public static void Prefix()
			{
				Community.Runtime.Initialize();
			}
		}
	}
}
