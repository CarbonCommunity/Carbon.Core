﻿using Carbon.Core;
using Harmony;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;

namespace Carbon.Extended
{
    [OxideHook ( "OnTeamUpdate", typeof ( object ) ), OxideHook.Category ( OxideHook.Category.Enum.Team )]
    [OxideHook.Parameter ( "currentTeam", typeof ( ulong ) )]
    [OxideHook.Parameter ( "newTeam", typeof ( ulong ) )]
    [OxideHook.Parameter ( "player", typeof ( BasePlayer ) )]
    [OxideHook.Info ( "Called when player's team is updated." )]
    [OxideHook.Patch ( typeof ( BasePlayer ), "UpdateTeam" )]
    public class BasePlayer_UpdateTeam
    {
        public static bool Prefix ( ulong newTeam, ref BasePlayer __instance )
        {
            return Interface.CallHook("OnTeamUpdate", __instance.currentTeam, newTeam, __instance ) == null;
        }
    }
}