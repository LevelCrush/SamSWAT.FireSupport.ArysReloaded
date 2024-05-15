using EFT;
using EFT.InputSystem;
using EFT.UI;
using EFT.UI.Gestures;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using System.Collections;
using System.Threading.Tasks;
using Comfort.Common;
using SamSWAT.FireSupport.ArysReloaded.SIT;
using StayInTarkov;
using StayInTarkov.Networking;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public class FireSupportController : UIInputNode
    {
        private bool _requestAvailable = true;
        private Coroutine _timerCoroutine;
        private FireSupportAudio _audio;
        private FireSupportUI _ui;
        private FireSupportSpotter _spotter;
        public GesturesMenu _gesturesMenu;

        public static FireSupportController Instance { get; private set; }
        public bool StrafeRequestAvailable => _requestAvailable && AvailableStrafeRequests > 0;
        public bool ExtractRequestAvailable => _requestAvailable && AvailableExtractRequests > 0;
        public bool AnyRequestAvailable => _requestAvailable && (AvailableStrafeRequests > 0 || AvailableExtractRequests > 0);
        public int AvailableStrafeRequests { get; private set; }
        public int AvailableExtractRequests { get; private set; }

        public static async Task<FireSupportController> Init(GesturesMenu gesturesMenu)
        {
            Instance = new GameObject("FireSupportController").AddComponent<FireSupportController>();
            Instance._audio = await FireSupportAudio.Load();
            Instance._spotter = await FireSupportSpotter.Load();
            Instance._ui = await FireSupportUI.Load(gesturesMenu);
            Instance._gesturesMenu = gesturesMenu;
            await FireSupportPool.LoadBundlesAndCreatePools();
            WeaponClass.Init();
            Instance.AvailableStrafeRequests = Plugin.AmountOfStrafeRequests.Value;
            Instance.AvailableExtractRequests = Plugin.AmountOfExtractionRequests.Value;
            if (!FireSupportHelper.DoneFirstVoicer)
            {
                FireSupportHelper.DoneFirstVoicer = true;
                Instance._audio.PlayVoiceover(EVoiceoverType.StationReminder);
            }

            Instance._ui.SupportRequested += Instance.OnSupportRequested;
            return Instance;
        }

        private void OnSupportRequested(ESupportType supportType)
        {
            switch (supportType)
            {
                case ESupportType.Strafe:
                    if (StrafeRequestAvailable)
                    {
                        _gesturesMenu.Close();
                        StaticManager.BeginCoroutine(_spotter.SpotterSequence(ESupportType.Strafe, (b, startPos, endPos) =>
                        {
                            if (!b)
                            {
                               
                                StaticManager.BeginCoroutine(StrafeRequest(startPos, endPos));
                                StayInTarkovHelperConstants.Logger.LogInfo("Sending fire support packet for Strafing");
                                var packet = new FireSupportPacket(Singleton<GameWorld>.Instance.MainPlayer.ProfileId);
                                packet.Mode = "Strafe";
                                packet.Vector1 = startPos;
                                packet.Vector2 = endPos;
                                GameClient.SendData(packet.Serialize());
                            }
                        }));
                    }
                    break;
                case ESupportType.Extract:
                    if (ExtractRequestAvailable)
                    {
                        _gesturesMenu.Close();
                        StaticManager.BeginCoroutine(_spotter.SpotterSequence(ESupportType.Extract, (b, pos, rot) =>
                        {
                            if (!b)
                            {
                               
                                StaticManager.BeginCoroutine(ExtractionRequest(pos, rot));
                                StayInTarkovHelperConstants.Logger.LogInfo("Sending fire support packet for extraction");
                                var packet = new FireSupportPacket(Singleton<GameWorld>.Instance.MainPlayer.ProfileId);
                                packet.Mode = "Extraction";
                                packet.Vector1 = pos;
                                packet.Vector2 = rot;
                                GameClient.SendData(packet.Serialize());
                            }
                        }));
                    }
                    break;
            }
        }

        public IEnumerator StrafeRequest(Vector3 strafeStartPos, Vector3 strafeEndPos)
        {
            StayInTarkovHelperConstants.Logger.LogInfo("Fire Support: Performing Strafe request");
            var a10 = FireSupportPool.TakeFromPool(ESupportType.Strafe);
            var pos = (strafeStartPos + strafeEndPos) / 2;
            var dir = (strafeEndPos - strafeStartPos).normalized;
            AvailableStrafeRequests--;
            _timerCoroutine = StaticManager.BeginCoroutine(Timer(Plugin.RequestCooldown.Value));
            _audio.PlayVoiceover(EVoiceoverType.StationStrafeRequest);
            yield return new WaitForSecondsRealtime(8f);
            _audio.PlayVoiceover(EVoiceoverType.JetArriving);
            a10.ProcessRequest(pos, dir, Vector3.zero);
        }

        public IEnumerator ExtractionRequest(Vector3 position, Vector3 rotation)
        {
            StayInTarkovHelperConstants.Logger.LogInfo("Fire Support: Performing Extraction request");
            var uh60 = FireSupportPool.TakeFromPool(ESupportType.Extract);
            AvailableExtractRequests--;
            _requestAvailable = false;
            _audio.PlayVoiceover(EVoiceoverType.StationExtractionRequest);
            yield return new WaitForSecondsRealtime(8f);
            uh60.ProcessRequest(position, Vector3.zero, rotation);
            _audio.PlayVoiceover(EVoiceoverType.SupportHeliArrivingToPickup);
            yield return new WaitForSecondsRealtime(35f + Plugin.HelicopterWaitTime.Value);
            if (Instance == null) yield break;
            _timerCoroutine = StaticManager.BeginCoroutine(Timer(Plugin.RequestCooldown.Value));
        }

        private IEnumerator Timer(float time)
        {
            _requestAvailable = false;
            _ui.timerText.enabled = true;
            while (time > 0)
            {
                time -= Time.deltaTime;
                if (time < 0)
                {
                    time = 0;
                }

                float minutes = Mathf.FloorToInt(time / 60);
                float seconds = Mathf.FloorToInt(time % 60);
                _ui.timerText.text = $"{minutes:00}.{seconds:00}";
                yield return null;
            }
            _ui.timerText.enabled = false;
            _requestAvailable = true;
            if (AnyRequestAvailable)
            {
                FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.StationAvailable);
            }
        }

        public override ETranslateResult TranslateCommand(ECommand command)
        {
            if (command == ECommand.MumbleToggle)
            {
                Debug.LogError("MUMBLE");
            }

            return ETranslateResult.Ignore;
        }

        public override void TranslateAxes(ref float[] axes)
        {
        }

        public override ECursorResult ShouldLockCursor()
        {
            return ECursorResult.Ignore;
        }

        private void OnDestroy()
        {
            AssetLoader.UnloadAllBundles();
            _ui.SupportRequested -= OnSupportRequested;
            StaticManager.KillCoroutine(ref _timerCoroutine);
        }
    }
}