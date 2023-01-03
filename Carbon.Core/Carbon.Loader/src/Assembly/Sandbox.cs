using System;
using System.IO;
using Carbon.LoaderEx.Utility;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.LoaderEx.ASM;

[Serializable]
public sealed class Sandbox<T> : IDisposable where T : MarshalByRefObject
{

	private AppDomain _domain;
	private string _identifier;
	private T _proxy;

	public T Do
	{ get => _proxy; }

	public Sandbox()
	{
		_identifier = $"sandbox_{Guid.NewGuid():N}";
		AppDomainSetup domaininfo = new AppDomainSetup();

		// this is still not perfect but it let's run with it for now.. ideally
		// the sandbox should be able to resolve their load requests using the 
		// domain assembly resolver event
		domaininfo.ApplicationBase = Path.Combine(
			System.Environment.CurrentDirectory, "carbon", "managed");

		_domain = AppDomain.CreateDomain(_identifier, null, domaininfo);

#if DEBUG
		Logger.Debug($"Created a new App Domain '{_identifier}'");
#endif

		Type type = typeof(T);
		_proxy = (T)_domain.CreateInstanceAndUnwrap(
			type.Assembly.FullName, type.FullName);

#if DEBUG
		Logger.Debug($" - Proxy to '{type.Name}' created");
#endif
	}

	public void Dispose()
	{
		if (_domain != null)
		{
			AppDomain.Unload(_domain);
			_domain = default;
		}
	}
}
