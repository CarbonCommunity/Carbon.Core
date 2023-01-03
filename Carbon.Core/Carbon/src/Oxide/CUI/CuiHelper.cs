using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Core;
using UnityEngine;
using Formatting = Newtonsoft.Json.Formatting;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Game.Rust.Cui;

public static class CuiHelper
{
	internal static JsonSerializerSettings _cuiSettings = new JsonSerializerSettings
	{
		DefaultValueHandling = DefaultValueHandling.Ignore
	};

	public static string ToJson(List<CuiElement> elements, bool format = false)
	{
		return JsonConvert.SerializeObject(elements, format ? Formatting.Indented : Formatting.None, _cuiSettings).Replace("\\n", "\n");
	}

	public static List<CuiElement> FromJson(string json) => JsonConvert.DeserializeObject<List<CuiElement>>(json);

	public static string GetGuid() => $"{Guid.NewGuid():N}";

	public static bool AddUi(BasePlayer player, List<CuiElement> elements) => AddUi(player, ToJson(elements));

	public static bool AddUi(BasePlayer player, string json)
	{
		if (player?.net != null && Interface.CallHook("CanUseUI", player, json) == null)
		{
			CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo { connection = player.net.connection }, null, "AddUI", json);
			return true;
		}

		return false;
	}

	public static bool DestroyUi(BasePlayer player, string name)
	{
		if (player?.net != null)
		{
			Interface.CallHook("OnDestroyUI", player, name);
			CommunityEntity.ServerInstance.ClientRPCEx(new Network.SendInfo { connection = player.net.connection }, null, "DestroyUI", name);
			return true;
		}

		return false;
	}

	public static void SetColor(this ICuiColor elem, Color color)
	{
		elem.Color = $"{color.r} {color.g} {color.b} {color.a}";
	}

	public static Color GetColor(this ICuiColor elem) => ColorEx.Parse(elem.Color);
}

[JsonConverter(typeof(ComponentConverter))]
public interface ICuiComponent
{
	[JsonProperty("type")]
	string Type { get; }
}

public interface ICuiColor
{
	[JsonProperty("color")]
	string Color { get; set; }
}
