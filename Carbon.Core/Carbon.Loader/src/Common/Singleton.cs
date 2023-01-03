using System;
using Carbon.LoaderEx.Utility;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.LoaderEx.Common;

public abstract class Singleton<T> where T : class
{
	private static readonly Lazy<T> Instance
		= new Lazy<T>(() => CreateInstance());

	public static T GetInstance()
		=> Instance.Value;

	private static T CreateInstance()
	{
		Logger.Debug($"A singleton of {typeof(T)} was created");
		return Activator.CreateInstance(typeof(T), true) as T;
	}
}
