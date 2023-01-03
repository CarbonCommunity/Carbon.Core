using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Extensions;

public static class ByteEx
{
	public enum ByteTypes
	{
		Auto,
		Byte,
		Kilobyte,
		Megabyte,
		Gigabyte,
		Terabyte,
		Petabyte,
		Exabyte
	}

	public static string Format<T>(this T value, ByteTypes type = ByteTypes.Auto, bool shortName = true, string valueFormat = "0.0", string stringFormat = "{0} {1}") where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
	{
		long num1 = (long)Convert.ChangeType(value, typeof(long));
		double num2 = 0;
		string byteType = "";

		// Exabyte
		if (num1 >= 1152921504606846976L && type == ByteTypes.Auto || type == ByteTypes.Exabyte)
		{
			byteType = shortName ? "Eb" : "Exabytes";
			num2 = (num1 >> 50);
		}

		// Petabyte
		else if (num1 >= 1125899906842624L && type == ByteTypes.Auto || type == ByteTypes.Petabyte)
		{
			byteType = shortName ? "Pb" : "Petabytes";
			num2 = (num1 >> 40);
		}

		// Terabyte
		else if (num1 >= 1099511627776L && type == ByteTypes.Auto || type == ByteTypes.Terabyte)
		{
			byteType = shortName ? "Tb" : "Terabytes";
			num2 = (num1 >> 30);
		}

		// Gigabyte
		else if (num1 >= 1073741824L && type == ByteTypes.Auto || type == ByteTypes.Gigabyte)
		{
			byteType = shortName ? "Gb" : "Gigabytes";
			num2 = (num1 >> 20);
		}

		// Megabyte
		else if (num1 >= 1048576L && type == ByteTypes.Auto || type == ByteTypes.Megabyte)
		{
			byteType = shortName ? "Mb" : "Megabytes";
			num2 = (num1 >> 10);
		}

		// Kilobyte or Byte
		else
		{
			if (num1 < 1024L && type == ByteTypes.Auto || type == ByteTypes.Byte)
			{
				byteType = shortName ? "B" : "Bytes";
				return string.Format(stringFormat, num1.ToString(valueFormat), byteType);
			}

			if (type == ByteTypes.Auto || type == ByteTypes.Kilobyte)
			{
				byteType = shortName ? "Kb" : "Kilobytes";
				num2 = num1;
			}
		}

		return string.Format(stringFormat, (num2 / 1024.0).ToString(valueFormat), byteType);
	}
	public static T MakeCopy<T>(this T source)
	{
		if (!typeof(T).IsSerializable)
		{
			throw new ArgumentException("The type must be serializable.", "source");
		}

		if (source == null)
		{
			return default(T);
		}

		var formatter = new BinaryFormatter();
		var stream = new MemoryStream();

		using (stream)
		{
			formatter.Serialize(stream, source);
			stream.Seek(0, SeekOrigin.Begin);

			return (T)formatter.Deserialize(stream);
		}
	}
}
