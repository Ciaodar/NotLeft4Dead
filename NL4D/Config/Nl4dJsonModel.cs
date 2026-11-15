using System;
using System.Collections.Generic;

namespace NL4D.Config
{
    public class Nl4dJsonModel
    {
        public bool ModEnabled { get; set; } = true;
        public float MinBaseSpawnChance { get; set; } = 0.15f;
        public int GuaranteedCasualtyThreshold { get; set; } = 1500;
        public float SpawnChanceMultiplier { get; set; } = 1.0f;
        public float WinnerSideSurvivorWeight { get; set; } = 0.35f;
        public bool AllowExceedingCompanionLimit { get; set; } = false;
        public int MaxUnrecruitedSurvivorsInWorld { get; set; } = 5;
        public float InitialWoundedHpRatioMin { get; set; } = 0.15f;
        public float InitialWoundedHpRatioMax { get; set; } = 0.20f;
        public int SurvivorRelationBonus { get; set; } = 100;
        public bool UseCustomNamePool { get; set; } = true;
        public bool DebugForceSpawn { get; set; } = false;

        private Dictionary<string, List<string>>? _cultureFirstNames;
        public Dictionary<string, List<string>> CultureFirstNames
        {
            get => _cultureFirstNames ??= CreateDefaultCultureNames();
            set => _cultureFirstNames = value ?? CreateDefaultCultureNames();
        }

        private List<string>? _survivorEpithets;
        public List<string> SurvivorEpithets
        {
            get => _survivorEpithets ??= CreateDefaultEpithets();
            set => _survivorEpithets = value ?? CreateDefaultEpithets();
        }

        public List<string> GetNamesForCulture(string? cultureId)
        {
            var dict = CultureFirstNames;
            if (!string.IsNullOrWhiteSpace(cultureId))
            {
                var lowerCulture = cultureId!.ToLowerInvariant();
                if (dict.TryGetValue(lowerCulture, out var list) && list != null && list.Count > 0)
                {
                    return list;
                }
            }

            if (dict.TryGetValue("default", out var defaultList) && defaultList != null && defaultList.Count > 0)
            {
                return defaultList;
            }

            var fallback = CreateDefaultCultureNames();
            if (!string.IsNullOrWhiteSpace(cultureId) && fallback.TryGetValue(cultureId!.ToLowerInvariant(), out var fbList) && fbList != null && fbList.Count > 0)
            {
                return fbList;
            }

            return fallback["default"];
        }

        public static Dictionary<string, List<string>> CreateDefaultCultureNames()
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["empire"] = new List<string>
                {
                    "Valerios", "Lucia", "Cassian", "Helena", "Tiberios",
                    "Theodora", "Justinian", "Antonia", "Maron", "Phocas",
                    "Zoe", "Constantia", "Demetrios", "Irene", "Leontios", "Sophia"
                },
                ["vlandia"] = new List<string>
                {
                    "Almaric", "Berengar", "Elys", "Godfrey", "Gisela",
                    "Hildegard", "Roland", "Morcar", "Adelaide", "Baldwin",
                    "Yvaine", "Guibert", "Odo", "Mathilda", "Garnier", "Beatrice"
                },
                ["sturgia"] = new List<string>
                {
                    "Branimir", "Militsa", "Viglaf", "Radomir", "Svetlana",
                    "Oleg", "Yaropolk", "Danica", "Borislav", "Rada",
                    "Vsevolod", "Zoritsa", "Kasyan", "Mila", "Dobrynya", "Yadviga"
                },
                ["battania"] = new List<string>
                {
                    "Bran", "Morrigan", "Cormac", "Eileen", "Breccan",
                    "Maeve", "Fingal", "Sorcha", "Cynan", "Rhiannon",
                    "Alpin", "Nesta", "Torcall", "Brid", "Cathal", "Fionnuala"
                },
                ["aserai"] = new List<string>
                {
                    "Tariq", "Yasmin", "Qasim", "Farah", "Nizar",
                    "Safiya", "Hamza", "Zainab", "Ghassan", "Layla",
                    "Bashir", "Samira", "Khalid", "Halima", "Munir", "Asma"
                },
                ["khuzait"] = new List<string>
                {
                    "Barlas", "Altun", "Temur", "Chagatai", "Sorghag",
                    "Kadan", "Yulbars", "Alghu", "Yesui", "Qutlugh",
                    "Batu", "Toghrul", "Arslan", "Sarangerel", "Mengu", "Khulan"
                },
                ["nord"] = new List<string>
                {
                    "Torstein", "Signy", "Halvar", "Aslog", "Eirik",
                    "Gunnvor", "Ragnvald", "Thora", "Brand", "Yngvild",
                    "Snorri", "Kari", "Hafgrim", "Brynhild", "Sigurd", "Gudrun"
                },
                ["default"] = new List<string>
                {
                    "Duncan", "Kendra", "Gareth", "Rowena", "Alden",
                    "Brina", "Jarek", "Vesper", "Torin", "Sylva",
                    "Marek", "Astrid", "Corin", "Lyra", "Eldred", "Morna"
                }
            };
        }

        public static List<string> CreateDefaultEpithets()
        {
            return new List<string>
            {
                "the Forsaken",
                "the Unbroken",
                "the Left-Behind",
                "of the Ashes",
                "Crow-Cheater",
                "Iron-Lung",
                "the Scarred",
                "Gravesworn",
                "Half-Dead",
                "the Unburied",
                "of the Wreckage",
                "Mud-Born",
                "the Deathless",
                "the Survivor",
                "Blood-Drinker",
                "Carrion-Bane"
            };
        }
    }
}