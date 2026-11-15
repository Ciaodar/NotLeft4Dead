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
    /// Handles initial encounter dialogue tree, survivor recruitment, or town escort routing.
    /// </summary>
    public sealed partial class WreckageSurvivorBehavior
    {
        private void AddRescueDialogues(CampaignGameStarter starter)
        {
            // NPC initial opening line from the debris
            starter.AddDialogLine(
                "nl4d_rescue_intro_line",
                "start",
                "nl4d_rescue_plea_token",
                "{=!}{NL4D_RESCUE_INTRO}",
                RescueIntroCondition,
                null,
                125
            );

            // NPC follow-up plea for help / recruitment
            starter.AddDialogLine(
                "nl4d_rescue_plea_line",
                "nl4d_rescue_plea_token",
                "nl4d_rescue_player_choice_token",
                "{=nl4d_rescue_plea}I have nothing left to give, yet I can hold a blade and pledge my life to your service. Please, take me with you, or at least help me reach safety.",
                null,
                null,
                100
            );

            // Player Option 1: Recruit survivor into party for free
            starter.AddPlayerLine(
                "nl4d_rescue_option_recruit",
                "nl4d_rescue_player_choice_token",
                "nl4d_rescue_recruit_reply_token",
                "{=nl4d_rescue_accept}Rise, friend. Anyone who survived that carnage belongs by my side. Welcome to my company.",
                null,
                OnRescueRecruitConsequence,
                100,
                OnRescueRecruitClickableCondition
            );

            // NPC grateful response to recruitment
            starter.AddDialogLine(
                "nl4d_rescue_recruit_reply_line",
                "nl4d_rescue_recruit_reply_token",
                "close_window",
                "{=nl4d_rescue_accepted_reply}By the heavens, you will not regret this! I will fight for you until my last breath.",
                null,
                null,
                100
            );

            // Player Option 2: Decline recruitment, send survivor safely to town
            starter.AddPlayerLine(
                "nl4d_rescue_option_decline",
                "nl4d_rescue_player_choice_token",
                "nl4d_rescue_decline_reply_token",
                "{=nl4d_rescue_decline}I cannot take you into my ranks right now, but I will see you safely guided to the nearest town. Rest and recover.",
                null,
                OnRescueDeclineConsequence,
                90
            );

            // NPC response to decline
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

            SetupRescueDialogVariables();
            return true;
        }

        private void SetupRescueDialogVariables()
        {
            if (_pendingRescueContext == null)
            {
                return;
            }

            TextObject lordName = _pendingRescueContext.FormerLeaderHero?.Name
                ?? (!string.IsNullOrWhiteSpace(_pendingRescueContext.FormerPartyName)
                    ? new TextObject(_pendingRescueContext.FormerPartyName)
                    : new TextObject("my commander"));

            TextObject formerFactionName = _pendingRescueContext.FormerFaction?.Name
                ?? (!string.IsNullOrWhiteSpace(_pendingRescueContext.FormerPartyName)
                    ? new TextObject(_pendingRescueContext.FormerPartyName)
                    : new TextObject("our army"));

            TextObject enemyFactionName = _pendingRescueContext.EnemyFaction?.Name
                ?? (!string.IsNullOrWhiteSpace(_pendingRescueContext.EnemyPartyName)
                    ? new TextObject(_pendingRescueContext.EnemyPartyName)
                    : new TextObject("the enemy"));

            TextObject partyName = !string.IsNullOrWhiteSpace(_pendingRescueContext.FormerPartyName)
                ? new TextObject(_pendingRescueContext.FormerPartyName)
                : lordName;

            MBTextManager.SetTextVariable("SURVIVOR_LORD_NAME", lordName);
            MBTextManager.SetTextVariable("SURVIVOR_FACTION_NAME", formerFactionName);
            MBTextManager.SetTextVariable("ENEMY_FACTION_NAME", enemyFactionName);
            MBTextManager.SetTextVariable("SURVIVOR_PARTY_NAME", partyName);
            MBTextManager.SetTextVariable("BATTLE_CASUALTIES", _pendingRescueContext.TotalCasualties);
            MBTextManager.SetTextVariable("HOURS_SINCE_BATTLE", _pendingRescueContext.HoursElapsedSinceBattle);
            MBTextManager.SetTextVariable("SURVIVOR_RELATION", Nl4dSettingsManager.SurvivorRelationBonus);

            if (_pendingRescueContext.IsNavalWreckage)
            {
                StringCalculator.SetDialogVariable(
                    "NL4D_RESCUE_INTRO",
                    "nl4d_rescue_naval_lord",
                    "The waters took almost everything... I watched our vessels splinter under the waves. By sheer fortune, I washed ashore amidst the floating timber.");
            }
            else if (_pendingRescueContext.WasFromWinningSide)
            {
                StringCalculator.SetDialogVariable(
                    "NL4D_RESCUE_INTRO",
                    "nl4d_rescue_winner_lord",
                    "We took the field that day, but when the banners marched on, they left the wounded behind to perish. I thought the scavengers would be my end.");
            }
            else
            {
                StringCalculator.SetDialogVariable(
                    "NL4D_RESCUE_INTRO",
                    "nl4d_rescue_defeated_lord",
                    "Our line broke under the weight of the enemy charge... My lord's banners fell, and I was trampled into the mud and left for dead.");
            }
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

            string townName = town?.Name?.ToString() ?? "the nearest settlement";
            MBTextManager.SetTextVariable("TARGET_TOWN_NAME", townName);

            var msg = new TextObject("{=nl4d_msg_survivor_sent_town}{HERO_NAME} has been escorted to {TARGET_TOWN_NAME}. You can find them in the local tavern.");
            msg.SetTextVariable("HERO_NAME", _pendingRescueHero.Name);
            msg.SetTextVariable("TARGET_TOWN_NAME", townName);
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
