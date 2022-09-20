﻿using Carbon.Core;
using Harmony;
using Oxide.Core;

namespace Carbon.Extended
{
    [OxideHook ( "OnItemRemove", typeof ( object ) ), OxideHook.Category ( OxideHook.Category.Enum.Item )]
    [OxideHook.Parameter ( "this", typeof ( Item ) )]
    [OxideHook.Info ( "Called before an item is destroyed." )]
    [OxideHook.Info ( "Return a non-null value stop item from being destroyed." )]
    [OxideHook.Patch ( typeof ( Item ), "Remove" )]
    public class Item_Remove
    {
        public static bool Prefix ( float fTime, ref Item __instance )
        {
            if ( __instance.removeTime > 0f )
            {
                return true;
            }

            return Interface.CallHook ( "OnItemRemove", __instance ) == null;
        }
    }
}