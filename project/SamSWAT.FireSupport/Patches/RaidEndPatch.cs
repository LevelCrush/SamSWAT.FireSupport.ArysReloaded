using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using StayInTarkov;

namespace SamSWAT.FireSupport.ArysReloaded.Patches
{
    public class RaidEndPatch: ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(EFT.Player), "OnDestroy");
        }

        [PatchPrefix]
        private static void Cleanup(EFT.Player __instance)
        {
            StayInTarkovHelperConstants.Logger.LogInfo("Fire Support Helper: Raid End");
            if (Singleton<GameWorld>.Instance.MainPlayer.Id == __instance.Id)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("Cleanup FireSupport Helper and unload assets");
                FireSupportHelper.GestureMenu = null;
                FireSupportHelper.IsInit = false;
                FireSupportHelper.DoneFirstVoicer = false;
                FireSupportHelper.FireSupportController = null;
                
                
                StayInTarkovHelperConstants.Logger.LogInfo("Fire Support Helper is expliclity unloading all related bundles");
                AssetLoader.UnloadAllBundles();
            }
        }
    }
}