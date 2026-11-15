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

        [SettingPropertyBool("{=nl4d_mcm_mod_enabled}Mod Enabled", Order = 0, RequireRestart = false, HintText = "{=nl4d_mcm_mod_enabled_hint}Master toggle to enable or disable Not Left 4 Dead battle aftermath survivor spawns.")]
        [SettingPropertyGroup("{=nl4d_mcm_group_general}General")]
        public bool ModEnabled { get; set; } = true;

        [SettingPropertyInteger("{=nl4d_mcm_relation_bonus}Survivor Relation Bonus", 0, 100, Order = 1, RequireRestart = false, HintText = "{=nl4d_mcm_relation_bonus_hint}Relation bonus awarded with the survivor upon rescuing and recruiting them (default: 100).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_general}General")]
        public int SurvivorRelationBonus { get; set; } = 100;

        [SettingPropertyBool("{=nl4d_mcm_custom_names}Use Custom Name & Epithet Pool", Order = 2, RequireRestart = false, HintText = "{=nl4d_mcm_custom_names_hint}When enabled, survivors receive culture-appropriate gritty names and survivor epithets (defined in config.json).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_general}General")]
        public bool UseCustomNamePool { get; set; } = true;

        // ──────────────────────────────────────────────
        // Spawn & Battle Size
        // ──────────────────────────────────────────────

        [SettingPropertyFloatingInteger("{=nl4d_mcm_min_spawn_chance}Minimum Base Spawn Chance", 0.0f, 1.0f, "#0%", Order = 0, RequireRestart = false, HintText = "{=nl4d_mcm_min_spawn_chance_hint}Base probability of finding a survivor in the smallest qualifying battle (default: 15%).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_spawn}Spawn & Battle Size")]
        public float MinBaseSpawnChance { get; set; } = 0.15f;

        [SettingPropertyInteger("{=nl4d_mcm_guaranteed_threshold}Guaranteed Casualty Threshold", 100, 5000, Order = 1, RequireRestart = false, HintText = "{=nl4d_mcm_guaranteed_threshold_hint}Total battle casualties (killed + wounded) at which survivor discovery chance reaches 100% (default: 1500).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_spawn}Spawn & Battle Size")]
        public int GuaranteedCasualtyThreshold { get; set; } = 1500;

        [SettingPropertyFloatingInteger("{=nl4d_mcm_spawn_multiplier}Spawn Chance Multiplier", 0.1f, 5.0f, "0.0x", Order = 2, RequireRestart = false, HintText = "{=nl4d_mcm_spawn_multiplier_hint}Overall multiplier applied to survivor spawn probability calculation.")]
        [SettingPropertyGroup("{=nl4d_mcm_group_spawn}Spawn & Battle Size")]
        public float SpawnChanceMultiplier { get; set; } = 1.0f;

        [SettingPropertyFloatingInteger("{=nl4d_mcm_winner_weight}Winning Side Survivor Weight", 0.0f, 1.0f, "#0%", Order = 3, RequireRestart = false, HintText = "{=nl4d_mcm_winner_weight_hint}Probability that a survivor originates from the winning army instead of the defeated side when both sides had Lords (default: 35%).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_spawn}Spawn & Battle Size")]
        public float WinnerSideSurvivorWeight { get; set; } = 0.35f;

        // ──────────────────────────────────────────────
        // Companion & Pool Rules
        // ──────────────────────────────────────────────

        [SettingPropertyBool("{=nl4d_mcm_exceed_limit}Allow Exceeding Companion Limit", Order = 0, RequireRestart = false, HintText = "{=nl4d_mcm_exceed_limit_hint}If enabled, you can recruit battlefield survivors even if your clan has reached its companion capacity.")]
        [SettingPropertyGroup("{=nl4d_mcm_group_companions}Companion & Pool Rules")]
        public bool AllowExceedingCompanionLimit { get; set; } = false;

        [SettingPropertyInteger("{=nl4d_mcm_max_unrecruited}Max Unrecruited Survivors (World FIFO Cap)", 1, 20, Order = 1, RequireRestart = false, HintText = "{=nl4d_mcm_max_unrecruited_hint}Maximum number of unrecruited survivors wandering the realm. Spawning a new survivor past this limit removes the oldest unrecruited survivor (default: 5).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_companions}Companion & Pool Rules")]
        public int MaxUnrecruitedSurvivorsInWorld { get; set; } = 5;

        [SettingPropertyFloatingInteger("{=nl4d_mcm_wounded_min}Initial Wounded HP Ratio Min", 0.05f, 0.50f, "#0%", Order = 2, RequireRestart = false, HintText = "{=nl4d_mcm_wounded_min_hint}Minimum initial health ratio for newly spawned wounded survivors (default: 15%).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_companions}Companion & Pool Rules")]
        public float InitialWoundedHpRatioMin { get; set; } = 0.15f;

        [SettingPropertyFloatingInteger("{=nl4d_mcm_wounded_max}Initial Wounded HP Ratio Max", 0.05f, 0.50f, "#0%", Order = 3, RequireRestart = false, HintText = "{=nl4d_mcm_wounded_max_hint}Maximum initial health ratio for newly spawned wounded survivors (default: 20%).")]
        [SettingPropertyGroup("{=nl4d_mcm_group_companions}Companion & Pool Rules")]
        public float InitialWoundedHpRatioMax { get; set; } = 0.20f;

        // ──────────────────────────────────────────────
        // Debug
        // ──────────────────────────────────────────────

        [SettingPropertyBool("{=nl4d_mcm_debug_force_spawn}Force Spawn (Debug)", Order = 0, RequireRestart = false, HintText = "{=nl4d_mcm_debug_force_spawn_hint}Forces a survivor to spawn after ANY qualifying battle regardless of size or probability.")]
        [SettingPropertyGroup("{=nl4d_mcm_group_debug}Debug")]
        public bool DebugForceSpawn { get; set; } = false;
    }
}