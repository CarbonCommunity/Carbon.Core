using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( BaseOven ), "SVSwitch" )]
    public class OnOvenToggle
    {
        public static void Prefix ()
        {
            HookExecutor.CallStaticHook ( "OnOvenToggle" );
        }
    }
}