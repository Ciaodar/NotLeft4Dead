using System.Linq;
using NL4D.Config;
using NL4D.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace NL4D.Services
{
    /// <summary>
    /// Handles companion recruitment and unrecruited dispersal workflows for rescued survivors.
    /// Manages clan companion limits, party insertion, and safe settlement relocation.
    /// </summary>
    public static class SurvivorRecruitmentService
    {
        public static bool CanPlayerRecruitSurvivor(out string? reasonIfBlocked)
        {
            if (Nl4dSettingsManager.AllowExceedingCompanionLimit)
            {
                reasonIfBlocked = null;
                return true;
            }

            if (Clan.PlayerClan == null)
            {
                reasonIfBlocked = null;
                return false;
            }

            if (Clan.PlayerClan.Companions.Count >= Clan.PlayerClan.CompanionLimit)
            {
                var text = new TextObject("{=nl4d_companion_limit_reached}You cannot recruit any more companions ({CURRENT}/{LIMIT}).");
                text.SetTextVariable("CURRENT", Clan.PlayerClan.Companions.Count);
                text.SetTextVariable("LIMIT", Clan.PlayerClan.CompanionLimit);
                reasonIfBlocked = text.ToString();
                return false;
            }

            reasonIfBlocked = null;
            return true;
        }

        public static void RecruitSurvivorFree(Hero survivor, RescuedSurvivorRecord record)
        {
            if (survivor == null || Clan.PlayerClan == null)
            {
                return;
            }

            AddCompanionAction.Apply(Clan.PlayerClan, survivor);

            if (MobileParty.MainParty != null)
            {
                AddHeroToPartyAction.Apply(survivor, MobileParty.MainParty);
            }

            if (record != null)
            {
                record.IsRecruited = true;
            }
        }

        public static Settlement? SendUnrecruitedSurvivorToNearestTown(Hero survivor, CampaignVec2 referencePosition)
        {
            if (survivor == null)
            {
                return null;
            }

            Settlement? nearestTown = Settlement.All
                .Where(s => s.IsTown)
                .OrderBy(s => referencePosition.Distance(s.GatePosition))
                .FirstOrDefault();

            if (nearestTown == null)
            {
                nearestTown = Settlement.All.FirstOrDefault(s => s.IsTown);
            }

            if (nearestTown == null)
            {
                return null;
            }

            survivor.ChangeState(Hero.CharacterStates.Active);
            EnterSettlementAction.ApplyForCharacterOnly(survivor, nearestTown);

            return nearestTown;
        }
    }
}
