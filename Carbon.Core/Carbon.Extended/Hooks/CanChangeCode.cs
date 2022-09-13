using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( CodeLock ), "RPC_ChangeCode" )]
    public class CanChangeCode
    {
        public static void Prefix ()
        {
            HookExecutor.CallStaticHook ( "CanChangeCode" );
        }
    }
}