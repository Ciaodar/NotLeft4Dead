using TaleWorlds.CampaignSystem;

namespace NL4D.Data
{
    /// <summary>
    /// Encapsulates the battle context extracted from a BattleWreckage site.
    /// Provides context on factions, commanders, casualties, and elapsed time.
    /// </summary>
    public sealed class WreckageBattleContext
    {
        public string WreckageId { get; set; } = string.Empty;
        public Hero? FormerLeaderHero { get; set; }
        public IFaction? FormerFaction { get; set; }
        public string FormerPartyName { get; set; } = string.Empty;
        public IFaction? EnemyFaction { get; set; }
        public string EnemyPartyName { get; set; } = string.Empty;
        public CultureObject Culture { get; set; } = null!;
        public bool WasFromWinningSide { get; set; }
        public bool IsNavalWreckage { get; set; }
        public int TotalCasualties { get; set; }
        public int HoursElapsedSinceBattle { get; set; }
        public CampaignVec2 Position { get; set; }

        public WreckageBattleContext()
        {
        }

        public WreckageBattleContext(
            string wreckageId,
            Hero? formerLeaderHero,
            IFaction? formerFaction,
            string formerPartyName,
            IFaction? enemyFaction,
            string enemyPartyName,
            CultureObject culture,
            bool wasFromWinningSide,
            bool isNavalWreckage,
            int totalCasualties,
            int hoursElapsedSinceBattle,
            CampaignVec2 position)
        {
            WreckageId = wreckageId ?? string.Empty;
            FormerLeaderHero = formerLeaderHero;
            FormerFaction = formerFaction;
            FormerPartyName = formerPartyName ?? string.Empty;
            EnemyFaction = enemyFaction;
            EnemyPartyName = enemyPartyName ?? string.Empty;
            Culture = culture;
            WasFromWinningSide = wasFromWinningSide;
            IsNavalWreckage = isNavalWreckage;
            TotalCasualties = totalCasualties;
            HoursElapsedSinceBattle = hoursElapsedSinceBattle;
            Position = position;
        }
    }
}
