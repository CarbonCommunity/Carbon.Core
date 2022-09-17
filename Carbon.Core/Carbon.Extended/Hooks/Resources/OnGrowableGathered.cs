﻿using Carbon.Core;
using Harmony;
using Oxide.Core;
using UnityEngine;

namespace Carbon.Extended
{
    [Hook ( "OnGrowableGathered" ), Hook.Category ( Hook.Category.Enum.Resources )]
    [Hook.Parameter ( "this", typeof ( GrowableEntity ) )]
    [Hook.Parameter ( "item", typeof ( Item ) )]
    [Hook.Parameter ( "player", typeof ( BasePlayer ) )]
    [Hook.Info ( "Called before the player receives an item from gathering a growable entity." )]
    [HarmonyPatch ( typeof ( GrowableEntity ), "GiveFruit" )]
    public class GrowableEntity_GiveFruit
    {
        public static bool Prefix ( BasePlayer player, int amount, bool applyCondition, ref GrowableEntity __instance )
        {
            var item = ItemManager.Create ( __instance.Properties.pickupItem, amount, 0UL );

            if ( applyCondition )
            {
                item.conditionNormalized = __instance.Properties.fruitVisualScaleCurve.Evaluate ( __instance.StageProgressFraction );
            }
            
            if ( player != null )
            {
                Interface.CallHook ( "OnGrowableGathered", __instance, item, player );
                player.GiveItem ( item, BaseEntity.GiveItemReason.PickedUp );
                return false;
            }

            item.Drop ( __instance.transform.position + Vector3.up * 0.5f, Vector3.up * 1f, default ( Quaternion ) );
            return false;
        }
    }
}