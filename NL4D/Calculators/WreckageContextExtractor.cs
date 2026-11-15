using System;
using System.Linq;
using NL4D.Config;
using NL4D.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BattleWreckages;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NL4D.Calculators
{
    /// <summary>
    /// Extracts structured battle and faction context from a Native BattleWreckage site.
    /// Determines former lord allegiance, victory state, culture, and environmental conditions.
    /// </summary>
    public static class WreckageContextExtractor
    {
        public static WreckageBattleContext? Extract(BattleWreckage? wreckage)
        {
            if (wreckage == null)
            {
                return null;
            }

            bool attackerWon = wreckage.WinnerSide == BattleSideEnum.Attacker;
            Hero? winningHero = attackerWon ? wreckage.AttackerLeaderHero : wreckage.DefenderLeaderHero;
            IFaction? winningFaction = attackerWon ? wreckage.AttackerFaction : wreckage.DefenderFaction;
            TextObject? winningPartyText = attackerWon ? wreckage.AttackerLeaderPartyName : wreckage.DefenderLeaderPartyName;

            Hero? defeatedHero = attackerWon ? wreckage.DefenderLeaderHero : wreckage.AttackerLeaderHero;
            IFaction? defeatedFaction = attackerWon ? wreckage.DefenderFaction : wreckage.AttackerFaction;
            TextObject? defeatedPartyText = attackerWon ? wreckage.DefenderLeaderPartyName : wreckage.AttackerLeaderPartyName;

            // Exclude Hero.MainHero to prevent treating player-led sides as an NPC Lord who abandoned survivors
            bool winnerEligible = winningHero != null && winningHero != Hero.MainHero;
            bool defeatedEligible = defeatedHero != null && defeatedHero != Hero.MainHero;

            if (!winnerEligible && !defeatedEligible)
            {
                return null;
            }

            // Decide which side's survivor we spawn
            bool selectWinner;
            if (winnerEligible && defeatedEligible)
            {
                float roll = MBRandom.RandomFloat;
                selectWinner = roll < Nl4dSettingsManager.WinnerSideSurvivorWeight;
            }
            else
            {
                selectWinner = winnerEligible;
            }

            Hero formerLeaderHero = selectWinner ? winningHero! : defeatedHero!;
            IFaction? formerFaction = selectWinner ? winningFaction : defeatedFaction;
            string formerPartyName = (selectWinner ? winningPartyText : defeatedPartyText)?.ToString() ?? string.Empty;

            IFaction? enemyFaction = selectWinner ? defeatedFaction : winningFaction;
            string enemyPartyName = (selectWinner ? defeatedPartyText : winningPartyText)?.ToString() ?? string.Empty;
            bool wasFromWinningSide = selectWinner;

            // Determine culture: FormerLeaderHero.Culture -> FormerFaction.Culture -> Closest Town -> Player
            CultureObject? culture = formerLeaderHero.Culture;
            if (culture == null && formerFaction != null)
            {
                culture = formerFaction.Culture;
            }

            if (culture == null)
            {
                Settlement? nearestTown = Settlement.All
                    .Where(s => s.IsTown)
                    .OrderBy(s => wreckage.Position.Distance(s.GatePosition))
                    .FirstOrDefault();

                culture = nearestTown?.Culture;
            }

            if (culture == null)
            {
                culture = Hero.MainHero?.Culture ?? CharacterObject.PlayerCharacter?.Culture ?? MBObjectManager.Instance.GetObject<CultureObject>("empire");
            }

            if (culture == null)
            {
                return null;
            }

            bool isNavalWreckage = !wreckage.Position.IsOnLand;
            int totalCasualties = wreckage.TotalCasualtyCountInBattle;
            int hoursElapsed = Math.Max(1, (int)(CampaignTime.Now - wreckage.BattleStartTime).ToHours);

            return new WreckageBattleContext(
                wreckage.StringId ?? string.Empty,
                formerLeaderHero,
                formerFaction,
                formerPartyName,
                enemyFaction,
                enemyPartyName,
                culture,
                wasFromWinningSide,
                isNavalWreckage,
                totalCasualties,
                hoursElapsed,
                wreckage.Position
            );
        }
    }
}
