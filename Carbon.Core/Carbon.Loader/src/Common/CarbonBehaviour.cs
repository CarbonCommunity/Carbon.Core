using System;
using UnityEngine;

/*
 *
 * Copyright (c) 2022-2023 Carbon Community 
 * All rights reserved.
 *
 */

namespace Carbon.LoaderEx.Common;

internal abstract class CarbonBehaviour : MonoBehaviour
{
	///
	/// Removes the dependency on Facepunch.UnityEngine
	///
	internal void Invoke(Action action, float time)
		=> InvokeHandler.Invoke(this, action, time);

	internal void InvokeRepeating(Action action, float time, float repeat)
		=> InvokeHandler.InvokeRepeating(this, action, time, repeat);

	internal void InvokeRandomized(Action action, float time, float repeat, float random)
		=> InvokeHandler.InvokeRandomized(this, action, time, repeat, random);

	internal void CancelInvoke(Action action)
		=> InvokeHandler.CancelInvoke(this, action);

	internal bool IsInvoking(Action action)
		=> InvokeHandler.IsInvoking(this, action);

	internal void InvokeRepeatingFixedTime(Action action)
		=> InvokeHandlerFixedTime.InvokeRepeating(this, action, 0.01f, 0.01f);

	internal void CancelInvokeFixedTime(Action action)
		=> InvokeHandlerFixedTime.CancelInvoke(this, action);

	internal bool IsInvokingFixedTime(Action action)
		=> InvokeHandlerFixedTime.IsInvoking(this, action);
}
