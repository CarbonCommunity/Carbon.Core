using Carbon.Core;
using Harmony;

namespace Carbon.Extended
{
    [HarmonyPatch ( typeof ( BaseMountable ), "MountPlayer" )]
    public class CanMountEntity
    {
        public static void Prefix ()
        {
            HookExecutor.CallStaticHook ( "CanMountEntity" );
        }
    }
}