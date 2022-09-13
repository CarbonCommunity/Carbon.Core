using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( BaseMelee ), "PlayerAttack" )]
    public class OnMeleeAttack
    {
        public static void Prefix ()
        {
            HookExecutor.CallStaticHook ( "OnMeleeAttack" );
        }
    }
}