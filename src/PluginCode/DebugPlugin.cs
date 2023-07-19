using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using System;
using System.Collections;
using UnityEngine;
using KKAPI.MainGame;

namespace MaterialGreeting
{
    [BepInPlugin(GUID, PluginName, Version)]
    // Tell BepInEx that this plugin needs KKAPI of at least the specified version.
    // If not found, this plugi will not be loaded and a warning will be shown.
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public class DebugPlugin : BaseUnityPlugin
    {
        /// <summary>
        /// Human-readable name of the plugin. In general, it should be short and concise.
        /// This is the name that is shown to the users who run BepInEx and to modders that inspect BepInEx logs. 
        /// </summary>
        public const string PluginName = "MaterialGreetingDebugPlugin";

        /// <summary>
        /// Unique ID of the plugin. Will be used as the default config file name.
        /// This must be a unique string that contains only characters a-z, 0-9 underscores (_) and dots (.)
        /// When creating Harmony patches or any persisting data, it's best to use this ID for easier identification.
        /// </summary>
        public const string GUID = "mosirnik.material-greeting-debug-plugin";

        /// <summary>
        /// Version of the plugin. Must be in form <major>.<minor>.<build>.<revision>.
        /// Major and minor versions are mandatory, but build and revision can be left unspecified.
        /// </summary>
        public const string Version = "1.0.0";

        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;

            Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
            CharacterApi.RegisterExtraBehaviour<DebugController>(GUID);
            GameAPI.PeriodChange += OnPeriodChange;
        }

        private static void OnPeriodChange(object sender, GameAPI.PeriodChangeEventArgs args)
        {
            foreach (var heroine in Singleton<Manager.Game>.Instance.HeroineList)
            {
                if (heroine.fixCharaID == 0 && heroine.relation < 0)
                {
                    heroine.talkEvent.Add(1);
                    DebugPlugin.Logger.LogWarning($"Increasing relationship with {heroine.Name}");
                }
            }
        }

        [HarmonyPatch(typeof(Illusion.Utils.ProbabilityCalclator))]
        private static class Hooks
        {
            [HarmonyPatch(nameof(Illusion.Utils.ProbabilityCalclator.DetectFromPercent))]
            [HarmonyPostfix]
            public static void PostDetectFromPercent(float percent, ref bool __result)
            {
                DebugPlugin.Logger.LogWarning($"Cheating on {percent}% chance ({__result})");
                __result = true;
            }
        }

        private class DebugController : CharaCustomFunctionController
        {
            protected override void OnCardBeingSaved(GameMode mode)
            {
            }

            protected override void OnReload(GameMode currentGameMode)
            {
                DebugPlugin.Logger.LogWarning($"[{Time.frameCount}] OnReload");
                StartCoroutine(OnReloadCo());
            }

            private IEnumerator OnReloadCo()
            {
                while (true)
                {
                    bool top = ChaControl.objTop;
                    bool head = ChaControl.objHead;
                    DebugPlugin.Logger.LogWarning($"[{Time.frameCount}] top={top} head={head}");
                    if (top && head) break;
                    yield return null;
                }
            }
        }
    }
}
