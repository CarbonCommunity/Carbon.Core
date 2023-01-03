using System;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Core;

public struct TimeMeasure : IDisposable
{
	// #if DEBUG
	// 		internal string _name;
	// 		internal string _warn;
	// 		internal int _miliseconds;
	// 		internal RealTimeSince _timeSince;
	// #endif

	public static TimeMeasure New(string name, int miliseconds = 75, string warn = null)
	{
		return default;
		// FIXME: get_realtimeSinceStartup can only be called from the main thread.
		// #if DEBUG
		// 			var result = default(TimeMeasure);
		// 			result._name = name;
		// 			result._warn = warn;
		// 			result._miliseconds = miliseconds;
		// 			result._timeSince = 0f;
		// 			return result;
		// #else
		// 			return default;
		// #endif
	}

	public void Dispose()
	{
		// FIXME: get_realtimeSinceStartup can only be called from the main thread.
		// #if DEBUG
		// 			var num = (float)_timeSince * 1000f;

		// 			if (num >= _miliseconds)
		// 			{
		// 				Carbon.Logger.Warn(
		// 					$" {_name} took {num:0}ms [abv {_miliseconds}]{(string.IsNullOrEmpty(_warn) ? "" : (": " + _warn))}");
		// 			}
		// #endif
	}
}
