using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( FireBall ), "TryToSpread" )]
    public class OnFireBallSpread
    {
        public static void Prefix ()
        {
            HookExecutor.CallStaticHook ( "OnFireBallSpread" );
        }
    }
}