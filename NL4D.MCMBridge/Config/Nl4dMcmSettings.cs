using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using TaleWorlds.Localization;

namespace NL4D.MCMBridge.Config
{
    public class Nl4dMcmSettings : AttributeGlobalSettings<Nl4dMcmSettings>
    {
        public override string Id => "NL4DSettings";
        public override string DisplayName => new TextObject("{=nl4d_mcm_display_name}Not Left 4 Dead").ToString();
        public override string FolderName => "NL4D";
        public override string FormatType => "json";

        // ──────────────────────────────────────────────
        // General
        // ──────────────────────────────────────────────

        private bool _modEnabled = true;
        [SettingPropertyBool("{=nl4d_mcm_mod_enabled}Mod Enabled", Order = 0, RequireRestart = false, HintText = "{=nl4d_mcm_mod_enabled_hint}Master toggle to enable or disable Not Left 4 Dead battle aftermath survivor spawns.")]
        [SettingPropertyGroup("{=nl4d_mcm_group_general}General")]
        public bool ModEnabled
        {
            get => _modEnabled;
            set
            {
                if (_modEnabled != value)
                {
                    _modEnabled = value;
                    OnPropertyChanged(nameof(ModEnabled));
                }
            }
        }

        private int _survivorRelationBonus = 100;
        [SettingPropertyInteger("{=nl4d_mcm_relation_bonus}Survivor Relation Bonus", 0, 100, Order = 1, RequireRestart = false, HintText = "{=nl4d_mcm_relation_bonus_hint}Relation bonus awarded with the survivor upon rescuing and recruiting them (default: 100).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_general}General")]
        public int SurvivorRelationBonus
        {
            get => _survivorRelationBonus;
            set
            {
                if (_survivorRelationBonus != value)
                {
                    _survivorRelationBonus = value;
                    OnPropertyChanged(nameof(SurvivorRelationBonus));
                }
            }
        }

        private bool _useCustomNamePool = true;
        [SettingPropertyBool("{=nl4d_mcm_custom_names}Use Custom Name & Epithet Pool", Order = 2, RequireRestart = false, HintText = "{=nl4d_mcm_custom_names_hint}When enabled, survivors receive culture-appropriate gritty names and survivor epithets (defined in config.json).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_general}General")]
        public bool UseCustomNamePool
        {
            get => _useCustomNamePool;
            set
            {
                if (_useCustomNamePool != value)
                {
                    _useCustomNamePool = value;
                    OnPropertyChanged(nameof(UseCustomNamePool));
                }
            }
        }

        // ──────────────────────────────────────────────
        // Spawn & Battle Size
        // ──────────────────────────────────────────────

        private float _minBaseSpawnChance = 0.15f;
        [SettingPropertyFloatingInteger("{=nl4d_mcm_min_spawn_chance}Minimum Base Spawn Chance", 0.0f, 1.0f, "#0%", Order = 0, RequireRestart = false, HintText = "{=nl4d_mcm_min_spawn_chance_hint}Base probability of finding a survivor in the smallest qualifying battle (default: 15%).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_spawn}Spawn & Battle Size")]
        public float MinBaseSpawnChance
        {
            get => _minBaseSpawnChance;
            set
            {
                if (_minBaseSpawnChance != value)
                {
                    _minBaseSpawnChance = value;
                    OnPropertyChanged(nameof(MinBaseSpawnChance));
                }
            }
        }

        private int _guaranteedCasualtyThreshold = 1500;
        [SettingPropertyInteger("{=nl4d_mcm_guaranteed_threshold}Guaranteed Casualty Threshold", 100, 5000, Order = 1, RequireRestart = false, HintText = "{=nl4d_mcm_guaranteed_threshold_hint}Total battle casualties (killed + wounded) at which survivor discovery chance reaches 100% (default: 1500).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_spawn}Spawn & Battle Size")]
        public int GuaranteedCasualtyThreshold
        {
            get => _guaranteedCasualtyThreshold;
            set
            {
                if (_guaranteedCasualtyThreshold != value)
                {
                    _guaranteedCasualtyThreshold = value;
                    OnPropertyChanged(nameof(GuaranteedCasualtyThreshold));
                }
            }
        }

        private float _spawnChanceMultiplier = 1.0f;
        [SettingPropertyFloatingInteger("{=nl4d_mcm_spawn_multiplier}Spawn Chance Multiplier", 0.1f, 5.0f, "0.0x", Order = 2, RequireRestart = false, HintText = "{=nl4d_mcm_spawn_multiplier_hint}Overall multiplier applied to survivor spawn probability calculation.")]
        [SettingPropertyGroup("{=nl4d_mcm_group_spawn}Spawn & Battle Size")]
        public float SpawnChanceMultiplier
        {
            get => _spawnChanceMultiplier;
            set
            {
                if (_spawnChanceMultiplier != value)
                {
                    _spawnChanceMultiplier = value;
                    OnPropertyChanged(nameof(SpawnChanceMultiplier));
                }
            }
        }

