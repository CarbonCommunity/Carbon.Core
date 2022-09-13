using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( ItemModUpgrade ), "ServerCommand" )]
    public class OnItemUpgrade
    {
        public static void Prefix ()
        {
            HookExecutor.CallStaticHook ( "OnItemUpgrade" );
        }
    }
}