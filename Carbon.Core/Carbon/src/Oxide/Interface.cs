using System;
using Carbon;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core
{
	public class Interface
	{
		public static OxideMod Oxide { get; set; } = new OxideMod();
		public static OxideMod uMod => Oxide;

		public static void Initialize()
		{
			Oxide.Load();
			Carbon.Logger.Log($"  Instance Directory: {Oxide.InstanceDirectory}");
			Carbon.Logger.Log($"  Root Directory: {Oxide.RootDirectory}");
			Carbon.Logger.Log($"  Config Directory: {Oxide.ConfigDirectory}");
			Carbon.Logger.Log($"  Data Directory: {Oxide.DataDirectory}");
			Carbon.Logger.Log($"  Lang Directory: {Oxide.LangDirectory}");
			Carbon.Logger.Log($"  Log Directory: {Oxide.LogDirectory}");
			Carbon.Logger.Log($"  Plugin Directory: {Oxide.PluginDirectory}");
		}

		public static OxideMod GetMod() => Oxide;

		public static T Call<T>(string hookName)
		{
			return (T)HookCaller.CallStaticHook(hookName);
		}
		public static T Call<T>(string hookName, object arg1)
		{
			return (T)HookCaller.CallStaticHook(hookName, arg1);
		}
		public static T Call<T>(string hookName, object arg1, object arg2)
		{
			return (T)HookCaller.CallStaticHook(hookName, arg1, arg2);
		}
		public static T Call<T>(string hookName, object arg1, object arg2, object arg3)
		{
			return (T)HookCaller.CallStaticHook(hookName, arg1, arg2, arg3);
		}
		public static T Call<T>(string hookName, object arg1, object arg2, object arg3, object arg4)
		{
			return (T)HookCaller.CallStaticHook(hookName, arg1, arg2, arg3, arg4);
		}
		public static T Call<T>(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5)
		{
			return (T)HookCaller.CallStaticHook(hookName, arg1, arg2, arg3, arg4, arg5);
		}
		public static T Call<T>(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
		{
			return (T)HookCaller.CallStaticHook(hookName, arg1, arg2, arg3, arg4, arg5, arg6);
		}
		public static T Call<T>(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
		{
			return (T)HookCaller.CallStaticHook(hookName, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}
		public static T Call<T>(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
		{
			return (T)HookCaller.CallStaticHook(hookName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}
		public static T Call<T>(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
		{
			return (T)HookCaller.CallStaticHook(hookName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
		}

		public static object CallHook(string hookName)
		{
			return HookCaller.CallStaticHook(hookName);
		}
		public static object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate)
		{
			return HookCaller.CallStaticDeprecatedHook(oldHook, newHook, expireDate);
		}
		public static object CallHook(string hookName, object arg1)
		{
			return HookCaller.CallStaticHook(hookName, arg1);
		}
		public static object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1)
		{
			return HookCaller.CallStaticDeprecatedHook(oldHook, newHook, expireDate, arg1);
		}
		public static object CallHook(string hookName, object arg1, object arg2)
		{
			return HookCaller.CallStaticHook(hookName, arg1, arg2);
		}
		public static object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2)
		{
			return HookCaller.CallStaticDeprecatedHook(oldHook, newHook, expireDate, arg1, arg2);
		}
		public static object CallHook(string hookName, object arg1, object arg2, object arg3)
		{
			return HookCaller.CallStaticHook(hookName, arg1, arg2, arg3);
		}
		public static object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3)
		{
			return HookCaller.CallStaticDeprecatedHook(oldHook, newHook, expireDate, arg1, arg2, arg3);
		}
		public static object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4)
		{
			return HookCaller.CallStaticHook(hookName, arg1, arg2, arg3, arg4);
		}
		public static object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4)
		{
			return HookCaller.CallStaticDeprecatedHook(oldHook, newHook, expireDate, arg1, arg2, arg3, arg4);
		}
		public static object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5)
		{
			return HookCaller.CallStaticHook(hookName, arg1, arg2, arg3, arg4, arg5);
		}
		public static object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5)
		{
			return HookCaller.CallStaticDeprecatedHook(oldHook, newHook, expireDate, arg1, arg2, arg3, arg4, arg5);
		}
		public static object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
		{
			return HookCaller.CallStaticHook(hookName, arg1, arg2, arg3, arg4, arg5, arg6);
		}
		public static object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
		{
			return HookCaller.CallStaticDeprecatedHook(oldHook, newHook, expireDate, arg1, arg2, arg3, arg4, arg5, arg6);
		}
		public static object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
		{
			return HookCaller.CallStaticHook(hookName, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}
		public static object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
		{
			return HookCaller.CallStaticDeprecatedHook(oldHook, newHook, expireDate, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
		}
		public static object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
		{
			return HookCaller.CallStaticHook(hookName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}
		public static object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
		{
			return HookCaller.CallStaticDeprecatedHook(oldHook, newHook, expireDate, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
		}
		public static object CallHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
		{
			return HookCaller.CallStaticHook(hookName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
		}
		public static object CallDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
		{
			return HookCaller.CallStaticDeprecatedHook(oldHook, newHook, expireDate, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
		}
		public static object Call(string hook, params object[] args)
		{
			return HookCaller.CallStaticHook(hook, args);
		}
	}
}
