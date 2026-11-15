using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace NL4D.Data
{
    /// <summary>
    /// Represents a persistent record of a survivor rescued, recruited, or released from a battle wreckage.
    /// Serialized using TaleWorlds.SaveSystem with explicit SaveableField attributes.
    /// </summary>
    public sealed class RescuedSurvivorRecord
    {
        [SaveableField(1)]
        public Hero? SurvivorHero;

        [SaveableField(2)]
        public string FormerLeaderName = string.Empty;

        [SaveableField(3)]
        public string FormerFactionName = string.Empty;

        [SaveableField(4)]
        public string EnemyFactionName = string.Empty;

        [SaveableField(5)]
        public bool WasFromWinningSide;

        [SaveableField(6)]
        public CampaignTime RescuedTime;

        [SaveableField(7)]
        public bool IsRecruited;

        public RescuedSurvivorRecord()
        {
        }

        public RescuedSurvivorRecord(
            Hero? survivorHero,
            string formerLeaderName,
            string formerFactionName,
            string enemyFactionName,
            bool wasFromWinningSide,
            CampaignTime rescuedTime,
            bool isRecruited)
        {
            SurvivorHero = survivorHero;
            FormerLeaderName = formerLeaderName ?? string.Empty;
            FormerFactionName = formerFactionName ?? string.Empty;
            EnemyFactionName = enemyFactionName ?? string.Empty;
            WasFromWinningSide = wasFromWinningSide;
            RescuedTime = rescuedTime;
            IsRecruited = isRecruited;
        }
    }
}
