using StayInTarkov.Coop.NetworkPacket;
using System.IO;
using System.Text;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using StayInTarkov;
using StayInTarkov.Coop.Components.CoopGameComponents;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.Players;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.SIT
{
    public class FireSupportPacket : BasePlayerPacket
    {
        
        public string Mode { get; set;  }
        public Vector3 Vector1 { get; set; }
        public Vector3 Vector2 { get; set; }
        
        public FireSupportPacket(string profileId) : base(new string(profileId.ToCharArray()), nameof(FireSupportPacket))
        {
            Mode = "Strafe";
            Vector1 = new Vector3();
            Vector2 = new Vector3();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose();
     
        }
        
        public override byte[] Serialize()
        {
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:Trying to serialize");
            var ms = new MemoryStream();

            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:Creating Binary Writer");
            using BinaryWriter writer = new BinaryWriter(ms);
            
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:Writing Header");
            WriteHeaderAndProfileId(writer);
            
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:{Mode}");
            writer.Write(Mode);
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:{Vector1}");
            writer.Write(Vector1);
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:{Vector2}");
            writer.Write(Vector2);
            
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:Done setting");
            
            return ms.ToArray();
        }

        public override ISITPacket Deserialize(byte[] bytes)
        {
            
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:Creating Binary Reader");
            using BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            ReadHeaderAndProfileId(reader);
            
            Mode = reader.ReadString();
            
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:{Mode} is set");
            Vector1 = reader.ReadVector3();
            
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:{Vector1} is set");
            
            Vector2 = reader.ReadVector3();
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:{Vector2} is set");
            
            return this;
        }

        protected override async void Process(CoopPlayerClient client)
        {
            
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}: Processing Fire Support Packet");
           
            // make sure fire suport controller is initialized 
            if (FireSupportController.Instance == null)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("FireSupportPacket did not find FireSupportController.");

                // technically...no gesture menu 
                // should be fine(i think)
                await FireSupportController.Init(null);
                
                StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}: Done Init with FireSupportController");
            }
            
            switch (Mode)
            {
                case "Extraction":
                {
                    StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}: Calling Extraction CoRoutine {Vector1} | {Vector2}");
                    StaticManager.BeginCoroutine(FireSupportController.Instance.ExtractionRequest(Vector1, Vector2));
                    break;
                }
                case "Strafe":
                {
                    StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}: Calling Strafe CoRoutine {Vector1} | {Vector2}");
                    StaticManager.BeginCoroutine(FireSupportController.Instance.StrafeRequest(Vector1, Vector2));
                    break;
                }
            }
        }
    }
}