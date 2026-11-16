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

        private Dictionary<string, List<string>>? _cultureFemaleFirstNames;
        public Dictionary<string, List<string>> CultureFemaleFirstNames
        {
            get => _cultureFemaleFirstNames ??= CreateDefaultCultureFemaleNames();
            set => _cultureFemaleFirstNames = value ?? CreateDefaultCultureFemaleNames();
        }

        private List<string>? _survivorEpithets;
        public List<string> SurvivorEpithets
        {
            get => _survivorEpithets ??= CreateDefaultEpithets();
            set => _survivorEpithets = value ?? CreateDefaultEpithets();
        }

        public List<string> GetNamesForCulture(string? cultureId)
        {
            return GetNamesForCulture(cultureId, false);
        }

        public List<string> GetNamesForCulture(string? cultureId, bool isFemale)
        {
            var dict = isFemale ? CultureFemaleFirstNames : CultureFirstNames;
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

            var fallback = isFemale ? CreateDefaultCultureFemaleNames() : CreateDefaultCultureNames();
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
                    "Valerios", "Cassian", "Tiberios", "Justinian", "Maron",
                    "Phocas", "Demetrios", "Leontios", "Alexios", "Konstantinos",
                    "Niketas", "Basileios", "Menander", "Theophilos"
                },
                ["vlandia"] = new List<string>
                {
                    "Almaric", "Berengar", "Godfrey", "Roland", "Morcar",
                    "Baldwin", "Guibert", "Odo", "Garnier", "Arnaud",
                    "Bertrand", "Amaury", "Geoffrey", "Hervieu"
                },
                ["sturgia"] = new List<string>
                {
                    "Branimir", "Viglaf", "Radomir", "Oleg", "Yaropolk",
                    "Borislav", "Vsevolod", "Kasyan", "Dobrynya", "Svyatoslav",
                    "Mstislav", "Rurik", "Stanislav", "Volodar"
                },
                ["battania"] = new List<string>
                {
                    "Bran", "Cormac", "Breccan", "Fingal", "Cynan",
                    "Alpin", "Torcall", "Cathal", "Morid", "Domhnall",
                    "Calgacus", "Kaelen", "Fergus", "Eogan"
                },
                ["aserai"] = new List<string>
                {
                    "Tariq", "Qasim", "Nizar", "Hamza", "Ghassan",
                    "Bashir", "Khalid", "Munir", "Suleiman", "Jafar",
                    "Rashid", "Fahd", "Karim", "Ziyad"
                },
                ["khuzait"] = new List<string>
                {
                    "Barlas", "Temur", "Chagatai", "Kadan", "Yulbars",
                    "Alghu", "Qutlugh", "Batu", "Toghrul", "Arslan",
                    "Mengu", "Khorchi", "Subutai", "Jochi"
                },
                ["nord"] = new List<string>
                {
                    "Torstein", "Halvar", "Eirik", "Ragnvald", "Brand",
                    "Snorri", "Hafgrim", "Sigurd", "Hakon", "Gunnar",
                    "Olaf", "Leif", "Bjorn", "Sten"
                },
                ["default"] = new List<string>
                {
                    "Duncan", "Gareth", "Alden", "Jarek", "Torin",
                    "Marek", "Corin", "Eldred", "Bram", "Garrick",
                    "Lachlan", "Lucan", "Gavin", "Rowan"
                }
            };
        }

        public static Dictionary<string, List<string>> CreateDefaultCultureFemaleNames()
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["empire"] = new List<string>
                {
                    "Lucia", "Helena", "Theodora", "Antonia", "Zoe",
                    "Constantia", "Irene", "Sophia", "Anna", "Eudokia",
                    "Pulcheria", "Agatha", "Theophano", "Kassiani"
                },
                ["vlandia"] = new List<string>
                {
                    "Elys", "Gisela", "Hildegard", "Adelaide", "Yvaine",
                    "Mathilda", "Beatrice", "Alienor", "Blanche", "Isolda",
                    "Gwenllian", "Margot", "Sibylla", "Eleanor"
                },
                ["sturgia"] = new List<string>
                {
                    "Militsa", "Svetlana", "Danica", "Rada", "Zoritsa",
                    "Mila", "Yadviga", "Olga", "Lyudmila", "Lada",
                    "Vesna", "Yaroslava", "Bogdana", "Daryna"
                },
                ["battania"] = new List<string>
                {
                    "Morrigan", "Eileen", "Maeve", "Sorcha", "Rhiannon",
                    "Nesta", "Brid", "Fionnuala", "Gweneth", "Kiera",
                    "Aoife", "Bronwen", "Deirdre", "Siobhan"
                },
                ["aserai"] = new List<string>
                {
                    "Yasmin", "Farah", "Safiya", "Zainab", "Layla",
                    "Samira", "Halima", "Asma", "Rania", "Salma",
                    "Fatima", "Noor", "Amina", "Huda"
                },
                ["khuzait"] = new List<string>
                {
                    "Altun", "Sorghag", "Yesui", "Sarangerel", "Khulan",
                    "Chabi", "Borte", "Qutlun", "Toghon", "Temulun",
                    "Altan", "Ebegei", "Yisun", "Gurbesu"
                },
                ["nord"] = new List<string>
                {
                    "Signy", "Aslog", "Gunnvor", "Thora", "Yngvild",
                    "Kari", "Brynhild", "Gudrun", "Astrid", "Freydis",
                    "Ingrid", "Helga", "Sigrid", "Rannveig"
                },
                ["default"] = new List<string>
                {
                    "Kendra", "Rowena", "Brina", "Vesper", "Sylva",
                    "Astrid", "Lyra", "Morna", "Elspeth", "Linnea",
                    "Carys", "Seraphina", "Isla", "Faye"
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