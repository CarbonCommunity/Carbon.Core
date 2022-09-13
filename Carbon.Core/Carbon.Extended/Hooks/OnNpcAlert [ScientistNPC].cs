using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( ScientistNPC ), "Alert" )]
    public class OnNpcAlert [ScientistNPC]
    {
        public static void Prefix ()
        {
            HookExecutor.CallStaticHook ( "OnNpcAlert [ScientistNPC]" );
        }
    }
}