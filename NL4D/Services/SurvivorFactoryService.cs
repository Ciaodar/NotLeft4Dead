using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NL4D.Calculators;
using NL4D.Config;
using NL4D.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NL4D.Services
{
    /// <summary>
    /// Spawns and configures rescued survivors directly from Native Wanderer archetypes.
    /// Preserves natural facial features, hair, beards, and correct gender mesh/proportions,
    /// while detaching survivors from Native CompanionsCampaignBehavior.TryKillCompanion.
    /// </summary>
    public static class SurvivorFactoryService
    {
        public static (Hero Survivor, RescuedSurvivorRecord Record)? CreateSurvivorFromWreckage(WreckageBattleContext context)
        {
            if (context == null || context.Culture == null)
            {
                return null;
            }

            // Step 1: Query Native Occupation.Wanderer archetype templates matching context culture
            var wandererCandidates = CharacterObject.All
                .Where(c => c.IsTemplate && c.Occupation == Occupation.Wanderer && c.Culture == context.Culture)
                .ToList();

            if (wandererCandidates.Count == 0)
            {
                wandererCandidates = CharacterObject.All
                    .Where(c => c.IsTemplate && c.Occupation == Occupation.Wanderer)
                    .ToList();
            }

            if (wandererCandidates.Count == 0)
            {
                wandererCandidates = CharacterObject.All.Where(c => c.IsTemplate).ToList();
            }

            if (wandererCandidates.Count == 0)
            {
                return null;
            }

            // Prefer an archetype template not currently active among alive wanderers in Calradia
            HashSet<CharacterObject> usedTemplates;
            try
            {
                usedTemplates = new HashSet<CharacterObject>(
                    Hero.AllAliveHeroes.Where(h => h.IsWanderer && h.Template != null).Select(h => h.Template)
                );
            }
            catch
            {
                usedTemplates = new HashSet<CharacterObject>();
            }

            var unusedCandidates = wandererCandidates.Where(c => !usedTemplates.Contains(c)).ToList();
            CharacterObject archetype = unusedCandidates.Count > 0
                ? unusedCandidates[MBRandom.RandomInt(unusedCandidates.Count)]
                : wandererCandidates[MBRandom.RandomInt(wandererCandidates.Count)];

            // Step 2: Find nearest settlement of that culture (or nearest town to wreckage) for bornSettlement
            Settlement? bornSettlement = Settlement.All
                .Where(s => s.IsTown && s.Culture == context.Culture)
                .OrderBy(s => context.Position.Distance(s.GatePosition))
                .FirstOrDefault();

            if (bornSettlement == null)
            {
                bornSettlement = Settlement.All
                    .Where(s => s.IsTown)
                    .OrderBy(s => context.Position.Distance(s.GatePosition))
                    .FirstOrDefault();
            }

            if (bornSettlement == null)
            {
                bornSettlement = Settlement.All.FirstOrDefault();
            }

            // Step 3: Spawn hero passing archetype directly into HeroCreator.CreateSpecialHero.
            // Native automatically generates natural, varied body properties (hair, beard, skin, face tags),
            // matching 3D gender mesh, and proper skills, traits, and equipment directly from archetype.
            int age = Campaign.Current.Models.AgeModel.HeroComesOfAge + 6 + MBRandom.RandomInt(14);
            Hero hero = HeroCreator.CreateSpecialHero(archetype, bornSettlement, null, null, age);
            if (hero == null)
            {
                return null;
            }

            // Step 4: Apply companion equipment modifiers (companion_armor, companion_weapon, companion_horse)
            AdjustEquipmentModifiers(hero.BattleEquipment);
            AdjustEquipmentModifiers(hero.CivilianEquipment);

            // Step 5: Ensure hero state is Active and Occupation is Wanderer
            hero.SetNewOccupation(Occupation.Wanderer);
            hero.ChangeState(Hero.CharacterStates.Active);

            // Step 6: Generate custom name matching hero's generated gender (male or female)
            var (fullName, firstName) = SurvivorNameCalculator.GenerateName(context.Culture, hero.IsFemale);
            hero.SetName(fullName, firstName);

            // Step 7: Set wounded HP ratio
            float hpRatio = MBRandom.RandomFloatRanged(Nl4dSettingsManager.InitialWoundedHpRatioMin, Nl4dSettingsManager.InitialWoundedHpRatioMax);
            hpRatio = Math.Max(0.05f, Math.Min(1.0f, hpRatio));
            hero.HitPoints = Math.Max(1, TaleWorlds.Library.MathF.Round(hero.MaxHitPoints * hpRatio));

            // Step 8: Set personal relation with player
            int relationBonus = Math.Max(-100, Math.Min(100, Nl4dSettingsManager.SurvivorRelationBonus));
            if (Hero.MainHero != null)
            {
                Hero.MainHero.SetPersonalRelation(hero, relationBonus);
            }

            // Step 9: Detach survivor from Native CompanionsCampaignBehavior.TryKillCompanion
            DetachFromNativeCompanionKiller(hero, archetype);

            // Step 10: Build and return record
            string formerLeaderName = context.FormerLeaderHero?.Name?.ToString()
                ?? (!string.IsNullOrWhiteSpace(context.FormerPartyName) ? context.FormerPartyName : "Former Commander");

            string formerFactionName = context.FormerFaction?.Name?.ToString()
                ?? string.Empty;

            string enemyFactionName = context.EnemyFaction?.Name?.ToString()
                ?? (!string.IsNullOrWhiteSpace(context.EnemyPartyName) ? context.EnemyPartyName : string.Empty);

            var record = new RescuedSurvivorRecord(
                hero,
                formerLeaderName,
                formerFactionName,
                enemyFactionName,
                context.WasFromWinningSide,
                CampaignTime.Now,
                isRecruited: false
            );

            return (hero, record);
        }

        /// <summary>
        /// Protects the created survivor from Native CompanionsCampaignBehavior.TryKillCompanion
        /// by removing the template from _aliveCompanionTemplates if no other active unrecruited wanderer uses it.
        /// </summary>
        public static void DetachFromNativeCompanionKiller(Hero hero, CharacterObject archetype)
        {
            try
            {
                var compBehavior = Campaign.Current?.GetCampaignBehavior<CompanionsCampaignBehavior>();
                if (compBehavior != null)
                {
                    var field = typeof(CompanionsCampaignBehavior).GetField("_aliveCompanionTemplates", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field?.GetValue(compBehavior) is HashSet<CharacterObject> templates)
                    {
                        bool otherUses = Hero.AllAliveHeroes.Any(h => h != hero && h.IsWanderer && h.CompanionOf == null && h.Template == archetype);
                        if (!otherUses)
                        {
                            templates.Remove(archetype);
                        }
                    }
                }
            }
            catch
            {
                // Non-intrusive failover
            }
        }

        /// <summary>
        /// Batch-protects all tracked unrecruited survivors against Native purge logic.
        /// Can be called on game loaded and session launched events.
        /// </summary>
        public static void ProtectAllUnrecruitedSurvivors(IEnumerable<Hero>? unrecruitedHeroes)
        {
            if (unrecruitedHeroes == null)
            {
                return;
            }

            try
            {
                var compBehavior = Campaign.Current?.GetCampaignBehavior<CompanionsCampaignBehavior>();
                if (compBehavior != null)
                {
                    var field = typeof(CompanionsCampaignBehavior).GetField("_aliveCompanionTemplates", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field?.GetValue(compBehavior) is HashSet<CharacterObject> templates)
                    {
                        foreach (var hero in unrecruitedHeroes)
                        {
                            if (hero != null && hero.IsAlive && hero.Template != null)
                            {
                                bool otherUses = Hero.AllAliveHeroes.Any(h => h != hero && h.IsWanderer && h.CompanionOf == null && h.Template == hero.Template);
                                if (!otherUses)
                                {
                                    templates.Remove(hero.Template);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Non-intrusive failover
            }
        }

        private static void AdjustEquipmentModifiers(Equipment? equipment)
        {
            if (equipment == null)
            {
                return;
            }

            ItemModifier? armorMod = MBObjectManager.Instance.GetObject<ItemModifier>("companion_armor");
            ItemModifier? weaponMod = MBObjectManager.Instance.GetObject<ItemModifier>("companion_weapon");
            ItemModifier? horseMod = MBObjectManager.Instance.GetObject<ItemModifier>("companion_horse");

            for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumEquipmentSetSlots; i++)
            {
                EquipmentElement el = equipment[i];
                if (el.Item != null)
                {
                    if (el.Item.ArmorComponent != null && armorMod != null)
                    {
                        equipment[i] = new EquipmentElement(el.Item, armorMod);
                    }
                    else if (el.Item.HorseComponent != null && horseMod != null)
                    {
                        equipment[i] = new EquipmentElement(el.Item, horseMod);
                    }
                    else if (el.Item.WeaponComponent != null && weaponMod != null)
                    {
                        equipment[i] = new EquipmentElement(el.Item, weaponMod);
                    }
                }
            }
        }
    }
}
