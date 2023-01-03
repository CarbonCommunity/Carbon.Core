using System;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Oxide.Core.Libraries;

public abstract class Library : IDisposable
{
	public virtual void Dispose() { }
}
