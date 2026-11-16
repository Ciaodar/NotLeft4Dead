using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TaleWorlds.Library;

namespace NL4D.Config
{
    public static class Nl4dSettingsManager
    {
        private static readonly object SyncRoot = new object();
        private static Nl4dJsonModel _settings = new Nl4dJsonModel();
        private static Func<Nl4dJsonModel>? _externalSettingsProvider;
        private const string ConfigFileName = "config.json";

        public static event Action? OnClearDataRequested;

        public static bool ModEnabled => SettingsOrDefault().ModEnabled;
        public static float MinBaseSpawnChance => SettingsOrDefault().MinBaseSpawnChance;
        public static int GuaranteedCasualtyThreshold => SettingsOrDefault().GuaranteedCasualtyThreshold;
        public static float SpawnChanceMultiplier => SettingsOrDefault().SpawnChanceMultiplier;
        public static float WinnerSideSurvivorWeight => SettingsOrDefault().WinnerSideSurvivorWeight;
        public static bool AllowExceedingCompanionLimit => SettingsOrDefault().AllowExceedingCompanionLimit;
        public static int MaxUnrecruitedSurvivorsInWorld => SettingsOrDefault().MaxUnrecruitedSurvivorsInWorld;
        public static float InitialWoundedHpRatioMin => SettingsOrDefault().InitialWoundedHpRatioMin;
        public static float InitialWoundedHpRatioMax => SettingsOrDefault().InitialWoundedHpRatioMax;
        public static int SurvivorRelationBonus => SettingsOrDefault().SurvivorRelationBonus;
        public static bool UseCustomNamePool => SettingsOrDefault().UseCustomNamePool;
        public static bool DebugForceSpawn => SettingsOrDefault().DebugForceSpawn;
        public static Dictionary<string, List<string>> CultureFirstNames => SettingsOrDefault().CultureFirstNames;
        public static Dictionary<string, List<string>> CultureFemaleFirstNames => SettingsOrDefault().CultureFemaleFirstNames;
        public static List<string> SurvivorEpithets => SettingsOrDefault().SurvivorEpithets;

        public static List<string> GetNamesForCulture(string? cultureId) => SettingsOrDefault().GetNamesForCulture(cultureId);
        public static List<string> GetNamesForCulture(string? cultureId, bool isFemale) => SettingsOrDefault().GetNamesForCulture(cultureId, isFemale);

        public static void Initialize()
        {
            Reload();
        }

        public static void TriggerClearDataEvent()
        {
            OnClearDataRequested?.Invoke();

            try
            {
                InformationManager.DisplayMessage(
                    new InformationMessage("NL4D Data Cleared!", Colors.Green));
            }
            catch
            {
                // UI or InformationManager not initialized
            }
        }

        public static void RegisterExternalSettingsProvider(Func<Nl4dJsonModel> provider)
        {
            lock (SyncRoot)
            {
                _externalSettingsProvider = provider;
                Reload();
            }
        }

        public static bool Reload()
        {
            lock (SyncRoot)
            {
                var defaults = new Nl4dJsonModel();
                var configPath = GetConfigPath();
                Nl4dJsonModel diskModel = defaults;

                try
                {
                    if (!File.Exists(configPath))
                    {
                        SaveInternal(configPath, defaults);
                        diskModel = defaults;
                    }
                    else
                    {
                        var json = File.ReadAllText(configPath);
                        var loaded = JsonConvert.DeserializeObject<Nl4dJsonModel>(json);
                        diskModel = loaded ?? defaults;
                    }
                }
                catch
                {
                    diskModel = defaults;
                    try
                    {
                        SaveInternal(configPath, diskModel);
                    }
                    catch
                    {
                        // In-memory fallback
                    }
                }

                if (_externalSettingsProvider != null)
                {
                    try
                    {
                        Nl4dJsonModel providerModel = _externalSettingsProvider();
                        if (providerModel != null)
                        {
                            // Preserve JSON file name pools and epithets since MCM does not configure them
                            if (providerModel.CultureFirstNames == null || providerModel.CultureFirstNames.Count == 0)
                            {
                                providerModel.CultureFirstNames = diskModel.CultureFirstNames;
                            }
                            if (providerModel.CultureFemaleFirstNames == null || providerModel.CultureFemaleFirstNames.Count == 0)
                            {
                                providerModel.CultureFemaleFirstNames = diskModel.CultureFemaleFirstNames;
                            }
                            if (providerModel.SurvivorEpithets == null || providerModel.SurvivorEpithets.Count == 0)
                            {
                                providerModel.SurvivorEpithets = diskModel.SurvivorEpithets;
                            }

                            _settings = providerModel;
                            return true;
                        }
                    }
                    catch
                    {
                        // Bridge provider failed; continue with disk JSON model.
                    }
                }

                _settings = diskModel;
                return true;
            }
        }

        private static Nl4dJsonModel SettingsOrDefault()
        {
            if (_externalSettingsProvider != null)
            {
                try
                {
                    var providerModel = _externalSettingsProvider();
                    if (providerModel != null)
                    {
                        if (providerModel.CultureFirstNames == null || providerModel.CultureFirstNames.Count == 0)
                        {
                            providerModel.CultureFirstNames = _settings.CultureFirstNames;
                        }
                        if (providerModel.CultureFemaleFirstNames == null || providerModel.CultureFemaleFirstNames.Count == 0)
                        {
                            providerModel.CultureFemaleFirstNames = _settings.CultureFemaleFirstNames;
                        }
                        if (providerModel.SurvivorEpithets == null || providerModel.SurvivorEpithets.Count == 0)
                        {
                            providerModel.SurvivorEpithets = _settings.SurvivorEpithets;
                        }
                        return providerModel;
                    }
                }
                catch
                {
                    // Fallback to cached settings if provider fails
                }
            }

            return _settings;
        }

        private static string GetConfigPath()
        {
            var gameRoot = BasePath.Name;
            if (!string.IsNullOrWhiteSpace(gameRoot))
            {
                var moduleConfig = Path.Combine(gameRoot, "Modules", "NL4D", ConfigFileName);
                if (File.Exists(moduleConfig))
                    return moduleConfig;

                try
                {
                    var moduleDir = Path.GetDirectoryName(moduleConfig);
                    if (!string.IsNullOrWhiteSpace(moduleDir))
                    {
                        Directory.CreateDirectory(moduleDir);
                        return moduleConfig;
                    }
                }
                catch
                {
                    // Fall back to documents path.
                }
            }

            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fallbackDir = Path.Combine(documents, "Mount and Blade II Bannerlord", "Configs", "NL4D");
            Directory.CreateDirectory(fallbackDir);
            return Path.Combine(fallbackDir, ConfigFileName);
        }

        private static void SaveInternal(string path, Nl4dJsonModel model)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var json = JsonConvert.SerializeObject(model, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("reload_config", "nl4d")]
        public static string ReloadConfigCommand(List<string> args)
        {
            bool ok = Reload();
            return ok ? "NL4D config reloaded." : "NL4D config reload failed, defaults applied.";
        }
    }
}