using System;
using System.Collections.Generic;
using System.Linq;
using NL4D.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NL4D.Calculators
{
    /// <summary>
    /// Generates immersive names for rescued survivors using custom culture pools,
    /// alive-hero deduplication, and survivor epithets.
    /// </summary>
    public static class SurvivorNameCalculator
    {
        public static (TextObject FullName, TextObject FirstName) GenerateName(CultureObject culture, bool isFemale)
        {
            string chosenFirstName;

            if (Nl4dSettingsManager.UseCustomNamePool)
            {
                var candidateNames = Nl4dSettingsManager.GetNamesForCulture(culture?.StringId);
                if (candidateNames == null || candidateNames.Count == 0)
                {
                    candidateNames = Nl4dSettingsManager.GetNamesForCulture("default");
                }

                // Check alive heroes to avoid duplicate names in Calradia if possible
                HashSet<string> aliveNames;
                try
                {
                    aliveNames = new HashSet<string>(
                        Hero.AllAliveHeroes.Where(h => h?.FirstName != null).Select(h => h.FirstName.ToString()),
                        StringComparer.OrdinalIgnoreCase
                    );
                }
                catch
                {
                    aliveNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

                var unusedCandidates = candidateNames.Where(n => !aliveNames.Contains(n)).ToList();
                var sourceList = unusedCandidates.Count > 0 ? unusedCandidates : candidateNames;

                chosenFirstName = sourceList.Count > 0
                    ? sourceList[MBRandom.RandomInt(sourceList.Count)]
                    : (isFemale ? "Sora" : "Darek");
            }
            else
            {
                TextObject? nativeName = null;
                try
                {
                    nativeName = NameGenerator.Current?.GenerateFirstNameForPlayer(culture, isFemale);
                }
                catch
                {
                    // Fallback if NameGenerator is unavailable
                }

                chosenFirstName = !string.IsNullOrWhiteSpace(nativeName?.ToString())
                    ? nativeName!.ToString()
                    : (isFemale ? "Sora" : "Darek");
            }

            // Pick epithet from settings pool
            var epithets = Nl4dSettingsManager.SurvivorEpithets;
            string chosenEpithet = (epithets != null && epithets.Count > 0)
                ? epithets[MBRandom.RandomInt(epithets.Count)]
                : "the Survivor";

            TextObject firstNameObj = new TextObject(chosenFirstName);
            TextObject fullNameObj = new TextObject("{=nl4d_name_with_epithet}{FIRST_NAME} {EPITHET}");
            fullNameObj.SetTextVariable("FIRST_NAME", firstNameObj);
            fullNameObj.SetTextVariable("EPITHET", new TextObject(chosenEpithet));

            return (fullNameObj, firstNameObj);
        }
    }
}
