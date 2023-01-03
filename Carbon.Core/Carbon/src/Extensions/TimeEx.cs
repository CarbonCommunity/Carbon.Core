using System;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Extensions;

public static class TimeEx
{
	/// <summary>
	/// Time formatting that includes date as well. Miliseconds, seconds, minutes, hours, days and weeks of the value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="v"></param>
	/// <param name="shortName">Shows formatting short: ms, m, s, h, etc. or miliseconds, minutes, seconds, hours, etc.</param>
	/// <param name="showMiliseconds">Show miliseconds when is under the "showMilisecondsUnderSeconds" parameter.</param>
	/// <param name="showMilisecondsUnderSeconds">As the parameter's name says.</param>
	/// <returns></returns>
	public static string Format<T>(T v, bool shortName = true, bool showMiliseconds = false) where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
	{
		var value = (float)Convert.ChangeType(v, typeof(float));

		var miliseconds = (int)((value - (int)value) * 10);
		var seconds = (long)value;
		var minutes = System.Math.Floor(value / 60.0);
		var hours = System.Math.Floor(minutes / 60.0);
		var days = System.Math.Floor(hours / 24.0);
		var weeks = System.Math.Floor(days / 7.0);

		var milisecond = "";
		var second = "";
		var minute = "";
		var hour = "";
		var day = "";
		var week = "";

		if (shortName)
		{
			if (showMiliseconds)
			{
				if (seconds < 5)
				{
					return seconds == 0 && miliseconds == 0 ? string.Format("0s") : string.Format("{0}{1}", string.Format(seconds == 0L ? "" : "{0}s", seconds), string.Format("{0}ms", miliseconds, milisecond));
				}
			}

			if (seconds < 60L)
			{
				return string.Format("{0}s", seconds);
			}

			if (minutes < 60.0)
			{
				return string.Format("{1}{0}", string.Format((seconds % 60L) == 0L ? "" : "{0}s", (seconds % 60L), second), string.Format("{0}m", minutes, minute), hours, days, weeks);
			}

			if (hours < 48.0)
			{
				return string.Format("{2}{1}{0}", string.Format((seconds % 60L) == 0L ? "" : "{0}s", (seconds % 60L)), string.Format((minutes % 60L) == 0L ? "" : "{0}m", (minutes % 60L)), string.Format("{0}h", hours), days, weeks);
			}

			if (days < 7.0)
			{
				return string.Format("{3}{2}{1}{0}", string.Format((seconds % 60L) == 0L ? "" : "{0}s", (seconds % 60L)), string.Format((minutes % 60.0) == 0L ? "" : "{0}m", (minutes % 60.0)), string.Format("{0}h", (hours % 24.0)), string.Format("{0}d", (days % 7.0)), weeks);
			}

			return string.Format("{4}{3}{2}{1}{0}", string.Format((seconds % 60L) == 0L ? "" : "{0}s", (seconds % 60L)), string.Format((minutes % 60L) == 0L ? "" : "{0}m", (minutes % 60L)), string.Format((hours % 24.0) == 0 ? "" : "{0}h", (hours % 24.0)), string.Format((days % 7.0) == 0 ? "" : "{0}d", (days % 7.0)), string.Format("{0}w", weeks));
		}
		else
		{
			if (showMiliseconds)
			{
				if (seconds < 5)
				{
					milisecond = miliseconds != 1 ? "Miliseconds" : "Milisecond";
					second = seconds != 1 ? "Seconds" : "Second";

					return seconds == 0 && miliseconds == 0 ? string.Format("0 Seconds") : string.Format("{0}{1}", string.Format(seconds == 0L ? "" : "{0} {1}, ", seconds, second), string.Format("{0} {1}", miliseconds, milisecond));
				}
			}

			if (seconds < 60L)
			{
				second = seconds != 1 ? "Seconds" : "Second";

				return string.Format("{0} {1}", seconds, second);
			}
			if (minutes < 60.0)
			{
				second = (seconds % 60L) != 1 ? "Seconds" : "Second";
				minute = minutes != 1 ? "Minutes" : "Minute";

				return string.Format("{1}{0}", string.Format((seconds % 60L) == 0L ? "" : " and {0} {1}", (seconds % 60L), second), string.Format("{0} {1}", minutes, minute), hours, days, weeks);
			}
			if (hours < 24.0)
			{
				second = (seconds % 60L) != 1 ? "Seconds" : "Second";
				minute = (minutes % 60.0) != 1 ? "Minutes" : "Minute";
				hour = hours != 1 ? "Hours" : "Hour";

				return string.Format("{2}{1}{0}", string.Format((seconds % 60L) == 0L ? "" : " and {0} {1}", (seconds % 60L), second), string.Format((minutes % 60L) == 0L ? "" : (seconds % 60L) == 0 ? " and {0} {1}" : ", {0} {1}", (minutes % 60L), minute), string.Format("{0} {1}", hours, hour), days, weeks);
			}
			if (days < 7.0)
			{
				second = (seconds % 60L) != 1 ? "Seconds" : "Second";
				minute = (minutes % 60.0) != 1 ? "Minutes" : "Minute";
				hour = (hours % 24.0) != 1 ? "Hours" : "Hour";
				day = (days % 7.0) != 1 ? "Days" : "Day";

				return string.Format("{3}{2}{1}{0}", string.Format((seconds % 60L) == 0L ? "" : " and {0} {1}", (seconds % 60L), second), string.Format((minutes % 60.0) == 0L ? "" : " and {0} {1}", (minutes % 60.0), minute), string.Format((minutes % 60.0) > 0 ? ", {0} {1}" : " and {0} {1}", (hours % 24.0), hour), string.Format("{0} {1}", (days % 7.0), day), weeks);
			}

			second = (seconds % 60L) != 1 ? "Seconds" : "Second";
			minute = (minutes % 60.0) != 1 ? "Minutes" : "Minute";
			hour = (hours % 24.0) != 1 ? "Hours" : "Hour";
			day = (days % 7.0) != 1 ? "Days" : "Day";
			week = weeks != 1 ? "Weeks" : "Week";

			return string.Format("{4}{3}{2}{1}{0}", string.Format((seconds % 60L) == 0L ? "" : " and {0} {1}", (seconds % 60L), second), string.Format((minutes % 60L) == 0L ? "" : (seconds % 60L) == 0 ? " and {0} {1}" : ", {0} {1}", (minutes % 60L), minute), string.Format((hours % 24.0) == 0 ? "" : ", {0} {1}", (hours % 24.0), hour), string.Format((days % 7.0) == 0 ? "" : ", {0} {1}", (days % 7.0), day), string.Format("{0} {1}", weeks, week));
		}
	}

