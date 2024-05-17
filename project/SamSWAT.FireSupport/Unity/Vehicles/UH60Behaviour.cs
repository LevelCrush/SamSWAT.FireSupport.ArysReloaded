using System;
using System.Collections;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using StayInTarkov;
using StayInTarkov.Coop.SITGameModes;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public class UH60Behaviour : MonoBehaviour, IFireSupportOption
    {
        private static readonly int FlySpeedMultiplier = Animator.StringToHash("FlySpeedMultiplier");
        private static readonly int FlyAway = Animator.StringToHash("FlyAway");
        [SerializeField] private Animator helicopterAnimator;
        [SerializeField] private AnimationCurve volumeCurve;
        public AudioSource engineCloseSource;
        public AudioSource engineDistantSource;
        public AudioSource rotorsCloseSource;
        public AudioSource rotorsDistantSource;
        private GameObject _extractionPoint;

        private ExfiltrationPoint heli_point;


        private void Update()
        {
            CrossFadeAudio();
        }

        public void ProcessRequest(Vector3 position, Vector3 direction, Vector3 rotation)
        {
            var heliTransform = transform;
            heliTransform.position = position;
            heliTransform.eulerAngles = rotation;
            helicopterAnimator.SetFloat(FlySpeedMultiplier, Plugin.HelicopterSpeedMultiplier.Value);
        }

        public void ReturnToPool()
        {
            gameObject.SetActive(false);
        }

        private void CrossFadeAudio()
        {
            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            if (player == null) return;

            var distance = Vector3.Distance(player.CameraPosition.position, rotorsCloseSource.transform.position);
            var volume = volumeCurve.Evaluate(distance);

            rotorsCloseSource.volume = volume;
            engineCloseSource.volume = volume - 0.2f;
            rotorsDistantSource.volume = 1 - volume;
            engineDistantSource.volume = 1 - volume;
        }

        private IEnumerator OnHelicopterArrive()
        {
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliPickingUp);
            CreateExfilPoint();
            var waitTime = Plugin.HelicopterWaitTime.Value * 0.75f;
            yield return new WaitForSeconds(waitTime);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliHurry);
            yield return new WaitForSeconds(Plugin.HelicopterWaitTime.Value - waitTime);

            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is sending the helicopter away. Closing extract");
            var sit_game = Singleton<ISITGame>.Instance as CoopSITGame;
            if (sit_game != null)
            {
                heli_point.Status = EExfiltrationStatus.NotPresent;
                heli_point.enabled = false;
                //heli_point.Disable(EExfiltrationStatus.Pending); 

                StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is updating the exilftration ui to remove it");
                sit_game.UpdateExfiltrationUi(heli_point, false);
            }


            helicopterAnimator.SetTrigger(FlyAway);
            Destroy(_extractionPoint);
            FireSupportAudio.Instance.PlayVoiceover(EVoiceoverType.SupportHeliLeavingNoPickup);
        }

        private void OnHelicopterLeft()
        {
            ReturnToPool();
        }

        private void CreateExfilPoint()
        {
            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is grabbing the SIT Game");
            var sit_game = Singleton<ISITGame>.Instance as CoopSITGame;
            if (sit_game == null)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("SIT game is null. Cant continue");
                return;
            }

            // _extractionPoint.AddComponent<HeliExfiltrationPoint>(); */
            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is attempting to add the helicoper exfil");
            StayInTarkovHelperConstants.Logger.LogInfo("Attempt 15");
            _extractionPoint = new GameObject
            {
                name = "Helicopter",
                layer = 13,
                transform =
                {
                    position = transform.position,
                    eulerAngles = new Vector3(-90, 0, 0)
                }
            };


            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is setting the colider for the zone");
            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is setting the colider for the zone");
            var extractionCollider = _extractionPoint.AddComponent<BoxCollider>();
            extractionCollider.size = new Vector3(16.5f, 20f, 15);
            extractionCollider.transform.position = _extractionPoint.transform.position;
            extractionCollider.isTrigger = true;


            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is attempting to add exfil point to object");
            heli_point = _extractionPoint.AddComponent<ExfiltrationPoint>();


            if (heli_point == null)
            {
                StayInTarkovHelperConstants.Logger.LogInfo("FireSupport helicopter extraction point is not defined");
                return;
            }


            var settings = new Settings7
            {
                Count = 0,
                PlayersCount = 0,
                Chance = 100,
                EntryPoints = sit_game.PlayerOwner.Player.Profile.Info.EntryPoint.ToLower(),
                MinTime = 0,
                MaxTime = 0,
                ExfiltrationTime = 8,
                EventAvailable = true,
                RequiredSlot = EquipmentSlot.FirstPrimaryWeapon,
                RequirementTip = "",
                PassageRequirement = ERequirementState.None,
                Id = "",
                Name = "Helicopter",
                ExfiltrationType = EExfiltrationType.Individual
            };

            heli_point.LoadSettings(settings, true);
            heli_point.Settings.Name = settings.Name;
            heli_point.Settings.EntryPoints = settings.EntryPoints;
            heli_point.transform.position = _extractionPoint.transform.position;
            heli_point.transform.eulerAngles = _extractionPoint.transform.eulerAngles;
            //  heli_point.transform.localScale = new Vector3(16.5f, 20f, 15);
            //var heli_box_colider = heli_point.GetComponent<BoxCollider>();
            // heli_box_colider.isTrigger = true;
            // heli_box_colider.enabled = true;


            StayInTarkovHelperConstants.Logger.LogInfo(
                $"FireSupport after loading these settings \r\n{heli_point.Settings.ToPrettyJson()}");

            //heli_point.SetStatusLogged(EExfiltrationStatus.RegularMode, "FireSupport.HeliExtract");

            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is hooking events for the exfil");
            heli_point.OnStartExtraction += OnExtract;
            heli_point.OnStatusChanged += OnStatusChanged;
            heli_point.OnCancelExtraction += OnCancel;


            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is adding to the exfiltration controller ");
            ExfiltrationControllerClass.Instance.ExfiltrationPoints.AddItem(heli_point);


            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is attempting to access the timer panel");
            StayInTarkovHelperConstants.Logger.LogInfo(
                $"FireSupport heli point settings are\r\n{heli_point.Settings.ToPrettyJson()}");

            sit_game.GameUi.TimerPanel.SetTime(DateTime.UtcNow, sit_game.PlayerOwner.Player.Side,
                sit_game.GameTimer.SessionSeconds(), new[] { heli_point });

            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is updating the exilftration ui");
            sit_game.UpdateExfiltrationUi(heli_point, false, true);
        }

        private void OnExtract(ExfiltrationPoint point, Player player)
        {
            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is starting extraction");
            var target_method = typeof(CoopSITGame).GetMethod("ExfiltrationPoint_OnStartExtraction",
                BindingFlags.NonPublic | BindingFlags.Instance);
            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is invoking the start of extraction ");
            object[] param_args = { point, player };
            target_method.Invoke(Singleton<ISITGame>.Instance, param_args);
        }

        private void OnCancel(ExfiltrationPoint point, Player player)
        {
            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is  canceling extraction");
            var target_method = typeof(CoopSITGame).GetMethod("ExfiltrationPoint_OnCancelExtraction",
                BindingFlags.NonPublic | BindingFlags.Instance);

            object[] param_args = { point, player };

            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is invoking the cancelation of extraction ");
            target_method.Invoke(Singleton<ISITGame>.Instance, param_args);
        }

        private void OnStatusChanged(ExfiltrationPoint point, EExfiltrationStatus prevStatus)
        {
            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is changing the status of the  extraction");
            var target_method = typeof(CoopSITGame).GetMethod("ExfiltrationPoint_OnStatusChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);

            object[] param_args = { point, prevStatus };

            StayInTarkovHelperConstants.Logger.LogInfo("FireSupport is invoking the status change of extraction ");
            target_method.Invoke(Singleton<ISITGame>.Instance, param_args);
        }
    }
}