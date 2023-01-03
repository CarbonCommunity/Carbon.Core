using System;
using System.Collections.Generic;
using Carbon.Base;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.Processors;

public class CarbonProcessor : BaseProcessor
{
	public override void Start() { }
	public override void OnDestroy() { }
	public override void Dispose() { }

	public Queue<Action> OnFrameQueue = new Queue<Action>();

	public void Update()
	{
		if (OnFrameQueue.Count <= 0) return;

		try
		{
			OnFrameQueue.Dequeue()?.Invoke();
		}
		catch (Exception exception)
		{
			Carbon.Logger.Error($"Failed to execute OnFrame callback ({exception.Message})\n{exception.StackTrace}");
		}
	}
}
