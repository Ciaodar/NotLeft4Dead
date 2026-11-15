using System;
using System.Collections.Generic;
using System.Linq;
using NL4D.Config;
using NL4D.Data;
using TaleWorlds.CampaignSystem.Actions;

namespace NL4D.Services
{
    /// <summary>
    /// Manages the unrecruited survivor population pool.
    /// Uses FIFO eviction to enforce the global unrecruited companion cap,
    /// safely despawning the oldest unrecruited survivors to prevent save game bloat.
    /// </summary>
    public static class SurvivorFifoPoolService
    {
        public static int CountActiveUnrecruited(IEnumerable<RescuedSurvivorRecord>? records)
        {
            if (records == null)
            {
                return 0;
            }

            int count = 0;
            foreach (var record in records)
            {
                if (record != null && !record.IsRecruited && record.SurvivorHero != null && record.SurvivorHero.IsAlive)
                {
                    count++;
                }
            }

            return count;
        }

        public static void RegisterUnrecruitedAndEnforceCap(IList<RescuedSurvivorRecord> records, RescuedSurvivorRecord newRecord)
        {
            if (records == null || newRecord == null)
            {
                return;
            }

            if (!records.Contains(newRecord))
            {
                records.Add(newRecord);
            }

            // Prune dead heroes or invalid entries from unrecruited tracking
            for (int i = records.Count - 1; i >= 0; i--)
            {
                var record = records[i];
                if (record == null || record.SurvivorHero == null || (!record.IsRecruited && !record.SurvivorHero.IsAlive))
                {
                    records.RemoveAt(i);
                }
            }

            int maxAllowed = Math.Max(1, Nl4dSettingsManager.MaxUnrecruitedSurvivorsInWorld);

            while (CountActiveUnrecruited(records) > maxAllowed)
            {
                RescuedSurvivorRecord? oldest = records
                    .Where(r => r != null && !r.IsRecruited && r.SurvivorHero != null && r.SurvivorHero.IsAlive)
                    .OrderBy(r => r.RescuedTime.ToDays)
                    .FirstOrDefault();

                if (oldest == null)
                {
                    break;
                }

                if (oldest.SurvivorHero != null && oldest.SurvivorHero.IsAlive)
                {
                    try
                    {
                        KillCharacterAction.ApplyByRemove(oldest.SurvivorHero);
                    }
                    catch
                    {
                        // Safe removal fallback
                    }
                }

                records.Remove(oldest);
            }
        }
    }
}
