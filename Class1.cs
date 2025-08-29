using BepInEx;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using HarmonyLib;
using EntityStates.VoidSurvivor.Weapon;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;

namespace VoidFiendBeam
{
    [BepInPlugin("com.YourName.VoidFiendBeam", "Void Fiend Beam", "1.0.0")]
    [BepInDependency("com.rune580.riskofoptions")]
    public class VoidFiendBeam : BaseUnityPlugin
    {
        public static ConfigEntry<float> BeamRange;
        public static ConfigEntry<float> BeamVfxXScale;
        public static ConfigEntry<float> BeamVfxYScale;
        public static ConfigEntry<float> BeamVfxZScale;

        // Track if we've already modified the prefab
        private static bool hasPrefabBeenModified = false;
        private static GameObject originalPrefab = null;

        public void Awake()
        {
            BeamRange = Config.Bind("Beam Settings", "Beam Range", 75f, "The effective range of the Corrupt Hand Beam. Default is 75.");
            BeamVfxXScale = Config.Bind("Beam Settings", "Beam VFX X Scale", 1f, "The scale of the beam's visual effects (VFX) on the X-axis. Default is 1.");
            BeamVfxYScale = Config.Bind("Beam Settings", "Beam VFX Y Scale", 1f, "The scale of the beam's visual effects (VFX) on the Y-axis. Default is 1.");
            BeamVfxZScale = Config.Bind("Beam Settings", "Beam VFX Z Scale", 3.25f, "The scale of the beam's visual effects (VFX) on the Z-axis. Default is 3.25.");

            ModSettingsManager.AddOption(new SliderOption(BeamRange, new SliderConfig() { min = 10f, max = 200f, FormatString = "{0:0}" }));
            ModSettingsManager.AddOption(new SliderOption(BeamVfxXScale, new SliderConfig() { min = 0.1f, max = 10f, FormatString = "{0:0.0}" }));
            ModSettingsManager.AddOption(new SliderOption(BeamVfxYScale, new SliderConfig() { min = 0.1f, max = 10f, FormatString = "{0:0.0}" }));
            ModSettingsManager.AddOption(new SliderOption(BeamVfxZScale, new SliderConfig() { min = 0.1f, max = 20f, FormatString = "{0:0.0}" }));

            Harmony harmony = new Harmony("com.YourName.VoidFiendBeam");
            harmony.PatchAll();
            Logger.LogInfo("Void Fiend Beam has awakened!");
        }
    }

    [HarmonyPatch(typeof(FireCorruptHandBeam), "OnEnter")]
    public static class FireCorruptHandBeam_OnEnter_Patch
    {
        static void Postfix(FireCorruptHandBeam __instance)
        {
            // 1. Increase the beam's effective range
            __instance.maxDistance = VoidFiendBeam.BeamRange.Value;

            // 2. Modify the prefab BEFORE any instances are created
            if (__instance.beamVfxPrefab != null)
            {
                VoidFiendVFXBeam.ScaleBeamVFX(
                    __instance.beamVfxPrefab,
                    VoidFiendBeam.BeamVfxXScale.Value,
                    VoidFiendBeam.BeamVfxYScale.Value,
                    VoidFiendBeam.BeamVfxZScale.Value
                );
            }
        }
    }

    public static class VoidFiendVFXBeam
    {
        public static void ScaleBeamVFX(GameObject beamVfxPrefab, float newVfxXScale, float newVfxYScale, float newVfxZScale)
        {
            if (beamVfxPrefab == null) return;

            // Apply the configured scales to the X, Y, and Z axes
            beamVfxPrefab.transform.localScale = new Vector3(newVfxXScale, newVfxYScale, newVfxZScale);
        }
    }

    // NEW: Patch the class constructor to modify the prefab EARLY
    [HarmonyPatch(typeof(FireCorruptHandBeam))]
    public static class FireCorruptHandBeam_Constructor_Patch
    {
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPostfix]
        static void Postfix(FireCorruptHandBeam __instance)
        {
            // Modify the prefab as soon as the class is constructed
            if (__instance.beamVfxPrefab != null)
            {
                VoidFiendVFXBeam.ScaleBeamVFX(
                    __instance.beamVfxPrefab,
                    VoidFiendBeam.BeamVfxXScale.Value,
                    VoidFiendBeam.BeamVfxYScale.Value,
                    VoidFiendBeam.BeamVfxZScale.Value
                );
            }
        }
    }
}