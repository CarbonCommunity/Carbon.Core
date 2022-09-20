﻿using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [OxideHook ( "CanTakeCutting", typeof ( object ) ), OxideHook.Category ( OxideHook.Category.Enum.Resources )]
    [OxideHook.Parameter ( "player", typeof ( BasePlayer ) )]
    [OxideHook.Parameter ( "this", typeof ( GrowableEntity ) )]
    [OxideHook.Info ( "Called when a player is trying to take a cutting (clone) of a GrowableEntity." )]
    [OxideHook.Patch ( typeof ( GrowableEntity ), "TakeClones" )]
    public class GrowableEntity_TakeClones
    {
        public static bool Prefix ( BasePlayer player, ref GrowableEntity __instance )
        {
            if ( player == null )
            {
                return false;
            }
            if ( !__instance.CanClone () )
            {
                return false;
            }

            return HookExecutor.CallStaticHook ( "CanTakeCutting", player, __instance ) == null;
        }
    }
}