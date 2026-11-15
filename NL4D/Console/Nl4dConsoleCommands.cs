using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using NL4D.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BattleWreckages;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NL4D.Console
{
    /// <summary>
    /// Developer and QA console commands for Not Left 4 Dead (NL4D).
    /// Registered under the 'nl4d' command namespace.
    /// Note: 'nl4d.reload_config' is handled in Nl4dSettingsManager.
    /// </summary>
    public static class Nl4dConsoleCommands
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("force_next", "nl4d")]
        public static string ForceNextCommand(List<string> args)
        {
            var behavior = WreckageSurvivorBehavior.Instance;
            if (behavior == null)
            {
                return "NL4D: WreckageSurvivorBehavior instance not found (is campaign active?).";
            }

            behavior.ForceNextSpawn();
            return "NL4D: Next wreckage investigation is guaranteed to roll a survivor.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("status", "nl4d")]
        public static string StatusCommand(List<string> args)
        {
            var behavior = WreckageSurvivorBehavior.Instance;
            if (behavior == null)
            {
                return "NL4D: WreckageSurvivorBehavior instance not found (is campaign active?).";
            }

            var records = behavior.GetRescuedSurvivorsSnapshot();
            if (records == null || records.Count == 0)
            {
                return "NL4D: No survivors rescued yet.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"NL4D Rescued Survivors Status (Total: {records.Count}):");

            for (int i = 0; i < records.Count; i++)
            {
                var r = records[i];
                if (r == null || r.SurvivorHero == null) continue;

                var h = r.SurvivorHero;
                string settlementName = h.CurrentSettlement?.Name?.ToString() ?? "Roaming / In Party";
                int relation = Hero.MainHero != null ? (int)Hero.MainHero.GetRelation(h) : 0;
                string statusStr = r.IsRecruited ? "Companion (Recruited)" : $"Unrecruited in {settlementName}";

                sb.AppendLine($"[{i + 1}] {h.Name} | {statusStr} | HP: {h.HitPoints}/{h.MaxHitPoints} | Rel: {relation} | Former: {r.FormerLeaderName} ({r.FormerFactionName})");
            }

            return sb.ToString();
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("spawn_test_wreckage", "nl4d")]
        public static string SpawnTestWreckageCommand(List<string> args)
        {
            if (Campaign.Current == null || MobileParty.MainParty == null)
            {
                return "NL4D: Campaign or MainParty is not active.";
            }

            // 1. Pick two alive kingdom leaders or lords other than Hero.MainHero
            var eligibleLords = Hero.AllAliveHeroes
                .Where(h => h != Hero.MainHero && h.IsLord && h.MapFaction != null && h.MapFaction.IsKingdomFaction)
                .ToList();

            if (eligibleLords.Count < 2)
            {
                eligibleLords = Hero.AllAliveHeroes
                    .Where(h => h != Hero.MainHero && h.IsLord)
                    .ToList();
            }

            if (eligibleLords.Count < 2)
            {
                return "NL4D: Could not find at least two eligible NPC lords to stage battle wreckage.";
            }

            Hero attackerLord = eligibleLords.FirstOrDefault(h => h.IsFactionLeader) ?? eligibleLords[0];
            Hero defenderLord = eligibleLords.FirstOrDefault(h => h != attackerLord && h.MapFaction != attackerLord.MapFaction)
                ?? eligibleLords.FirstOrDefault(h => h != attackerLord)
                ?? eligibleLords[1];

            // 2. Position near player party (slightly offset on land)
            CampaignVec2 playerPos = MobileParty.MainParty.Position;
            CampaignVec2 spawnPos = playerPos + new Vec2(0.35f, 0.35f);

            // 3. Instantiate BattleWreckage using FormatterServices
            BattleWreckage wreckage = (BattleWreckage)FormatterServices.GetUninitializedObject(typeof(BattleWreckage));

            string stringId = Campaign.Current.CampaignObjectManager.FindNextUniqueStringId<BattleWreckage>("wreckage_1");
            CampaignTime destroyTime = CampaignTime.DaysFromNow(14f);
            CampaignTime battleStart = CampaignTime.HoursFromNow(-8f);

            TextObject attackerPartyName = attackerLord.PartyBelongedTo?.Name ?? attackerLord.Name;
            TextObject defenderPartyName = defenderLord.PartyBelongedTo?.Name ?? defenderLord.Name;

            // Build troop rosters with casualties (total 1600 casualties)
            TroopRoster attackerDied = TroopRoster.CreateDummyTroopRoster();
            TroopRoster defenderDied = TroopRoster.CreateDummyTroopRoster();
            TroopRoster attackerWounded = TroopRoster.CreateDummyTroopRoster();
            TroopRoster defenderWounded = TroopRoster.CreateDummyTroopRoster();

            CharacterObject? attackerBasicTroop = attackerLord.Culture?.BasicTroop ?? CharacterObject.PlayerCharacter;
            CharacterObject? defenderBasicTroop = defenderLord.Culture?.BasicTroop ?? CharacterObject.PlayerCharacter;

            if (attackerBasicTroop != null)
            {
                attackerDied.AddToCounts(attackerBasicTroop, 550);
                attackerWounded.AddToCounts(attackerBasicTroop, 300);
            }

            if (defenderBasicTroop != null)
            {
                defenderDied.AddToCounts(defenderBasicTroop, 500);
                defenderWounded.AddToCounts(defenderBasicTroop, 250);
            }

            // Set properties and fields via reflection
            wreckage.StringId = stringId;
            SetField(wreckage, "Position", spawnPos);
            SetField(wreckage, "DestroyTime", destroyTime);
            SetField(wreckage, "WreckageTypeCategory", BattleWreckage.WreckageType.Epic);
            SetField(wreckage, "AttackerLeaderHero", attackerLord);
            SetField(wreckage, "DefenderLeaderHero", defenderLord);
            SetField(wreckage, "AttackerLeaderPartyName", attackerPartyName);
            SetField(wreckage, "DefenderLeaderPartyName", defenderPartyName);
            SetField(wreckage, "AttackerFaction", attackerLord.MapFaction);
            SetField(wreckage, "DefenderFaction", defenderLord.MapFaction);
            SetField(wreckage, "WinnerSide", BattleSideEnum.Attacker);
            SetField(wreckage, "BattleStartTime", battleStart);
            SetField(wreckage, "AttackerHealthyTroopCountAtStart", 1200);
            SetField(wreckage, "DefenderHealthyTroopCountAtStart", 900);
            SetField(wreckage, "AttackerDiedInBattle", attackerDied);
            SetField(wreckage, "DefenderDiedInBattle", defenderDied);
            SetField(wreckage, "AttackerWoundedInBattle", attackerWounded);
            SetField(wreckage, "DefenderWoundedInBattle", defenderWounded);

            var nameObj = new TextObject("{=nl4d_test_wreckage_name}Battle Aftermath ({ATTACKER} vs {DEFENDER})");
            nameObj.SetTextVariable("ATTACKER", attackerLord.Name);
            nameObj.SetTextVariable("DEFENDER", defenderLord.Name);
            SetProperty(wreckage, "Name", nameObj);

            // 4. Add to CampaignObjectManager via internal AddWreckage method
            var addMethod = typeof(CampaignObjectManager).GetMethod("AddWreckage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            addMethod?.Invoke(Campaign.Current.CampaignObjectManager, new object[] { wreckage });

            // 5. Notify CampaignEventDispatcher to create visual map entity
            CampaignEventDispatcher.Instance.OnMapInteractableCreated(wreckage);
            wreckage.UpdateVisibility();

            // 6. Force next wreckage spawn in NL4D
            WreckageSurvivorBehavior.Instance?.ForceNextSpawn();

            return $"NL4D: Spawned test Epic BattleWreckage '{stringId}' at ({spawnPos.X:F1}, {spawnPos.Y:F1}) between {attackerLord.Name} and {defenderLord.Name} with 1,600 casualties! ForceNextSpawn is enabled.";
        }

        private static void SetField(object obj, string fieldName, object value)
        {
            Type? type = obj.GetType();
            FieldInfo? field = null;
            while (type != null && field == null)
            {
                field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                type = type.BaseType;
            }
            field?.SetValue(obj, value);
        }

        private static void SetProperty(object obj, string propName, object value)
        {
            Type? type = obj.GetType();
            PropertyInfo? prop = null;
            while (type != null && prop == null)
            {
                prop = type.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                type = type.BaseType;
            }
            prop?.SetValue(obj, value, null);
        }
    }
}
