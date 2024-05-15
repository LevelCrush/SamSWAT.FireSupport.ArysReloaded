using System;
using Comfort.Common;
using EFT;
using EFT.Airdrop;
using EFT.InputSystem;
using EFT.UI.Gestures;
using HarmonyLib;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SamSWAT.FireSupport.ArysReloaded.SIT;
using StayInTarkov;
using StayInTarkov.Coop.SITGameModes;
using UnityEngine;
using ModulePatch = Aki.Reflection.Patching.ModulePatch;

namespace SamSWAT.FireSupport.ArysReloaded.Patches
{
    public class FireSupportNetworkPacketPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CoopSITGame).GetMethod("CreateExfiltrationPointAndInitDeathHandler", BindingFlags.Public | BindingFlags.Instance);
        }

        [Aki.Reflection.Patching.PatchPostfix]
        public static  void PatchPostfix()
        { 
            StayInTarkovHelperConstants.Logger.LogInfo("Trying to patch in FireSupport Network Packet");
           var fields =
               typeof(StayInTarkovHelperConstants).GetFields(BindingFlags.Static | BindingFlags.NonPublic);

           foreach (var field in fields)
           {
               StayInTarkovHelperConstants.Logger.LogInfo($"Packet Patch found: {field.Name}");
               
               if (field.Name == "_sitTypes")
               {
                   StayInTarkovHelperConstants.Logger.LogInfo("PacketPatch found _sitTypes");
                   var new_types = new List<Type>();
                   new_types.Add(typeof(FireSupportPacket));
                   var merged = StayInTarkovHelperConstants.SITTypes.Union(new_types).ToArray();
                   field.SetValue(null, merged);
                   StayInTarkovHelperConstants.Logger.LogInfo("PacketPatch merged  _sitTypes");
               }
           }
           StayInTarkovHelperConstants.Logger.LogInfo("Done patching in FireSupport network packets");
        }
    }
}