        private float _winnerSideSurvivorWeight = 0.35f;
        [SettingPropertyFloatingInteger("{=nl4d_mcm_winner_weight}Winning Side Survivor Weight", 0.0f, 1.0f, "#0%", Order = 3, RequireRestart = false, HintText = "{=nl4d_mcm_winner_weight_hint}Probability that a survivor originates from the winning army instead of the defeated side when both sides had Lords (default: 35%).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_spawn}Spawn & Battle Size")]
        public float WinnerSideSurvivorWeight
        {
            get => _winnerSideSurvivorWeight;
            set
            {
                if (_winnerSideSurvivorWeight != value)
                {
                    _winnerSideSurvivorWeight = value;
                    OnPropertyChanged(nameof(WinnerSideSurvivorWeight));
                }
            }
        }

        // ──────────────────────────────────────────────
        // Companion & Pool Rules
        // ──────────────────────────────────────────────

        private bool _allowExceedingCompanionLimit = false;
        [SettingPropertyBool("{=nl4d_mcm_exceed_limit}Allow Exceeding Companion Limit", Order = 0, RequireRestart = false, HintText = "{=nl4d_mcm_exceed_limit_hint}If enabled, you can recruit battlefield survivors even if your clan has reached its companion capacity.")]
        [SettingPropertyGroup("{=nl4d_mcm_group_companions}Companion & Pool Rules")]
        public bool AllowExceedingCompanionLimit
        {
            get => _allowExceedingCompanionLimit;
            set
            {
                if (_allowExceedingCompanionLimit != value)
                {
                    _allowExceedingCompanionLimit = value;
                    OnPropertyChanged(nameof(AllowExceedingCompanionLimit));
                }
            }
        }

        private int _maxUnrecruitedSurvivorsInWorld = 5;
        [SettingPropertyInteger("{=nl4d_mcm_max_unrecruited}Max Unrecruited Survivors (World FIFO Cap)", 1, 20, Order = 1, RequireRestart = false, HintText = "{=nl4d_mcm_max_unrecruited_hint}Maximum number of unrecruited survivors wandering the realm. Spawning a new survivor past this limit removes the oldest unrecruited survivor (default: 5).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_companions}Companion & Pool Rules")]
        public int MaxUnrecruitedSurvivorsInWorld
        {
            get => _maxUnrecruitedSurvivorsInWorld;
            set
            {
                if (_maxUnrecruitedSurvivorsInWorld != value)
                {
                    _maxUnrecruitedSurvivorsInWorld = value;
                    OnPropertyChanged(nameof(MaxUnrecruitedSurvivorsInWorld));
                }
            }
        }

        private float _initialWoundedHpRatioMin = 0.15f;
        [SettingPropertyFloatingInteger("{=nl4d_mcm_wounded_min}Initial Wounded HP Ratio Min", 0.05f, 0.50f, "#0%", Order = 2, RequireRestart = false, HintText = "{=nl4d_mcm_wounded_min_hint}Minimum initial health ratio for newly spawned wounded survivors (default: 15%).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_companions}Companion & Pool Rules")]
        public float InitialWoundedHpRatioMin
        {
            get => _initialWoundedHpRatioMin;
            set
            {
                if (_initialWoundedHpRatioMin != value)
                {
                    _initialWoundedHpRatioMin = value;
                    OnPropertyChanged(nameof(InitialWoundedHpRatioMin));
                }
            }
        }

        private float _initialWoundedHpRatioMax = 0.20f;
        [SettingPropertyFloatingInteger("{=nl4d_mcm_wounded_max}Initial Wounded HP Ratio Max", 0.05f, 0.50f, "#0%", Order = 3, RequireRestart = false, HintText = "{=nl4d_mcm_wounded_max_hint}Maximum initial health ratio for newly spawned wounded survivors (default: 20%).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_companions}Companion & Pool Rules")]
        public float InitialWoundedHpRatioMax
        {
            get => _initialWoundedHpRatioMax;
            set
            {
                if (_initialWoundedHpRatioMax != value)
                {
                    _initialWoundedHpRatioMax = value;
                    OnPropertyChanged(nameof(InitialWoundedHpRatioMax));
                }
            }
        }

        // ──────────────────────────────────────────────
        // Debug
        // ──────────────────────────────────────────────

        private bool _debugForceSpawn = false;
        [SettingPropertyBool("{=nl4d_mcm_debug_force_spawn}Force Spawn (Debug)", Order = 0, RequireRestart = false, HintText = "{=nl4d_mcm_debug_force_spawn_hint}Forces a survivor to spawn after ANY qualifying battle regardless of size or probability.")]
        [SettingPropertyGroup("{=nl4d_mcm_group_debug}Debug")]
        public bool DebugForceSpawn
        {
            get => _debugForceSpawn;
            set
            {
                if (_debugForceSpawn != value)
                {
                    _debugForceSpawn = value;
                    OnPropertyChanged(nameof(DebugForceSpawn));
                }
            }
        }
    }
}