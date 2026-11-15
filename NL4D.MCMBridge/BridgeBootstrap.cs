using System;
using System.ComponentModel;
using NL4D.Config;
using NL4D.MCMBridge.Config;

namespace NL4D.MCMBridge
{
    public static class BridgeBootstrap
    {
        public static bool TryRegister()
        {
            try
            {
                var mcmSettings = Nl4dMcmSettings.Instance;
                if (mcmSettings != null)
                {
                    mcmSettings.PropertyChanged += OnMcmSettingsChanged;
                }

                Nl4dSettingsManager.RegisterExternalSettingsProvider(BuildModelFromMcm);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void OnMcmSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            Nl4dSettingsManager.Reload();
        }

        private static Nl4dJsonModel BuildModelFromMcm()
        {
            Nl4dMcmSettings mcm = Nl4dMcmSettings.Instance ?? new Nl4dMcmSettings();
            return new Nl4dJsonModel
            {
                ModEnabled = mcm.ModEnabled,
                MinBaseSpawnChance = mcm.MinBaseSpawnChance,
                GuaranteedCasualtyThreshold = mcm.GuaranteedCasualtyThreshold,
                SpawnChanceMultiplier = mcm.SpawnChanceMultiplier,
                WinnerSideSurvivorWeight = mcm.WinnerSideSurvivorWeight,
                AllowExceedingCompanionLimit = mcm.AllowExceedingCompanionLimit,
                MaxUnrecruitedSurvivorsInWorld = mcm.MaxUnrecruitedSurvivorsInWorld,
                InitialWoundedHpRatioMin = mcm.InitialWoundedHpRatioMin,
                InitialWoundedHpRatioMax = mcm.InitialWoundedHpRatioMax,
                SurvivorRelationBonus = mcm.SurvivorRelationBonus,
                UseCustomNamePool = mcm.UseCustomNamePool,
                DebugForceSpawn = mcm.DebugForceSpawn
            };
        }
    }
}