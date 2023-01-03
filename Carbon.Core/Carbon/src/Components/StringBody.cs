using System;
using System.Collections.Generic;
using Carbon.Extensions;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Components;

public struct StringBody : IDisposable
{
	private List<object> _items { get; set; }
	public List<object> Items { get { return _items ?? (_items = new List<object>()); } }

	public StringBody Add(object data)
	{
		Items.Add(data ?? "");

		return this;
	}
	public StringBody Add(object[] datas)
	{
		Items.AddRange(datas);

		return this;
	}
	public StringBody Empty()
	{
		Add(data: null);

		return this;
	}
	public StringBody Remove(object data)
	{
		Items.Remove(data);

		return this;
	}
	public StringBody Clear()
	{
		Items.Clear();

		return this;
	}

	public enum ExportTypes
	{
		Append,
		NewLine
	}

	#region "To" Family

	public string ToAppended(string inBetweenString = " ")
	{
		var result = "";

		foreach (var item in Items)
		{
			result += $"{item}{inBetweenString}";
		}

		return result.Trim();
	}
	public string ToNewLine()
	{
		var result = "";

		foreach (var item in Items)
		{
			result += $"{item}\n";
		}

		return result.Trim();
	}
	public override string ToString()
	{
		return ToAppended("");
	}

	#endregion

	#region "Export" Family

	public StringBody Export(string filePath, ExportTypes exportType, bool @override = true, string inBetweenString = " ", int padding = 50, char paddingCharacter = ' ', bool spacing = true)
	{
		if (!@override && OsEx.File.Exists(filePath))
		{
			return this;
		}

		try
		{
			switch (exportType)
			{
				case ExportTypes.Append:
					OsEx.File.Create(filePath, ToAppended(inBetweenString));
					break;

				case ExportTypes.NewLine:
					OsEx.File.Create(filePath, ToNewLine());
					break;
			}
		}
		catch { }

		return this;
	}

	#endregion

	public void Dispose()
	{
		Items.Clear();
		_items = null;
	}
}
