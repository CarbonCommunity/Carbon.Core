﻿using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [OxideHook ( "OnEntitySpawned" ), OxideHook.Category ( OxideHook.Category.Enum.Entity )]
    [OxideHook.Parameter ( "entity", typeof ( BaseNetworkable ) )]
    [OxideHook.Info ( "Called after any networked entity has spawned (including trees)." )]
    [OxideHook.Patch ( typeof ( BaseNetworkable ), "Spawn" )]
    public class BaseNetworkable_Spawn_OnEntitySpawned
    {
        public static void Postfix ( ref BaseNetworkable __instance )
        {
            HookExecutor.CallStaticHook ( "OnEntitySpawned", __instance );
        }
    }
}