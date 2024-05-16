using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Airdrop;
using EFT.UI.Gestures;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using StayInTarkov;
using ModulePatch = StayInTarkov.ModulePatch;

namespace SamSWAT.FireSupport.ArysReloaded.Patches
{
    public class GesturesMenuPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GesturesMenu).GetMethod(nameof(GesturesMenu.Init));
        }

        [PatchPostfix]
        public static async void PatchPostfix(GesturesMenu __instance)
        {
            if (FireSupportHelper.GestureMenu == null)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is storing a reference to The GestureMenu");
                FireSupportHelper.GestureMenu = __instance;
            }

            if (!IsFireSupportAvailable())
            {
                StayInTarkovHelperConstants.Logger.LogInfo("No firesupport available");
                return;
            }

            await FireSupportHelper.InitController();
        }

        private static bool IsFireSupportAvailable()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("FireSupport Gesture Patch says there is no game world");
                return false;
            }

            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is finding suitable locations");
            var locationIsSuitable = gameWorld.MainPlayer.Location.ToLower() == "sandbox"
                                     || LocationScene.GetAll<AirdropPoint>().Any();

            if (!Plugin.Enabled.Value || !locationIsSuitable)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is not enabled");
                return false;
            }

            if (FireSupportHelper.FireSupportController != null)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("FireSupportController has already been init");
                return false;
            }


            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is looking at the registered players");
            var player = gameWorld.RegisteredPlayers[0];
            if (!(player is LocalPlayer))
            {
                StayInTarkovHelperConstants.Logger.LogInfo("FireSupport says this is not a local player");
                return false;
            }

            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is checking the inventory for a range finder");
            var inventory = player.Profile.Inventory;

            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is finding out if there is a range finder");
            var hasRangefinder = inventory.AllRealPlayerItems.Any(x => x.TemplateId == ItemConstants.RANGEFINDER_TPL);

            return hasRangefinder;
        }
    }
}