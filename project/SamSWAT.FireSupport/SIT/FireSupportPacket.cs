using System.IO;
using EFT;
using SamSWAT.FireSupport.ArysReloaded.Unity;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using StayInTarkov;
using StayInTarkov.Coop.NetworkPacket;
using StayInTarkov.Coop.NetworkPacket.Player;
using StayInTarkov.Coop.Players;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.SIT
{
    public class FireSupportPacket : BasePlayerPacket
    {
        public FireSupportPacket()
        {
        }

        public FireSupportPacket(string profileId) : base(new string(profileId.ToCharArray()),
            nameof(FireSupportPacket))
        {
            Mode = "Strafe";
            Vector1 = new Vector3();
            Vector2 = new Vector3();
        }

        public string Mode { get; set; }
        public Vector3 Vector1 { get; set; }
        public Vector3 Vector2 { get; set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose();
        }

        public override byte[] Serialize()
        {
            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:Trying to serialize");
            var ms = new MemoryStream();

            StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}:Creating Binary Writer");
            using var writer = new BinaryWriter(ms);

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
            using var reader = new BinaryReader(new MemoryStream(bytes));
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

            if (client.GetPlayer.IsYourPlayer)
            {
                StayInTarkovHelperConstants.Logger.LogInfo($"{nameof(FireSupportPacket)}: Ignoring own packet");
                return;
            }

            // make sure fire suport controller is initialized 
            if (FireSupportController.Instance == null)
            {
                // at this point. This should not be a thing. But just in case.
                StayInTarkovHelperConstants.Logger.LogInfo(
                    $"{nameof(FireSupportPacket)}: Fire Support was not initialized. Initializing");
                await FireSupportHelper.InitController();
                StayInTarkovHelperConstants.Logger.LogInfo(
                    $"{nameof(FireSupportPacket)}: Fire Support is done initializing");
            }

            switch (Mode)
            {
                case "Ready":
                {
                    StayInTarkovHelperConstants.Logger.LogInfo(
                        $"{nameof(FireSupportPacket)}: Telling the team we are ready for fire support");
                    await FireSupportHelper.InitController();
                    break;
                }
                case "Extraction":
                {
                    StayInTarkovHelperConstants.Logger.LogInfo(
                        $"{nameof(FireSupportPacket)}: Calling Extraction CoRoutine {Vector1} | {Vector2}");
                    StaticManager.BeginCoroutine(FireSupportController.Instance.ExtractionRequest(Vector1, Vector2));
                    break;
                }
                case "Strafe":
                {
                    StayInTarkovHelperConstants.Logger.LogInfo(
                        $"{nameof(FireSupportPacket)}: Calling Strafe CoRoutine {Vector1} | {Vector2}");
                    StaticManager.BeginCoroutine(FireSupportController.Instance.StrafeRequest(Vector1, Vector2));
                    break;
                }
            }
        }
    }
}