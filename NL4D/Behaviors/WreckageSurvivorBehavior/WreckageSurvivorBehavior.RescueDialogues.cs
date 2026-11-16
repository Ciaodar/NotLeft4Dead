using System.Linq;
using NL4D.Calculators;
using NL4D.Config;
using NL4D.Data;
using NL4D.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NL4D.Behaviors
{
    /// <summary>
    /// Rescue dialogue partial class for WreckageSurvivorBehavior.
    /// Handles initial encounter dialogue tree, interactive lore hub, survivor recruitment, or town escort routing.
    /// </summary>
    public sealed partial class WreckageSurvivorBehavior
    {
        private void AddRescueDialogues(CampaignGameStarter starter)
        {
            // NPC initial opening line from the debris -> opens interactive lore hub
            starter.AddDialogLine(
                "nl4d_rescue_intro_line",
                "start",
                "nl4d_rescue_hub_token",
                "{=!}{NL4D_RESCUE_INTRO}",
                RescueIntroCondition,
                null,
                125
            );

            // --- Interactive Lore Hub Options ---

            // Question 1: What happened in the battle?
            starter.AddPlayerLine(
                "nl4d_rescue_ask_battle",
                "nl4d_rescue_hub_token",
                "nl4d_rescue_battle_lore_reply_token",
                "{=nl4d_rescue_q_battle}What actually happened here? Tell me of the battle.",
                () => !_askedBattleLore,
                OnAskBattleLoreConsequence,
                100
            );

            // NPC Reply for Question 1 -> returns to hub
            starter.AddDialogLine(
                "nl4d_rescue_battle_lore_reply",
                "nl4d_rescue_battle_lore_reply_token",
                "nl4d_rescue_hub_token",
                "{=!}{NL4D_RESCUE_BATTLE_LORE}",
                null,
                null,
                100
            );

            // Question 2: Who are you? (Identity & Background)
            starter.AddPlayerLine(
                "nl4d_rescue_ask_identity",
                "nl4d_rescue_hub_token",
                "nl4d_rescue_identity_lore_reply_token",
                "{=nl4d_rescue_q_identity}Who are you really? What is your story?",
                () => !_askedIdentity,
                OnAskIdentityLoreConsequence,
                100
            );

            // NPC Reply for Question 2 -> returns to hub
            starter.AddDialogLine(
                "nl4d_rescue_identity_lore_reply",
                "nl4d_rescue_identity_lore_reply_token",
                "nl4d_rescue_hub_token",
                "{=!}{NL4D_RESCUE_IDENTITY_LORE}",
                null,
                null,
                100
            );

            // Question 3: How bad are your wounds?
            starter.AddPlayerLine(
                "nl4d_rescue_ask_wounds",
                "nl4d_rescue_hub_token",
                "nl4d_rescue_wounds_lore_reply_token",
                "{=nl4d_rescue_q_wounds}What happened to you? How bad are your wounds?",
                () => !_askedWounds,
                OnAskWoundsLoreConsequence,
                100
            );

            // NPC Reply for Question 3 -> leads to plea
            starter.AddDialogLine(
                "nl4d_rescue_wounds_lore_reply",
                "nl4d_rescue_wounds_lore_reply_token",
                "nl4d_rescue_plea_token",
                "{=!}{NL4D_RESCUE_WOUNDS_LORE}",
                null,
                null,
                100
            );

            // NPC follow-up plea line -> returns to hub
            starter.AddDialogLine(
                "nl4d_rescue_plea_line",
                "nl4d_rescue_plea_token",
                "nl4d_rescue_hub_token",
                "{=!}{NL4D_RESCUE_PLEA}",
                null,
                null,
                100
            );

            // --- Terminal Decisions ---

            // Decision 1: Recruit survivor into party for free
            starter.AddPlayerLine(
                "nl4d_rescue_option_recruit",
                "nl4d_rescue_hub_token",
                "nl4d_rescue_recruit_reply_token",
                "{=nl4d_rescue_accept}Rise, friend. Anyone who survived that carnage belongs by my side. Welcome to my company.",
                null,
                OnRescueRecruitConsequence,
                100,
                OnRescueRecruitClickableCondition
            );

            // NPC grateful response to recruitment -> closes window
            starter.AddDialogLine(
                "nl4d_rescue_recruit_reply_line",
                "nl4d_rescue_recruit_reply_token",
                "close_window",
                "{=nl4d_rescue_accepted_reply}By the heavens, you will not regret this! I will fight for you until my last breath.",
                null,
                null,
                100
            );

            // Decision 2: Decline recruitment, send survivor safely to town
            starter.AddPlayerLine(
                "nl4d_rescue_option_decline",
                "nl4d_rescue_hub_token",
                "nl4d_rescue_decline_reply_token",
                "{=nl4d_rescue_decline}I cannot take you into my ranks right now, but I will see you safely guided to the nearest town. Rest and recover.",
                null,
                OnRescueDeclineConsequence,
                90
            );

            // NPC response to decline -> closes window
            starter.AddDialogLine(
                "nl4d_rescue_decline_reply_line",
                "nl4d_rescue_decline_reply_token",
                "close_window",
                "{=nl4d_rescue_declined_reply}Thank you for your mercy... I will find shelter in {TARGET_TOWN_NAME}. If you ever need a loyal sword, look for me in the tavern.",
                null,
                null,
                100
            );
        }

        private bool RescueIntroCondition()
        {
            if (!_isInitialRescueConversation)
            {
                return false;
            }

            Hero? currentHero = Hero.OneToOneConversationHero;
            if (currentHero == null || currentHero != _pendingRescueHero)
            {
                return false;
            }

            if (_pendingRescueContext == null)
            {
                return false;
            }

            currentHero.SetHasMet();
            SetupRescueDialogVariables();
            return true;
        }

        private void SetupRescueDialogVariables()
        {
            if (_pendingRescueContext == null)
            {
                return;
            }

            TextObject lordLink = _pendingRescueContext.FormerLeaderHero?.EncyclopediaLinkWithName
                ?? (!string.IsNullOrWhiteSpace(_pendingRescueContext.FormerPartyName)
                    ? new TextObject(_pendingRescueContext.FormerPartyName)
                    : new TextObject("my commander"));

            TextObject formerFactionLink = _pendingRescueContext.FormerFaction?.EncyclopediaLinkWithName
                ?? lordLink;

            TextObject enemyFactionLink = _pendingRescueContext.EnemyFaction?.EncyclopediaLinkWithName
                ?? (!string.IsNullOrWhiteSpace(_pendingRescueContext.EnemyPartyName)
                    ? new TextObject(_pendingRescueContext.EnemyPartyName)
                    : new TextObject("the enemy"));

            TextObject enemyLordLink = _pendingRescueContext.EnemyLeaderHero?.EncyclopediaLinkWithName
                ?? (!string.IsNullOrWhiteSpace(_pendingRescueContext.EnemyPartyName)
                    ? new TextObject(_pendingRescueContext.EnemyPartyName)
                    : enemyFactionLink);

            Settlement? nearestTown = Settlement.All
                .Where(s => s.IsTown)
                .OrderBy(s => _pendingRescueContext.Position.Distance(s.GatePosition))
                .FirstOrDefault();

            TextObject nearestTownLink = nearestTown?.EncyclopediaLinkWithName
                ?? new TextObject("the nearest town");

            TextObject bornSettlementLink = _pendingRescueHero?.BornSettlement?.EncyclopediaLinkWithName
                ?? nearestTownLink;

            TextObject survivorFullName = _pendingRescueHero?.Name
                ?? new TextObject("a nameless soldier");

            TextObject survivorCultureName = _pendingRescueContext.Culture?.Name
                ?? new TextObject("Calradia");

            MBTextManager.SetTextVariable("SURVIVOR_LORD_NAME", lordLink);
            MBTextManager.SetTextVariable("ENEMY_LORD_NAME", enemyLordLink);
            MBTextManager.SetTextVariable("SURVIVOR_FACTION_NAME", formerFactionLink);
            MBTextManager.SetTextVariable("ENEMY_FACTION_NAME", enemyFactionLink);
            MBTextManager.SetTextVariable("NEAREST_SETTLEMENT_NAME", nearestTownLink);
            MBTextManager.SetTextVariable("TARGET_TOWN_NAME", nearestTownLink);
            MBTextManager.SetTextVariable("BORN_SETTLEMENT_NAME", bornSettlementLink);
            MBTextManager.SetTextVariable("SURVIVOR_FULL_NAME", survivorFullName);
            MBTextManager.SetTextVariable("SURVIVOR_CULTURE_NAME", survivorCultureName);
            MBTextManager.SetTextVariable("BATTLE_CASUALTIES", _pendingRescueContext.TotalCasualties);
            MBTextManager.SetTextVariable("HOURS_SINCE_BATTLE", _pendingRescueContext.HoursElapsedSinceBattle);
            MBTextManager.SetTextVariable("SURVIVOR_RELATION", Nl4dSettingsManager.SurvivorRelationBonus);

            if (_pendingRescueContext.IsNavalWreckage)
            {
                StringCalculator.SetDialogVariable(
                    "NL4D_RESCUE_INTRO",
                    "nl4d_rescue_naval_lord",
                    "The waters took almost everything... When {ENEMY_FACTION_NAME}'s fleet crashed into our line, {SURVIVOR_LORD_NAME}'s flagship was torn to kindling. I drifted amidst the timber for {HOURS_SINCE_BATTLE} hours before washing ashore.");
            }
            else if (_pendingRescueContext.WasFromWinningSide)
            {
                StringCalculator.SetDialogVariable(
                    "NL4D_RESCUE_INTRO",
                    "nl4d_rescue_winner_lord",
                    "We took the field against {ENEMY_LORD_NAME}, but victory has a bitter taste. When {SURVIVOR_LORD_NAME}'s host marched onward, they abandoned the wounded to die in the dirt. {HOURS_SINCE_BATTLE} hours under the open sky...");
            }
            else
            {
                StringCalculator.SetDialogVariable(
                    "NL4D_RESCUE_INTRO",
                    "nl4d_rescue_defeated_lord",
                    "Our line broke under {ENEMY_LORD_NAME}'s charge... {SURVIVOR_LORD_NAME} was routed, and {ENEMY_FACTION_NAME}'s men butchered anyone who could not flee. I lay buried beneath the corpses for {HOURS_SINCE_BATTLE} hours.");
            }

            StringCalculator.SetDialogVariable(
                "NL4D_RESCUE_PLEA",
                "nl4d_rescue_plea",
                "You found me just in time. I will never forget this mercy (Relation +{SURVIVOR_RELATION}). Take me with you, and my blade is yours.");
        }

        private void OnAskBattleLoreConsequence()
        {
            _askedBattleLore = true;
            SetupRescueDialogVariables();

            string baseId = _pendingRescueContext != null && _pendingRescueContext.WasFromWinningSide
                ? "nl4d_rescue_battle_lore_winner"
                : "nl4d_rescue_battle_lore_defeated";

            string fallback = _pendingRescueContext != null && _pendingRescueContext.WasFromWinningSide
                ? "{SURVIVOR_LORD_NAME} led us against {ENEMY_LORD_NAME} in a brutal clash. Over {BATTLE_CASUALTIES} men fell before {ENEMY_FACTION_NAME} broke. But when the vanguard pursued them, nobody bothered to carry the broken back to camp."
                : "It was a slaughter. {ENEMY_LORD_NAME}'s cavalry smashed through our vanguard. {BATTLE_CASUALTIES} souls perished on this field. {SURVIVOR_LORD_NAME} fled or fell, and we were trampled beneath iron and hooves.";

            StringCalculator.SetDialogVariable("NL4D_RESCUE_BATTLE_LORE", baseId, fallback);
        }

        private void OnAskIdentityLoreConsequence()
        {
            _askedIdentity = true;
            SetupRescueDialogVariables();

            string fallback = "I was born near {BORN_SETTLEMENT_NAME}, marching under {SURVIVOR_FACTION_NAME}'s banners since I was old enough to hold a spear. My name is {SURVIVOR_FULL_NAME}. I have no home left to return to, only my skill with steel.";

            StringCalculator.SetDialogVariable("NL4D_RESCUE_IDENTITY_LORE", "nl4d_rescue_identity_lore", fallback);
        }

        private void OnAskWoundsLoreConsequence()
        {
            _askedWounds = true;
            SetupRescueDialogVariables();

            string fallback = "A spear grazed my ribs and a bolt took me in the shoulder. The blood loss nearly took me, but the bleeding has stopped. With a few days of bread and rest, I will be fit to fight again.";

            StringCalculator.SetDialogVariable("NL4D_RESCUE_WOUNDS_LORE", "nl4d_rescue_wounds_lore", fallback);
            StringCalculator.SetDialogVariable(
                "NL4D_RESCUE_PLEA",
                "nl4d_rescue_plea",
                "You found me just in time. I will never forget this mercy (Relation +{SURVIVOR_RELATION}). Take me with you, and my blade is yours.");
        }

        private bool OnRescueRecruitClickableCondition(out TextObject explanation)
        {
            if (!SurvivorRecruitmentService.CanPlayerRecruitSurvivor(out string? reason))
            {
                explanation = new TextObject(reason ?? string.Empty);
                return false;
            }

            explanation = TextObject.GetEmpty();
            return true;
        }

        private void OnRescueRecruitConsequence()
        {
            if (_pendingRescueHero == null || _pendingRescueRecord == null)
            {
                return;
            }

            SurvivorRecruitmentService.RecruitSurvivorFree(_pendingRescueHero, _pendingRescueRecord);

            if (!_rescuedSurvivors.Contains(_pendingRescueRecord))
            {
                _rescuedSurvivors.Add(_pendingRescueRecord);
            }

            var msg = new TextObject("{=nl4d_msg_survivor_recruited}{HERO_NAME} has joined your party as a loyal companion.");
            msg.SetTextVariable("HERO_NAME", _pendingRescueHero.Name);
            InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Colors.Green));

            _pendingRescueHero = null;
            _pendingRescueRecord = null;
            _pendingRescueContext = null;
        }

        private void OnRescueDeclineConsequence()
        {
            FinalizeSurvivorAsUnrecruited();
            _pendingRescueHero = null;
            _pendingRescueRecord = null;
            _pendingRescueContext = null;
        }

        private void FinalizeSurvivorAsUnrecruited()
        {
            if (_pendingRescueHero == null || _pendingRescueRecord == null || _pendingRescueContext == null)
            {
                return;
            }

            Settlement? town = SurvivorRecruitmentService.SendUnrecruitedSurvivorToNearestTown(
                _pendingRescueHero,
                _pendingRescueContext.Position);

            SurvivorFifoPoolService.RegisterUnrecruitedAndEnforceCap(_rescuedSurvivors, _pendingRescueRecord);

            TextObject townLink = town?.EncyclopediaLinkWithName ?? new TextObject("the nearest settlement");
            MBTextManager.SetTextVariable("TARGET_TOWN_NAME", townLink);

            var msg = new TextObject("{=nl4d_msg_survivor_sent_town}{HERO_NAME} has been escorted to {TARGET_TOWN_NAME}. You can find them in the local tavern.");
            msg.SetTextVariable("HERO_NAME", _pendingRescueHero.Name);
            msg.SetTextVariable("TARGET_TOWN_NAME", townLink);
            InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Colors.Cyan));
        }

        private void OnConversationEnded(System.Collections.Generic.IEnumerable<CharacterObject> characters)
        {
            if (_isInitialRescueConversation && _pendingRescueHero != null)
            {
                // Safety net: if player exited via ESC or interruption before choosing
                if (_pendingRescueRecord != null && !_pendingRescueRecord.IsRecruited)
                {
                    FinalizeSurvivorAsUnrecruited();
                }
            }

            ClearPendingState();
        }
    }
}
