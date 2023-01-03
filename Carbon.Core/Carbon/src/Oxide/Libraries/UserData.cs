using System.Collections.Generic;
using ProtoBuf;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core.Libraries;

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public class UserData
{
	public string LastSeenNickname { get; set; } = "Unnamed";
	public string Language { get; set; } = "en";

	public HashSet<string> Perms { get; set; } = new HashSet<string>();
	public HashSet<string> Groups { get; set; } = new HashSet<string>();
}
