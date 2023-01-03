using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Carbon.Base;
using Carbon.Core;
using Facepunch;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon
{
	public static class HookCaller
	{
		internal static Dictionary<int, object[]> _argumentBuffer { get; } = new Dictionary<int, object[]>();
		internal static Dictionary<string, int> _hookTimeBuffer { get; } = new Dictionary<string, int>();
		internal static Dictionary<string, int> _hookTotalTimeBuffer { get; } = new Dictionary<string, int>();
		internal static Dictionary<string, DateTime> _lastDeprecatedWarningAt { get; } = new Dictionary<string, DateTime>();

		internal static void _appendHookTime(string hook, int time)
		{
			if (!Community.Runtime.Config.HookTimeTracker) return;

			if (!_hookTimeBuffer.TryGetValue(hook, out var total))
			{
				_hookTimeBuffer.Add(hook, time);
			}
			else _hookTimeBuffer[hook] = total + time;

			if (!_hookTotalTimeBuffer.TryGetValue(hook, out total))
			{
				_hookTotalTimeBuffer.Add(hook, time);
			}
			else _hookTotalTimeBuffer[hook] = total + time;

		}
		internal static void _clearHookTime(string hook)
		{
			if (!Community.Runtime.Config.HookTimeTracker) return;

			if (!_hookTimeBuffer.ContainsKey(hook))
			{
				_hookTimeBuffer.Add(hook, 0);
			}
			else
			{
				_hookTimeBuffer[hook] = 0;
			}
		}

		public static int GetHookTime(string hook)
		{
			if (!_hookTimeBuffer.TryGetValue(hook, out var total))
			{
				return 0;
			}

			return total;
		}
		public static int GetHookTotalTime(string hook)
		{
			if (!_hookTotalTimeBuffer.TryGetValue(hook, out var total))
			{
				return 0;
			}

			return total;
		}

		internal static object[] _allocateBuffer(int count)
		{
			if (!_argumentBuffer.TryGetValue(count, out var buffer))
			{
				_argumentBuffer.Add(count, buffer = new object[count]);
			}

			return buffer;
		}
		internal static object[] _rescaleBuffer(object[] oldBuffer, int newScale)
		{
			if (oldBuffer.Length == newScale)
			{
				return oldBuffer;
			}

			var newBuffer = _allocateBuffer(newScale);

			for (int i = 0; i < newScale; i++)
			{
				if (i > oldBuffer.Length - 1) break;

				newBuffer[i] = oldBuffer[i];
			}

			return newBuffer;
		}
		internal static void _clearBuffer(object[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = null;
			}
		}

		private static object CallHook<T>(this T plugin, string hookName, BindingFlags flags, object[] args) where T : BaseHookable
		{
			if (plugin.IsHookIgnored(hookName)) return null;

			var id = $"{hookName}[{(args == null ? 0 : args.Length)}]";
			var result = (object)null;

			if (plugin.HookMethodAttributeCache.TryGetValue(id, out var hooks))
			{
				foreach (var method in hooks)
				{
					var methodResult = DoCall(method);
					if (methodResult != null) result = methodResult;
				}
				return result;
			}

			if (!plugin.HookCache.TryGetValue(id, out hooks))
			{
				plugin.HookCache.Add(id, hooks = new List<MethodInfo>());

				foreach (var method in plugin.Type.GetMethods(flags))
				{
					if (method.Name != hookName) continue;

					hooks.Add(method);
				}
			}

			foreach (var method in hooks)
			{
				try
				{
					var methodResult = DoCall(method);
					if (methodResult != null) result = methodResult;
				}
				catch (ArgumentException) { }
				catch (TargetParameterCountException) { }
				catch (Exception ex)
				{
					var exception = ex.InnerException ?? ex;
					Carbon.Logger.Error(
						$"Failed to call hook '{hookName}' on plugin '{plugin.Name} v{plugin.Version}'",
						exception
					);
				}
			}

			object DoCall(MethodInfo method)
			{
				if (method == null) return null;

				if (args != null)
				{
					var actualLength = method.GetParameters().Length;
					if (actualLength != args.Length)
					{
						args = _rescaleBuffer(args, actualLength);
					}
				}

				var beforeTicks = Environment.TickCount;
				plugin.TrackStart();
				result = method?.Invoke(plugin, args);
				plugin.TrackEnd();
				var afterTicks = Environment.TickCount;
				var totalTicks = afterTicks - beforeTicks;

				_appendHookTime(hookName, totalTicks);

				if (afterTicks > beforeTicks + 100 && afterTicks > beforeTicks)
				{
					Carbon.Logger.Warn($" {plugin?.Name} hook took longer than 100ms {hookName} [{totalTicks:0}ms]");
				}

				return result;
			}

			return result;
		}
		private static object CallDeprecatedHook<T>(this T plugin, string oldHook, string newHook, DateTime expireDate, BindingFlags flags, object[] args) where T : BaseHookable
		{
			if (expireDate < DateTime.Now)
			{
				return null;
			}

			DateTime now = DateTime.Now;

			if (!_lastDeprecatedWarningAt.TryGetValue(oldHook, out DateTime lastWarningAt) || (now - lastWarningAt).TotalSeconds > 3600f)
			{
				_lastDeprecatedWarningAt[oldHook] = now;

				Carbon.Logger.Warn($"'{plugin.Name} v{plugin.Version}' is using deprecated hook '{oldHook}', which will stop working on {expireDate.ToString("D")}. Please ask the author to update to '{newHook}'");
			}

			return CallDeprecatedHook(plugin, oldHook, newHook, expireDate, flags, args);
		}

		private static object CallStaticHook(string hookName, BindingFlags flag = BindingFlags.NonPublic | BindingFlags.Static, object[] args = null)
		{
			var objectOverride = (object)null;
			var hookableOverride = (BaseHookable)null;

			_clearHookTime(hookName);

			foreach (var module in Community.Runtime.ModuleProcessor.Modules)
			{
				var result = module.CallHook(hookName, flags: flag, args: args);
				if (result != null && objectOverride != null)
				{
					Carbon.Logger.Warn($"Hook '{hookName}' conflicts with {hookableOverride.Name}");
					break;
				}

				if (result != null)
				{
					objectOverride = result;
					hookableOverride = module;
				}
			}

			var array = args == null || args.Length == 0 ? null : args.ToArray();

			foreach (var mod in Loader._loadedMods)
			{
				foreach (var plugin in mod.Plugins)
				{
					try
					{
						var result = plugin.CallHook(hookName, flags: flag, args: array);
						if (result != null && objectOverride != null)
						{
							Carbon.Logger.Warn($"Hook '{hookName}' conflicts with {hookableOverride.Name}");
							break;
						}

						if (result != null)
						{
							objectOverride = result;
							hookableOverride = plugin;
						}
					}
					catch (Exception ex) { Logger.Error("Fuck:", ex); }
				}
			}

			if (array != null) Pool.Free(ref array);

			return objectOverride;
		}
		private static object CallStaticDeprecatedHook(string oldHook, string newHook, DateTime expireDate, BindingFlags flag = BindingFlags.NonPublic | BindingFlags.Static, object[] args = null)
		{
			if (expireDate < DateTime.Now)
			{
				return null;
			}

			DateTime now = DateTime.Now;

			if (!_lastDeprecatedWarningAt.TryGetValue(oldHook, out DateTime lastWarningAt) || (now - lastWarningAt).TotalSeconds > 3600f)
			{
				_lastDeprecatedWarningAt[oldHook] = now;

				Carbon.Logger.Warn($"A plugin is using deprecated hook '{oldHook}', which will stop working on {expireDate.ToString("D")}. Please ask the author to update to '{newHook}'");
			}

			return CallStaticHook(oldHook, flag, args);
		}

		public static object CallHook(BaseHookable plugin, string hookName)
		{
			return CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null);
		}
		public static T CallHook<T>(BaseHookable plugin, string hookName)
		{
			return (T)CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null);
		}
		public static T CallDeprecatedHook<T>(BaseHookable plugin, string oldHook, string newHook, DateTime expireDate)
		{
			return (T)CallDeprecatedHook(plugin, oldHook, newHook, expireDate, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null);
		}
		public static object CallHook(BaseHookable plugin, string hookName, object arg1)
		{
			var buffer = _allocateBuffer(1);
			buffer[0] = arg1;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static T CallHook<T>(BaseHookable plugin, string hookName, object arg1)
		{
			var buffer = _allocateBuffer(1);
			buffer[0] = arg1;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static T CallDeprecatedHook<T>(BaseHookable plugin, string oldHook, string newHook, DateTime expireDate, object arg1)
		{
			var buffer = _allocateBuffer(1);
			buffer[0] = arg1;

			var result = CallDeprecatedHook(plugin, oldHook, newHook, expireDate, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static object CallHook(BaseHookable plugin, string hookName, object arg1, object arg2)
		{
			var buffer = _allocateBuffer(2);
			buffer[0] = arg1;
			buffer[1] = arg2;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static T CallHook<T>(BaseHookable plugin, string hookName, object arg1, object arg2)
		{
			var buffer = _allocateBuffer(2);
			buffer[0] = arg1;
			buffer[1] = arg2;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static T CallDeprecatedHook<T>(BaseHookable plugin, string oldHook, string newHook, DateTime expireDate, object arg1, object arg2)
		{
			var buffer = _allocateBuffer(2);
			buffer[0] = arg1;
			buffer[1] = arg2;

			var result = CallDeprecatedHook(plugin, oldHook, newHook, expireDate, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static object CallHook(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3)
		{
			var buffer = _allocateBuffer(3);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static T CallHook<T>(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3)
		{
			var buffer = _allocateBuffer(3);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static T CallDeprecatedHook<T>(BaseHookable plugin, string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3)
		{
			var buffer = _allocateBuffer(3);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;

			var result = CallDeprecatedHook(plugin, oldHook, newHook, expireDate, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static object CallHook(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3, object arg4)
		{
			var buffer = _allocateBuffer(4);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static T CallHook<T>(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3, object arg4)
		{
			var buffer = _allocateBuffer(4);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static T CallDeprecatedHook<T>(BaseHookable plugin, string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4)
		{
			var buffer = _allocateBuffer(4);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;

			var result = CallDeprecatedHook(plugin, oldHook, newHook, expireDate, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static object CallHook(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3, object arg4, object arg5)
		{
			var buffer = _allocateBuffer(5);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static T CallHook<T>(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3, object arg4, object arg5)
		{
			var buffer = _allocateBuffer(5);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static T CallDeprecatedHook<T>(BaseHookable plugin, string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5)
		{
			var buffer = _allocateBuffer(5);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;

			var result = CallDeprecatedHook(plugin, oldHook, newHook, expireDate, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static object CallHook(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
		{
			var buffer = _allocateBuffer(6);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static T CallHook<T>(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
		{
			var buffer = _allocateBuffer(6);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static T CallDeprecatedHook<T>(BaseHookable plugin, string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
		{
			var buffer = _allocateBuffer(6);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;

			var result = CallDeprecatedHook(plugin, oldHook, newHook, expireDate, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static object CallHook(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
		{
			var buffer = _allocateBuffer(7);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static T CallHook<T>(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
		{
			var buffer = _allocateBuffer(7);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static T CallDeprecatedHook<T>(BaseHookable plugin, string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
		{
			var buffer = _allocateBuffer(7);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;

			var result = CallDeprecatedHook(plugin, oldHook, newHook, expireDate, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static object CallHook(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
		{
			var buffer = _allocateBuffer(8);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;
			buffer[8] = arg8;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static T CallHook<T>(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
		{
			var buffer = _allocateBuffer(8);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;
			buffer[8] = arg8;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static T CallDeprecatedHook<T>(BaseHookable plugin, string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
		{
			var buffer = _allocateBuffer(8);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;
			buffer[8] = arg8;

			var result = CallDeprecatedHook(plugin, oldHook, newHook, expireDate, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static object CallHook(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
		{
			var buffer = _allocateBuffer(9);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;
			buffer[8] = arg8;
			buffer[9] = arg9;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static T CallHook<T>(BaseHookable plugin, string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
		{
			var buffer = _allocateBuffer(9);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;
			buffer[8] = arg8;
			buffer[9] = arg9;

			var result = CallHook(plugin, hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}
		public static T CallDeprecatedHook<T>(BaseHookable plugin, string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
		{
			var buffer = _allocateBuffer(9);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;
			buffer[8] = arg8;
			buffer[9] = arg9;

			var result = CallDeprecatedHook(plugin, oldHook, newHook, expireDate, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, buffer);

			_clearBuffer(buffer);
			return (T)result;
		}

		public static object CallStaticHook(string hookName)
		{
			return CallStaticHook(hookName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null);
		}
		public static object CallStaticDeprecatedHook(string oldHook, string newHook, DateTime expireDate)
		{
			return CallStaticDeprecatedHook(oldHook, newHook, expireDate, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null);
		}
		public static object CallStaticHook(string hookName, object arg1)
		{
			var buffer = _allocateBuffer(1);
			buffer[0] = arg1;

			var result = CallStaticHook(hookName, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1)
		{
			var buffer = _allocateBuffer(1);
			buffer[0] = arg1;

			var result = CallStaticDeprecatedHook(oldHook, newHook, expireDate, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticHook(string hookName, object arg1, object arg2)
		{
			var buffer = _allocateBuffer(2);
			buffer[0] = arg1;
			buffer[1] = arg2;

			var result = CallStaticHook(hookName, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2)
		{
			var buffer = _allocateBuffer(2);
			buffer[0] = arg1;
			buffer[1] = arg2;

			var result = CallStaticDeprecatedHook(oldHook, newHook, expireDate, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticHook(string hookName, object arg1, object arg2, object arg3)
		{
			var buffer = _allocateBuffer(3);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;

			var result = CallStaticHook(hookName, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3)
		{
			var buffer = _allocateBuffer(3);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;

			var result = CallStaticDeprecatedHook(oldHook, newHook, expireDate, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticHook(string hookName, object arg1, object arg2, object arg3, object arg4)
		{
			var buffer = _allocateBuffer(4);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;

			var result = CallStaticHook(hookName, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4)
		{
			var buffer = _allocateBuffer(4);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;

			var result = CallStaticDeprecatedHook(oldHook, newHook, expireDate, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5)
		{
			var buffer = _allocateBuffer(5);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;

			var result = CallStaticHook(hookName, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5)
		{
			var buffer = _allocateBuffer(5);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;

			var result = CallStaticDeprecatedHook(oldHook, newHook, expireDate, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
		{
			var buffer = _allocateBuffer(6);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;

			var result = CallStaticHook(hookName, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
		{
			var buffer = _allocateBuffer(6);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;

			var result = CallStaticDeprecatedHook(oldHook, newHook, expireDate, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
		{
			var buffer = _allocateBuffer(7);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;

			var result = CallStaticHook(hookName, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
		{
			var buffer = _allocateBuffer(7);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;

			var result = CallStaticDeprecatedHook(oldHook, newHook, expireDate, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
		{
			var buffer = _allocateBuffer(8);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;
			buffer[8] = arg8;

			var result = CallStaticHook(hookName, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8)
		{
			var buffer = _allocateBuffer(8);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;
			buffer[8] = arg8;

			var result = CallStaticDeprecatedHook(oldHook, newHook, expireDate, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticHook(string hookName, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
		{
			var buffer = _allocateBuffer(9);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;
			buffer[8] = arg8;
			buffer[9] = arg9;

			var result = CallStaticHook(hookName, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}
		public static object CallStaticDeprecatedHook(string oldHook, string newHook, DateTime expireDate, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9)
		{
			var buffer = _allocateBuffer(9);
			buffer[0] = arg1;
			buffer[1] = arg2;
			buffer[2] = arg3;
			buffer[3] = arg4;
			buffer[4] = arg5;
			buffer[6] = arg6;
			buffer[7] = arg7;
			buffer[8] = arg8;
			buffer[9] = arg9;

			var result = CallStaticDeprecatedHook(oldHook, newHook, expireDate, flag: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, args: buffer);

			_clearBuffer(buffer);
			return result;
		}

		private static object CallPublicHook<T>(this T plugin, string hookName, object[] args) where T : BaseHookable
		{
			return CallHook(plugin, hookName, BindingFlags.Public | BindingFlags.Instance, args);
		}
		private static object CallPublicStaticHook(string hookName, object[] args)
		{
			return CallStaticHook(hookName, BindingFlags.Public | BindingFlags.Instance, args);
		}
	}
}
