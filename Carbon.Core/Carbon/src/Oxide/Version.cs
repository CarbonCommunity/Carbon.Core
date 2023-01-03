/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core;

public struct VersionNumber
{
	public int Major;

	public int Minor;

	public int Patch;

	public VersionNumber(int major, int minor, int patch)
	{
		Major = major;
		Minor = minor;
		Patch = patch;
	}

	public override string ToString()
	{
		return $"{Major}.{Minor}.{Patch}";
	}

	public static bool operator ==(VersionNumber a, VersionNumber b)
	{
		if (a.Major == b.Major && a.Minor == b.Minor)
		{
			return a.Patch == b.Patch;
		}

		return false;
	}

	public static bool operator !=(VersionNumber a, VersionNumber b)
	{
		if (a.Major == b.Major && a.Minor == b.Minor)
		{
			return a.Patch != b.Patch;
		}

		return true;
	}

	public static bool operator >(VersionNumber a, VersionNumber b)
	{
		if (a.Major < b.Major)
		{
			return false;
		}

		if (a.Major > b.Major)
		{
			return true;
		}

		if (a.Minor < b.Minor)
		{
			return false;
		}

		if (a.Minor > b.Minor)
		{
			return true;
		}

		return a.Patch > b.Patch;
	}

	public static bool operator >=(VersionNumber a, VersionNumber b)
	{
		if (a.Major < b.Major)
		{
			return false;
		}

		if (a.Major > b.Major)
		{
			return true;
		}

		if (a.Minor < b.Minor)
		{
			return false;
		}

		if (a.Minor > b.Minor)
		{
			return true;
		}

		return a.Patch >= b.Patch;
	}

	public static bool operator <(VersionNumber a, VersionNumber b)
	{
		if (a.Major > b.Major)
		{
			return false;
		}

		if (a.Major < b.Major)
		{
			return true;
		}

		if (a.Minor > b.Minor)
		{
			return false;
		}

		if (a.Minor < b.Minor)
		{
			return true;
		}

		return a.Patch < b.Patch;
	}

	public static bool operator <=(VersionNumber a, VersionNumber b)
	{
		if (a.Major > b.Major)
		{
			return false;
		}

		if (a.Major < b.Major)
		{
			return true;
		}

		if (a.Minor > b.Minor)
		{
			return false;
		}

		if (a.Minor < b.Minor)
		{
			return true;
		}

		return a.Patch <= b.Patch;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is VersionNumber))
		{
			return false;
		}

		VersionNumber versionNumber = (VersionNumber)obj;
		return this == versionNumber;
	}

	public override int GetHashCode()
	{
		return ((17 * 23 + Major.GetHashCode()) * 23 + Minor.GetHashCode()) * 23 + Patch.GetHashCode();
	}
}
