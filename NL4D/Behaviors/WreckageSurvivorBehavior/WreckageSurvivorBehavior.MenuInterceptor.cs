using System;
using System.Reflection;
using NL4D.Calculators;
using NL4D.Config;
using NL4D.Data;
using NL4D.Services;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BattleWreckages;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace NL4D.Behaviors
{
    /// <summary>
    /// Menu interception partial class for WreckageSurvivorBehavior.
    /// Hooks Native battle wreckage menus to evaluate survivor spawn chances and launch rescue encounters.
    /// </summary>
    public sealed partial class WreckageSurvivorBehavior
    {
        private void OnGameMenuOpened(MenuCallbackArgs args)
        {
            string? menuId = args.MenuContext?.GameMenu?.StringId ?? Campaign.Current.CurrentMenuContext?.GameMenu?.StringId;
            if (string.IsNullOrEmpty(menuId))
            {
                return;
            }

            if (menuId == "battle_wreckage" || menuId == "battle_wreckage_investigate_wait_menu")
            {
                _activeWreckage = MobileParty.MainParty?.Ai?.AiBehaviorInteractable as BattleWreckage
                    ?? GetEncounteredWreckageFromNativeBehavior()
                    ?? FindClosestWreckage(MobileParty.MainParty?.Position ?? CampaignVec2.Zero, 15f);
            }
            else if (menuId == "battle_wreckage_recover_troops_decision" || menuId == "battle_wreckage_results_connection_menu" || menuId == "battle_wreckage_results_menu")
            {
                if (!Nl4dSettingsManager.ModEnabled)
                {
                    return;
                }

                BattleWreckage? wreckage = _activeWreckage
                    ?? (MobileParty.MainParty?.Ai?.AiBehaviorInteractable as BattleWreckage)
                    ?? GetEncounteredWreckageFromNativeBehavior()
                    ?? FindClosestWreckage(MobileParty.MainParty?.Position ?? CampaignVec2.Zero, 15f);

                if (wreckage == null)
                {
                    return;
                }

                string wreckageId = !string.IsNullOrEmpty(wreckage.StringId)
                    ? wreckage.StringId
                    : wreckage.Id.ToString();

                if (string.IsNullOrEmpty(wreckageId) || _processedWreckageIds.Contains(wreckageId))
                {
                    return;
                }

                _processedWreckageIds.Add(wreckageId);
                if (_processedWreckageIds.Count > 200)
                {
                    _processedWreckageIds.RemoveRange(0, _processedWreckageIds.Count - 200);
                }

                WreckageBattleContext? context = WreckageContextExtractor.Extract(wreckage);
                if (context == null)
                {
                    return;
                }

                bool shouldForce = _forceNextWreckageSpawn || Nl4dSettingsManager.DebugForceSpawn;
                _forceNextWreckageSpawn = false;

                bool spawnSuccess = SurvivorSpawnCalculator.RollForSurvivor(context, shouldForce);
                if (!spawnSuccess)
                {
                    return;
                }

                var created = SurvivorFactoryService.CreateSurvivorFromWreckage(context);
                if (!created.HasValue)
                {
                    return;
                }

                _pendingRescueHero = created.Value.Survivor;
                _pendingRescueRecord = created.Value.Record;
                _pendingRescueContext = context;
                _isInitialRescueConversation = true;

                var msg = new TextObject("{=nl4d_msg_survivor_found}Among the debris and fallen soldiers, you hear a faint groan... A survivor clings to life!");
                InformationManager.DisplayMessage(new InformationMessage(msg.ToString(), Colors.Yellow));

                // Arm conversation launch on next UI frame via Game.Current.AfterTick
                ArmConversationLaunchOnNextFrame();
            }
        }

        private void LaunchRescueConversation()
        {
            if (_pendingRescueHero == null || _pendingRescueHero.CharacterObject == null)
            {
                ClearPendingState();
                return;
            }

            try
            {
                var playerData = new ConversationCharacterData(CharacterObject.PlayerCharacter);
                var survivorData = new ConversationCharacterData(
                    character: _pendingRescueHero.CharacterObject,
                    party: null,
                    noHorse: true,
                    noWeapon: true,
                    spawnAfterFight: false,
                    isCivilianEquipmentRequiredForLeader: false,
                    isCivilianEquipmentRequiredForBodyGuardCharacters: false,
                    noBodyguards: true);

                CampaignMapConversation.OpenConversation(playerData, survivorData);
            }
            catch (Exception ex)
            {
                Debug.Print($"[NL4D] Failed to open conversation with survivor: {ex.Message}");
                FinalizeSurvivorAsUnrecruited();
                ClearPendingState();
            }
        }

        private static BattleWreckage? FindClosestWreckage(CampaignVec2 position, float maxDistance)
        {
            var wreckages = Campaign.Current?.CampaignObjectManager?.Wreckages;
            if (wreckages == null || wreckages.Count == 0)
            {
                return null;
            }

            BattleWreckage? closest = null;
            float closestDist = maxDistance;

            for (int i = 0; i < wreckages.Count; i++)
            {
                var w = wreckages[i];
                if (w == null) continue;
                float dist = position.Distance(w.Position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = w;
                }
            }

            return closest;
        }

        private static BattleWreckage? GetEncounteredWreckageFromNativeBehavior()
        {
            try
            {
                var nativeBehavior = Campaign.Current?.GetCampaignBehavior<BattleWreckageCampaignBehavior>();
                if (nativeBehavior != null)
                {
                    var field = typeof(BattleWreckageCampaignBehavior).GetField("_encounteredBattleWreckage", BindingFlags.Instance | BindingFlags.NonPublic);
                    return field?.GetValue(nativeBehavior) as BattleWreckage;
                }
            }
            catch
            {
                // Safe reflection fallback
            }
            return null;
        }
    }
}