	/// <summary>
	/// It can work with: [ms] - miliseconds, [s] - seconds, [m] - minutes, [h] - hours, [d] - dayse and [w] - weeks.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="v"></param>
	/// <param name="format"></param>
	/// <param name="integerFormat"></param>
	/// <returns></returns>
	public static string FormatPlayer<T>(T v, string format = "[m]:[s].[ms]", string integerFormat = "00")
	{
		var value = (float)Convert.ChangeType(v, typeof(float));

		var miliseconds = (int)((value - (int)value) * 10);
		var seconds = (long)value % 60L;
		var minutes = System.Math.Floor(value / 60.0);
		var hours = System.Math.Floor(minutes / 60.0);
		var days = System.Math.Floor(hours / 24.0);
		var weeks = System.Math.Floor(days / 7.0);

		format = format.Replace("[ms]", miliseconds.ToString(integerFormat));
		format = format.Replace("[s]", seconds.ToString(integerFormat));
		format = format.Replace("[m]", minutes.ToString(integerFormat));
		format = format.Replace("[h]", hours.ToString(integerFormat));
		format = format.Replace("[d]", days.ToString(integerFormat));
		format = format.Replace("[w]", weeks.ToString(integerFormat));

		return format;
	}

	public static string FormatPlayer<T>(T v, bool showMiliseconds, string integerFormat = "00")
	{
		var format = "";
		var value = (float)Convert.ChangeType(v, typeof(float));

		var miliseconds = (int)((value - (int)value) * 10);
		var seconds = (long)value % 60L;
		var minutes = System.Math.Floor(value / 60.0);
		var hours = System.Math.Floor(minutes / 60.0);
		var days = System.Math.Floor(hours / 24.0);
		var weeks = System.Math.Floor(days / 7.0);


		format += weeks > 1 ? (weeks).ToString(integerFormat) + ":" : "";

		format += (days % 7.0) > 1 ? days.ToString(integerFormat) + ":" : weeks > 0 ? days.ToString(integerFormat) + ":" : "";

		format += (hours % 24.0) > 0 ? hours.ToString(integerFormat) + ":" : days > 0 || weeks > 0 ? hours.ToString(integerFormat) + ":" : "";

		format += (minutes % 60.0).ToString(integerFormat) + ":";

		format += (seconds % 60L).ToString(integerFormat);

		if (showMiliseconds)
		{
			format += "." + miliseconds.ToString(integerFormat);
		}

		return format;
	}
}
