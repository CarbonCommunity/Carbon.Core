///
/// Copyright (c) 2022 Carbon Community 
/// All rights reserved
/// 


namespace Carbon.Hooks
{
	[Hook.AlwaysPatched]
	[Hook("OnPlayerDisconnected"), Hook.Category(Hook.Category.Enum.Player)]
	[Hook.Parameter("this", typeof(BasePlayer))]
	[Hook.Parameter("reason", typeof(string), true)]
	[Hook.Info("Called after the player has disconnected from the server.")]
	[Hook.Patch(typeof(ServerMgr), "OnDisconnected")]
	public class OnPlayerDisconnected
	{
		public static void Postfix(string strReason, Network.Connection connection)
		{
			HookCaller.CallStaticHook("OnPlayerDisconnected", connection.player as BasePlayer, strReason);
		}
	}
}
