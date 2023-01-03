using System;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Base.Interfaces;

public interface IModule : IHookableModule, IDisposable
{
	string Name { get; }

	void Init();
	void InitEnd();
	void Save();
	void Load();

	void SetEnabled(bool enabled);
	bool GetEnabled();
	void OnEnableStatus();

	void OnEnabled(bool initialized);
	void OnDisabled(bool initialized);
}
