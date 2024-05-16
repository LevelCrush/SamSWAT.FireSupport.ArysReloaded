using System.Collections;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using CommonAssets.Scripts.Game;
using EFT;
using EFT.UI;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public class HeliExfiltrationPoint : MonoBehaviour, IPhysicsTrigger
    {
        private readonly MethodInfo _stopSession;
        private Coroutine _coroutine;
        private float _timer;

        private HeliExfiltrationPoint()
        {
            var t = typeof(EndByExitTrigerScenario).GetNestedTypes().Single(x => x.IsInterface);
            _stopSession = t.GetMethod("StopSession");
        }

        private void OnDestroy()
        {
            if (Singleton<GameUI>.Instantiated) Singleton<GameUI>.Instance.BattleUiPanelExitTrigger.Close();

            if (_coroutine == null) return;
            StopCoroutine(_coroutine);
        }

        public string Description => "HeliExfiltrationPoint";

        public void OnTriggerEnter(Collider other)
        {
            var player = Singleton<GameWorld>.Instance.GetPlayerByCollider(other);
            if (player == null || !player.IsYourPlayer) return;

            _timer = Plugin.HelicopterExtractTime.Value;
            Singleton<GameUI>.Instance.BattleUiPanelExitTrigger.Show(_timer);
            _coroutine = StartCoroutine(Timer(player.ProfileId));
        }

        public void OnTriggerExit(Collider other)
        {
            var player = Singleton<GameWorld>.Instance.GetPlayerByCollider(other);
            if (player == null || !player.IsYourPlayer) return;

            _timer = Plugin.HelicopterExtractTime.Value;
            Singleton<GameUI>.Instance.BattleUiPanelExitTrigger.Close();

            if (_coroutine == null) return;
            StopCoroutine(_coroutine);
        }

        private IEnumerator Timer(string profileId)
        {
            while (_timer > 0)
            {
                _timer -= Time.deltaTime;
                yield return null;
            }


            _stopSession.Invoke(Singleton<AbstractGame>.Instance, new object[]
            {
                profileId,
                ExitStatus.Survived,
                "UH-60 BlackHawk"
            });
        }
    }
}