using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using SamSWAT.FireSupport.ArysReloaded.SIT;
using StayInTarkov;
using StayInTarkov.Coop;
using StayInTarkov.Coop.Players;
using StayInTarkov.Coop.SITGameModes;
using ModulePatch = StayInTarkov.ModulePatch;

namespace SamSWAT.FireSupport.ArysReloaded.Patches
{
    public class FireSupportNetworkPacketPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(CoopSITGame).GetMethod("CreateExfiltrationPointAndInitDeathHandler", BindingFlags.Public | BindingFlags.Instance);
        }
    
       

        [PatchPostfix]
        public static  void PatchPostfix()
        { 
            
            StayInTarkovHelperConstants.Logger.LogInfo("Trying to patch in FireSupport Network Packet");
           var fields =
               typeof(StayInTarkovHelperConstants).GetFields(BindingFlags.Static | BindingFlags.NonPublic);

           foreach (var field in fields)
           {
               if (field.Name == "_sitTypes")
               {
                   StayInTarkovHelperConstants.Logger.LogInfo("PacketPatch found _sitTypes");
                   var new_types = new List<Type>();
                   new_types.Add(typeof(FireSupportPacket));
                   var merged = StayInTarkovHelperConstants.SITTypes.Union(new_types).ToArray();
                   field.SetValue(null, merged);
                   StayInTarkovHelperConstants.Logger.LogInfo("PacketPatch merged  _sitTypes");
                   break;
               }
           }
           StayInTarkovHelperConstants.Logger.LogInfo("Done patching in FireSupport network packets");
        }
    }
}
