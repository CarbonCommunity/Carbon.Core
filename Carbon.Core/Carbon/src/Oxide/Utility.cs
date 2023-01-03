using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core;

public class Utility
{
	public static void DatafileToProto<T>(string name, bool deleteAfter = true)
	{
		var dataFileSystem = Interface.Oxide.DataFileSystem;

		if (!dataFileSystem.ExistsDatafile(name))
		{
			return;
		}

		if (ProtoStorage.Exists(name))
		{
			Carbon.Logger.Warn($"Failed to import JSON file: {name} already exists.");
			return;
		}

		try
		{
			ProtoStorage.Save<T>(dataFileSystem.ReadObject<T>(name), name);

			if (deleteAfter)
			{
				File.Delete(dataFileSystem.GetFile(name).Filename);
			}
		}
		catch (Exception ex)
		{
			Carbon.Logger.Error("Failed to convert datafile to proto storage: " + name, ex);
		}
	}

	public static void PrintCallStack()
		=> Carbon.Logger.Log(string.Format("CallStack: {0}{1}", new StackTrace(1, true)));

	public static string FormatBytes(double bytes)
	{
		string arg;

		if (bytes > 1048576.0)
		{
			arg = "mb";
			bytes /= 1048576.0;
		}
		else if (bytes > 1024.0)
		{
			arg = "kb";
			bytes /= 1024.0;
		}
		else
		{
			arg = "b";
		}
		return string.Format("{0:0}{1}", bytes, arg);
	}

	public static string GetDirectoryName(string name)
	{
		string result;

		try
		{
			name = name.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			result = name.Substring(0, name.LastIndexOf(Path.DirectorySeparatorChar));
		}
		catch
		{
			result = null;
		}
		return result;
	}

	public static string GetFileNameWithoutExtension(string value)
	{
		int num = value.Length - 1;

		for (int i = num; i >= 1; i--)
		{
			if (value[i] == '.')
			{
				num = i - 1;
				break;
			}
		}

		int num2 = 0;

		for (int j = num - 1; j >= 0; j--)
		{
			char c = value[j];
			if (c == '/' || c == '\\')
			{
				num2 = j + 1;
				break;
			}
		}

		return value.Substring(num2, num - num2 + 1);
	}

	public static string CleanPath(string path)
	{
		if (path == null)
		{
			return null;
		}

		return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
	}

	public static T ConvertFromJson<T>(string jsonstr)
	{
		return JsonConvert.DeserializeObject<T>(jsonstr);
	}

	public static string ConvertToJson(object obj, bool indented = false)
	{
		return JsonConvert.SerializeObject(obj, indented ? Formatting.Indented : Formatting.None);
	}

	public static IPAddress GetLocalIP()
	{
		UnicastIPAddressInformation unicastIPAddressInformation = null;
		foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
		{
			if (networkInterface.OperationalStatus == OperationalStatus.Up)
			{
				IPInterfaceProperties ipproperties = networkInterface.GetIPProperties();
				if (ipproperties.GatewayAddresses.Count != 0 && !ipproperties.GatewayAddresses[0].Address.Equals(IPAddress.Parse("0.0.0.0")))
				{
					foreach (UnicastIPAddressInformation unicastIPAddressInformation2 in ipproperties.UnicastAddresses)
					{
						if (unicastIPAddressInformation2.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(unicastIPAddressInformation2.Address))
						{
							if (!unicastIPAddressInformation2.IsDnsEligible)
							{
								if (unicastIPAddressInformation == null)
								{
									unicastIPAddressInformation = unicastIPAddressInformation2;
								}
							}
							else
							{
								if (unicastIPAddressInformation2.PrefixOrigin == PrefixOrigin.Dhcp)
								{
									return unicastIPAddressInformation2.Address;
								}
								if (unicastIPAddressInformation == null || !unicastIPAddressInformation.IsDnsEligible)
								{
									unicastIPAddressInformation = unicastIPAddressInformation2;
								}
							}
						}
					}
				}
			}
		}
		if (unicastIPAddressInformation == null)
		{
			return null;
		}
		return unicastIPAddressInformation.Address;
	}

	internal static string[] _dotSplit = new string[] { "." };

	public static bool IsLocalIP(string ipAddress)
	{
		var array = ipAddress.Split(_dotSplit, StringSplitOptions.RemoveEmptyEntries); ;

		var array2 = new int[]
		{
			int.Parse(array[0]),
			int.Parse(array[1]),
			int.Parse(array[2]),
			int.Parse(array[3])
		};

		return array2[0] == 0 || array2[0] == 10 || (array2[0] == 100 && array2[1] == 64) || array2[0] == 127 || (array2[0] == 192 && array2[1] == 168) || (array2[0] == 172 && array2[1] >= 16 && array2[1] <= 31);
	}

	public static bool ValidateIPv4(string ipAddress)
	{
		if (string.IsNullOrEmpty(ipAddress.Trim()))
		{
			return false;
		}

		var array = ipAddress.Replace("\"", string.Empty).Trim().Split(_dotSplit, StringSplitOptions.RemoveEmptyEntries);

		if (array.Length == 4)
		{
			return array.All(delegate (string r)
			{
				return byte.TryParse(r, out _);
			});
		}
		return false;
	}

	public static int GetNumbers(string input)
	{
		int result;
		int.TryParse(Regex.Replace(input, "[^.0-9]", ""), out result);
		return result;
	}
}
