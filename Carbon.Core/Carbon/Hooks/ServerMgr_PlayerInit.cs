﻿using Harmony;
using Carbon.Core.Harmony;

[HarmonyPatch ( typeof ( BasePlayer ), "PlayerInit" )]
public class ServerMgr_PlayerInit
{
    public static void Postfix ( Network.Connection c )
    {
        HookExecutor.CallStaticHook ( "OnPlayerConnected", c.player as BasePlayer );
    }
}