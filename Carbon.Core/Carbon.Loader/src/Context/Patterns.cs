

using System.Collections.Generic;
///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 
namespace Carbon.LoaderEx.Context;

internal sealed class Patterns
{
	internal static readonly string carbonNamePattern =
		@"(?i)^(carbon(?:\.(?:doorstop|loader))?)(_\w+)?$";

	internal static readonly string carbonFileNamePattern =
		@"(?i)^carbon([\.-](doorstop|loader))?(.dll)$";

	internal static readonly string oxideCompiledAssembly =
		@"(?i)^(script\.)(.+)(\.[-\w]+)";

	internal static readonly List<string> refWhitelist = new List<string>
	{
		// Facepunch managed refs
		@"^Assembly-CSharp(-firstpass)?$",
		@"^Facepunch(.\w+(.\w+)+)?$",
		@"^Newtonsoft(.\w+)?$",
		@"^Rust(.\w+(.\w+)+)?$",
		@"^Unity(.\w+(.\w+)+)?$",
		@"^UnityEngine(.\w+)?$",

		// Carbon managed refs
		@"^Carbon(-\d+)?$",

		// System stuff
		@"^mscorlib$",
		@"^System.Drawing(.\w+)?$",
		@"^System.Core$",
		@"^System.Xml(.\w+)?$",
		@"^System$",
	};

	internal static readonly Dictionary<string, string> refTranslator = new Dictionary<string, string>
	{
		// special case: carbon random asm name
		{ @"^Carbon(-\d+)?$", "Carbon" },
		{ @"^0Harmony$", "1Harmony" }
	};
}

