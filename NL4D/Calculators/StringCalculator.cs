using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NL4D.Calculators
{
    /// <summary>
    /// Utility for localized strings with dynamic variant randomization (_count / _N convention)
    /// and session-cached dialog variable resolution.
    /// </summary>
    public static class StringCalculator
    {
        private static readonly Dictionary<string, string> RolledVariants = new Dictionary<string, string>();

        private static int GetVariantCount(string baseId)
        {
            string countText = new TextObject("{=" + baseId + "_count}0").ToString();
            return int.TryParse(countText, out var count) && count > 0 ? count : 0;
        }

        public static string GetString(string baseId, string fallbackString)
        {
            int variantCount = GetVariantCount(baseId);
            string id = variantCount > 0
                ? "{=" + baseId + "_" + MBRandom.RandomInt(1, variantCount + 1) + "}"
                : "{=" + baseId + "}";

            return new TextObject(id + fallbackString).ToString();
        }

        public static void SetDialogVariable(string baseId, string fallbackString)
        {
            SetDialogVariable(baseId, baseId, fallbackString);
        }

        public static void SetDialogVariable(string variableName, string baseId, string fallbackString)
        {
            if (!RolledVariants.TryGetValue(baseId, out string id))
            {
                int variantCount = GetVariantCount(baseId);
                id = variantCount > 0
                    ? "{=" + baseId + "_" + MBRandom.RandomInt(1, variantCount + 1) + "}"
                    : "{=" + baseId + "}";
                RolledVariants[baseId] = id;
            }

            MBTextManager.SetTextVariable(variableName.ToUpperInvariant(), new TextObject(id + fallbackString));
        }

        public static void ClearDialogVariables()
        {
            RolledVariants.Clear();
        }
    }
}
