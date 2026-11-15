using System;
using System.Linq;
using NL4D.Calculators;
using NL4D.Config;
using NL4D.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NL4D.Services
{
    /// <summary>
    /// Spawns and configures rescued survivors using the Two-Step Occupation Pattern:
    /// Initialized with custom Occupation.Soldier template (immune to Native TryKillCompanion wanderer purge),
    /// then enriched with Wanderer archetypal attributes, skills, equipment, and switched to Occupation.Wanderer.
    /// </summary>
    public static class SurvivorFactoryService
    {
        public static (Hero Survivor, RescuedSurvivorRecord Record)? CreateSurvivorFromWreckage(WreckageBattleContext context)
        {
            if (context == null || context.Culture == null)
            {
                return null;
            }

            // Step 1: Look up custom Occupation.Soldier template (immune to TryKillCompanion)
            CharacterObject? baseSoldierTemplate = MBObjectManager.Instance.GetObject<CharacterObject>("nl4d_survivor_" + context.Culture.StringId);
            if (baseSoldierTemplate == null)
            {
                baseSoldierTemplate = MBObjectManager.Instance.GetObject<CharacterObject>("nl4d_survivor_empire");
            }

            if (baseSoldierTemplate == null)
            {
                baseSoldierTemplate = CharacterObject.All.FirstOrDefault(c => c.IsTemplate && c.StringId.StartsWith("nl4d_survivor_"));
            }

            if (baseSoldierTemplate == null)
            {
                baseSoldierTemplate = CharacterObject.All.FirstOrDefault(c => c.IsTemplate && c.Culture == context.Culture && c.Occupation != Occupation.Wanderer)
                    ?? CharacterObject.All.FirstOrDefault(c => c.IsTemplate && c.Culture == context.Culture)
                    ?? CharacterObject.All.FirstOrDefault(c => c.IsTemplate);
            }

            if (baseSoldierTemplate == null)
            {
                return null;
            }

            // Step 2: Find matching culture Native Occupation.Wanderer template for archetypal skills/gear
            var wandererCandidates = CharacterObject.All.Where(c => c.IsTemplate && c.Occupation == Occupation.Wanderer && c.Culture == context.Culture).ToList();
            if (wandererCandidates.Count == 0)
            {
                wandererCandidates = CharacterObject.All.Where(c => c.IsTemplate && c.Occupation == Occupation.Wanderer).ToList();
            }

            if (wandererCandidates.Count == 0)
            {
                wandererCandidates = CharacterObject.All.Where(c => c.IsTemplate).ToList();
            }

            if (wandererCandidates.Count == 0)
            {
                return null;
            }

            CharacterObject archetype = wandererCandidates[MBRandom.RandomInt(wandererCandidates.Count)];

            // Step 3: Find nearest settlement of that culture (or nearest town to wreckage) for bornSettlement
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

            // Step 4: Call HeroCreator.CreateSpecialHero with base soldier template
            int age = Campaign.Current.Models.AgeModel.HeroComesOfAge + 6 + MBRandom.RandomInt(14);
            Hero hero = HeroCreator.CreateSpecialHero(baseSoldierTemplate, bornSettlement, null, null, age);
            if (hero == null)
            {
                return null;
            }

            // Step 5: Copy appearance/body properties, Skills, Attributes, Traits, and Equipment from archetype
            hero.StaticBodyProperties = archetype.GetBodyPropertiesMin().StaticProperties;

            // Skills
            if (hero.HeroDeveloper != null)
            {
                foreach (SkillObject skill in Skills.All)
                {
                    int skillVal = archetype.GetSkillValue(skill);
                    if (skillVal > 0)
                    {
                        hero.HeroDeveloper.SetInitialSkillLevel(skill, skillVal);
                    }
                }

                // Attributes (from archetype HeroObject if available, otherwise developer will calculate)
                if (archetype.HeroObject != null)
                {
                    foreach (CharacterAttribute attr in Attributes.All)
                    {
                        int attrVal = archetype.HeroObject.GetAttributeValue(attr);
                        if (attrVal > 0)
                        {
                            hero.HeroDeveloper.AddAttribute(attr, attrVal, checkUnspentPoints: false);
                        }
                    }
                }

                hero.HeroDeveloper.InitializeHeroDeveloper(developCharacterStats: true);
            }

            // Traits
            foreach (TraitObject trait in TraitObject.All)
            {
                int traitVal = archetype.GetTraitLevel(trait);
                if (traitVal != 0)
                {
                    hero.SetTraitLevel(trait, traitVal);
                }
            }

            // Equipment (Battle & Civilian)
            if (archetype.FirstBattleEquipment != null)
            {
                for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumEquipmentSetSlots; i++)
                {
                    hero.BattleEquipment[i] = archetype.FirstBattleEquipment[i];
                }
            }

            if (archetype.FirstCivilianEquipment != null)
            {
                for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumEquipmentSetSlots; i++)
                {
                    hero.CivilianEquipment[i] = archetype.FirstCivilianEquipment[i];
                }
            }

            AdjustEquipmentModifiers(hero.BattleEquipment);
            AdjustEquipmentModifiers(hero.CivilianEquipment);

            // Step 6: Set Occupation to Wanderer and set State to Active
            hero.SetNewOccupation(Occupation.Wanderer);
            hero.ChangeState(Hero.CharacterStates.Active);

            // Step 7: Generate custom name via SurvivorNameCalculator and set on hero
            var (fullName, firstName) = SurvivorNameCalculator.GenerateName(context.Culture, hero.IsFemale);
            hero.SetName(fullName, firstName);

            // Step 8: Set wounded HP ratio
            float hpRatio = MBRandom.RandomFloatRanged(Nl4dSettingsManager.InitialWoundedHpRatioMin, Nl4dSettingsManager.InitialWoundedHpRatioMax);
            hpRatio = Math.Max(0.05f, Math.Min(1.0f, hpRatio));
            hero.HitPoints = Math.Max(1, TaleWorlds.Library.MathF.Round(hero.MaxHitPoints * hpRatio));

            // Step 9: Set personal relation with player
            int relationBonus = Math.Max(-100, Math.Min(100, Nl4dSettingsManager.SurvivorRelationBonus));
            if (Hero.MainHero != null)
            {
                Hero.MainHero.SetPersonalRelation(hero, relationBonus);
            }

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

