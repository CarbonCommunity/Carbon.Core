﻿using Carbon.Core;
using CompanionServer.Handlers;
using Harmony;
using Oxide.Core;
using ProtoBuf;

namespace Carbon.Extended
{
    [OxideHook ( "OnMeleeAttack", typeof ( bool ) ), OxideHook.Category ( OxideHook.Category.Enum.Player )]
    [OxideHook.Parameter ( "player", typeof ( BasePlayer ) )]
    [OxideHook.Parameter ( "hitInfo", typeof ( HitInfo ) )]
    [OxideHook.Info ( "Useful for canceling melee attacks." )]
    [OxideHook.Patch ( typeof ( BaseMelee ), "PlayerAttack" )]
    public class BaseMelee_PlayerAttack
    {
        public static bool Prefix ( BaseEntity.RPCMessage msg, ref BaseMelee __instance )
        {
            var player = msg.player;
            var result = true;
            var oldPosition = msg.read.Position;

            if ( !__instance.VerifyClientAttack ( player ) )
            {
                return true;
            }
            using ( var playerAttack = ProtoBuf.PlayerAttack.Deserialize ( msg.read ) )
            {
                if ( playerAttack != null )
                {
                    var hitInfo = Facepunch.Pool.Get<HitInfo> ();
                    hitInfo.LoadFromAttack ( playerAttack.attack, true );
                    hitInfo.Initiator = player;
                    hitInfo.Weapon = __instance;
                    hitInfo.WeaponPrefab = __instance;
                    hitInfo.Predicted = msg.connection;
                    hitInfo.damageProperties = __instance.damageProperties;
                    result = HookExecutor.CallStaticHook ( "OnMeleeAttack", player, hitInfo ) == null;
                    Facepunch.Pool.Free ( ref hitInfo );
                }
            }

            msg.read.Position = oldPosition;
            return result;
        }
    }
}