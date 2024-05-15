using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.InputSystem;
using EFT.UI.Gestures;
using HarmonyLib;
using SamSWAT.FireSupport.ArysReloaded.SIT;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using StayInTarkov;
using StayInTarkov.Networking;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils
{
    public static class FireSupportHelper
    {
        public static bool IsInit { get; set; }
        public static bool DoneFirstVoicer { get; set; }
        
        public static GesturesMenu GestureMenu { get; set; }
        
        public static FireSupportController FireSupportController { get; set; }
        
        public static async Task<FireSupportController> InitController()
        {
       
            if (!FireSupportHelper.IsInit)
            {
                var owner = Singleton<GameWorld>.Instance.MainPlayer.GetComponent<GamePlayerOwner>();
                FireSupportHelper.FireSupportController = await FireSupportController.Init(FireSupportHelper.GestureMenu);
                
                Traverse.Create(owner).Field<List<InputNode>>("_children").Value.Add( FireSupportHelper.FireSupportController);
                var gesturesBindPanel =
                    FireSupportHelper.GestureMenu.gameObject.GetComponentInChildren<GesturesBindPanel>(true);
                gesturesBindPanel.transform.localPosition = new Vector3(0, -530, 0);
                FireSupportHelper.IsInit = true;
            }

            return FireSupportHelper.FireSupportController;
        }

        public static void SendReadyPacket()
        {
            if (!FireSupportHelper.IsInit)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("Sending fire support packet for reading up");
                var packet = new FireSupportPacket(Singleton<GameWorld>.Instance.MainPlayer.ProfileId);
                packet.Mode = "Ready";
                packet.Vector1 = Vector3.one;
                packet.Vector2 = Vector3.one;
                GameClient.SendData(packet.Serialize());
            }
        }
        
    }
}