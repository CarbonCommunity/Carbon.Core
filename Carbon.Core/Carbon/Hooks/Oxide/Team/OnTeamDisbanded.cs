﻿using Carbon.Core;
using ConVar;
using Harmony;
using Oxide.Core;
using UnityEngine;

namespace Carbon.Extended
{
    [OxideHook ( "OnTeamDisbanded"), OxideHook.Category ( OxideHook.Category.Enum.Team )]
    [OxideHook.Parameter ( "team", typeof ( RelationshipManager.PlayerTeam ) )]
    [OxideHook.Info ( "Called when the team was disbanded." )]
    [OxideHook.Patch ( typeof ( RelationshipManager ), "DisbandTeam" )]
    public class RelationshipManager_DisbandTeam_OnTeamDisbanded
    {
        public static void Postfix ( RelationshipManager.PlayerTeam teamToDisband )
        {
        }
    }
}