using System;
using NL4D.Config;
using NL4D.Data;
using TaleWorlds.Core;

namespace NL4D.Calculators
{
    /// <summary>
    /// Calculates the probability of finding a living survivor at a battle wreckage site
    /// based on battle casualty counts, settings thresholds, and multiplier curves.
    /// </summary>
    public static class SurvivorSpawnCalculator
    {
        public static float CalculateSpawnChance(WreckageBattleContext context)
        {
            if (context == null || !Nl4dSettingsManager.ModEnabled)
            {
                return 0f;
            }

            if (Nl4dSettingsManager.DebugForceSpawn)
            {
                return 1f;
            }

            int guaranteedThreshold = Math.Max(1, Nl4dSettingsManager.GuaranteedCasualtyThreshold);
            if (context.TotalCasualties >= guaranteedThreshold)
            {
                return 1f;
            }

            float minChance = Math.Max(0f, Math.Min(1f, Nl4dSettingsManager.MinBaseSpawnChance));
            float casualtyRatio = Math.Max(0f, Math.Min(1f, (float)context.TotalCasualties / guaranteedThreshold));

            // Linear progression from minChance (at 0 casualties) to 1.0f (at GuaranteedCasualtyThreshold)
            float baseChance = minChance + (1f - minChance) * casualtyRatio;
            baseChance = Math.Max(0f, Math.Min(1f, baseChance));

            float finalChance = baseChance * Nl4dSettingsManager.SpawnChanceMultiplier;
            return Math.Max(0f, Math.Min(1f, finalChance));
        }

        public static bool RollForSurvivor(WreckageBattleContext context, bool forceSpawn = false)
        {
            if (context == null || !Nl4dSettingsManager.ModEnabled)
            {
                return false;
            }

            if (forceSpawn || Nl4dSettingsManager.DebugForceSpawn)
            {
                return true;
            }

            float chance = CalculateSpawnChance(context);
            if (chance <= 0f)
            {
                return false;
            }

            if (chance >= 1f)
            {
                return true;
            }

            return MBRandom.RandomFloat < chance;
        }
    }
}
