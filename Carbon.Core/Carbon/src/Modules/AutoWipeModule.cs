using System;
using Carbon.Base;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Modules;

public class AutoWipeModule : CarbonModule<AutoWipeConfig, AutoWipeData>
{
	public override string Name => "AutoWipe";
	public override Type Type => typeof(AutoWipeModule);
}

public class AutoWipeConfig
{
	public int NextWipeSeed = -1;
}
public class AutoWipeData
{

}
