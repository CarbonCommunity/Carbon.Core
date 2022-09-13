using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( ConsoleNetwork ), "SendClientCommand" )]
    public class OnSendCommand
    {
        public static void Prefix ()
        {
            HookExecutor.CallStaticHook ( "OnSendCommand" );
        }
    }
}