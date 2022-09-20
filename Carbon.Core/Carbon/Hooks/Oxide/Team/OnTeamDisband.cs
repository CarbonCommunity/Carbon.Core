﻿using Carbon.Core;
using Harmony;
using Oxide.Core;
using UnityEngine;

namespace Carbon.Extended
{
    [OxideHook ( "OnTeamDisband", typeof ( object ) ), OxideHook.Category ( OxideHook.Category.Enum.Team )]
    [OxideHook.Parameter ( "team", typeof ( RelationshipManager.PlayerTeam ) )]
    [OxideHook.Info ( "Useful for canceling team disbandment." )]
    [OxideHook.Patch ( typeof ( RelationshipManager ), "DisbandTeam" )]
    public class RelationshipManager_DisbandTeam_OnTeamDisband
    {
        public static bool Prefix ( ref RelationshipManager.PlayerTeam teamToDisband, ref RelationshipManager __instance )
        {
            if ( Interface.CallHook ( "OnTeamDisband", teamToDisband ) != null )
            {
                return false;
            }

            __instance.teams.Remove ( teamToDisband.teamID );
            Interface.CallHook ( "OnTeamDisbanded", teamToDisband );
            Facepunch.Pool.Free ( ref teamToDisband );
            return false;
        }
    }
}