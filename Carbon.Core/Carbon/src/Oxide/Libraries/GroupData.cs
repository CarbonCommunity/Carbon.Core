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
public class GroupData
{
	public string Title { get; set; } = string.Empty;

	public int Rank { get; set; }

	public HashSet<string> Perms { get; set; } = new HashSet<string>();

	public string ParentGroup { get; set; } = string.Empty;
}
