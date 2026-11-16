using NL4D.Data;
using NL4D.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NL4D.Behaviors
{
    /// <summary>
    /// Tavern reunion dialogue partial class for WreckageSurvivorBehavior.
    /// Manages tavern encounters with previously rescued, unrecruited survivors,
    /// allowing the player to recruit them later for free with high priority (120).
    /// </summary>
    public sealed partial class WreckageSurvivorBehavior
    {
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            AddRescueDialogues(starter);
            AddTavernReunionDialogues(starter);

            // Protect all existing unrecruited survivors from Native CompanionsCampaignBehavior.TryKillCompanion
            var unrecruited = new System.Collections.Generic.List<Hero>();
            for (int i = 0; i < _rescuedSurvivors.Count; i++)
            {
                var r = _rescuedSurvivors[i];
                if (r != null && !r.IsRecruited && r.SurvivorHero != null && r.SurvivorHero.IsAlive)
                {
                    unrecruited.Add(r.SurvivorHero);
                }
            }
            SurvivorFactoryService.ProtectAllUnrecruitedSurvivors(unrecruited);
        }

        private void AddTavernReunionDialogues(CampaignGameStarter starter)
        {
            // NPC Greeting in tavern with high priority (120) to precede Native wanderer lines (110)
            starter.AddDialogLine(
                "nl4d_reunion_greeting_line",
                "start",
                "nl4d_reunion_player_choice_token",
                "{=nl4d_reunion_greeting}I have not forgotten how you pulled me from the jaws of death, and I never will, my lord. I still wish with all my heart to join your company.",
                TavernReunionCondition,
                null,
                120
            );

            // Player Option 1: Recruit survivor from tavern for free
            starter.AddPlayerLine(
                "nl4d_reunion_option_recruit",
                "nl4d_reunion_player_choice_token",
                "nl4d_reunion_recruit_reply_token",
                "{=nl4d_reunion_accept}Certainly, we will accomplish great things together. Come with me.",
                null,
                OnTavernReunionRecruitConsequence,
                100,
                OnTavernReunionRecruitClickableCondition
            );

            // NPC accepted response
            starter.AddDialogLine(
                "nl4d_reunion_recruit_reply_line",
                "nl4d_reunion_recruit_reply_token",
                "close_window",
                "{=nl4d_reunion_accepted_reply}Thank you! My blade and life are yours.",
                null,
                null,
                100
            );

            // Player Option 2: Decline recruitment
            starter.AddPlayerLine(
                "nl4d_reunion_option_decline",
                "nl4d_reunion_player_choice_token",
                "nl4d_reunion_decline_reply_token",
                "{=nl4d_reunion_decline}Unfortunately, I still have no room for you.",
                null,
                null,
                90
            );

            // NPC declined response
            starter.AddDialogLine(
                "nl4d_reunion_decline_reply_line",
                "nl4d_reunion_decline_reply_token",
                "close_window",
                "{=nl4d_reunion_declined_reply}I understand, my lord. I will remain here in the tavern if you change your mind.",
                null,
                null,
                100
            );
        }

        private bool TavernReunionCondition()
        {
            if (_isInitialRescueConversation)
            {
                return false;
            }

            Hero? currentHero = Hero.OneToOneConversationHero;
            if (currentHero == null)
            {
                return false;
            }

            return TryGetUnrecruitedRecord(currentHero, out _);
        }

        private bool OnTavernReunionRecruitClickableCondition(out TextObject explanation)
        {
            if (!SurvivorRecruitmentService.CanPlayerRecruitSurvivor(out string? reason))
            {
                explanation = new TextObject(reason ?? string.Empty);
                return false;
            }

            explanation = TextObject.GetEmpty();
            return true;
        }

        private void OnTavernReunionRecruitConsequence()
        {
            Hero? currentHero = Hero.OneToOneConversationHero;
            if (currentHero == null)
            {
                return;
            }

            if (TryGetUnrecruitedRecord(currentHero, out RescuedSurvivorRecord? record) && record != null)
            {
                SurvivorRecruitmentService.RecruitSurvivorFree(currentHero, record);

                var msg = new TextObject("{=nl4d_msg_survivor_recruited}{HERO_NAME} has joined your party as a loyal companion.");
                msg.SetTextVariable("HERO_NAME", currentHero.Name);
                InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Colors.Green));
            }
        }
    }
}
