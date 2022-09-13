using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( ResourceDispenser ), "GiveResourceFromItem" )]
    public class OnDispenserGather
    {
        public static void Prefix ()
        {
            HookExecutor.CallStaticHook ( "OnDispenserGather" );
        }
    }
}