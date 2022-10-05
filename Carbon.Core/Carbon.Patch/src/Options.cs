﻿using CommandLine;

namespace Carbon.Patch
{
	public class Options
	{
		[Option('p', "path", Required = true,
			HelpText = "Base path for build root.")]
		public string basePath { get; set; }

		[Option('c', "configuration", Required = false,
			Default = "Debug", HelpText = ".NET target build configuration.")]
		public string targetConfiguration { get; set; }
	}
}
