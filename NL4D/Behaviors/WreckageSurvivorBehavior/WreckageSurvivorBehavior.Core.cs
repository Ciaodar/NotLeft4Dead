using System;
using System.Collections.Generic;
using NL4D.Calculators;
using NL4D.Config;
using NL4D.Data;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.BattleWreckages;
using TaleWorlds.Core;

namespace NL4D.Behaviors
{
    /// <summary>
    /// Core lifecycle, state storage, and event registration for WreckageSurvivorBehavior.
    /// Manages persistent rescued survivors and wreckage processing history.
    /// </summary>
    public sealed partial class WreckageSurvivorBehavior : CampaignBehaviorBase
    {
        public static WreckageSurvivorBehavior? Instance { get; private set; }

        // Persistent data saved with savegame
        private List<RescuedSurvivorRecord> _rescuedSurvivors = new List<RescuedSurvivorRecord>();
        private List<string> _processedWreckageIds = new List<string>();

        // Transient session state
        private BattleWreckage? _activeWreckage;
        private Hero? _pendingRescueHero;
        private RescuedSurvivorRecord? _pendingRescueRecord;
        private WreckageBattleContext? _pendingRescueContext;
        private bool _isInitialRescueConversation;
        private bool _forceNextWreckageSpawn;
        private bool _awaitingConversationLaunchOnTick;
        private bool _askedBattleLore;
        private bool _askedIdentity;
        private bool _askedWounds;

        public WreckageSurvivorBehavior()
        {
            Instance = this;
            Nl4dSettingsManager.OnClearDataRequested += HandleClearDataRequested;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.GameMenuOpened.AddNonSerializedListener(this, OnGameMenuOpened);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, OnConversationEnded);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("_rescuedSurvivors", ref _rescuedSurvivors);
            dataStore.SyncData("_processedWreckageIds", ref _processedWreckageIds);

            if (dataStore.IsLoading)
            {
                if (_rescuedSurvivors == null)
                {
                    _rescuedSurvivors = new List<RescuedSurvivorRecord>();
                }
                if (_processedWreckageIds == null)
                {
                    _processedWreckageIds = new List<string>();
                }
            }
        }

        private void ArmConversationLaunchOnNextFrame()
        {
            _awaitingConversationLaunchOnTick = true;
            if (Game.Current != null)
            {
                Game.Current.AfterTick -= OnGameAfterTick;
                Game.Current.AfterTick += OnGameAfterTick;
            }
        }

        private void OnGameAfterTick(float dt)
        {
            if (!_awaitingConversationLaunchOnTick)
            {
                if (Game.Current != null)
                {
                    Game.Current.AfterTick -= OnGameAfterTick;
                }
                return;
            }

            if (GameStateManager.Current?.ActiveState is TaleWorlds.CampaignSystem.GameState.MapState
                && Campaign.Current?.ConversationManager != null
                && !Campaign.Current.ConversationManager.IsConversationInProgress)
            {
                _awaitingConversationLaunchOnTick = false;
                if (Game.Current != null)
                {
                    Game.Current.AfterTick -= OnGameAfterTick;
                }
                LaunchRescueConversation();
            }
        }

        private void HandleClearDataRequested()
        {
            _rescuedSurvivors.Clear();
            _processedWreckageIds.Clear();
            ClearPendingState();
        }

        public void ForceNextSpawn()
        {
            _forceNextWreckageSpawn = true;
        }

        public IReadOnlyList<RescuedSurvivorRecord> GetRescuedSurvivorsSnapshot()
        {
            return _rescuedSurvivors.AsReadOnly();
        }

        public bool TryGetUnrecruitedRecord(Hero hero, out RescuedSurvivorRecord? record)
        {
            record = null;
            if (hero == null)
            {
                return false;
            }

            for (int i = 0; i < _rescuedSurvivors.Count; i++)
            {
                var r = _rescuedSurvivors[i];
                if (r != null && !r.IsRecruited && r.SurvivorHero == hero)
                {
                    record = r;
                    return true;
                }
            }

            return false;
        }

        private void ClearPendingState()
        {
            if (Game.Current != null)
            {
                Game.Current.AfterTick -= OnGameAfterTick;
            }
            _activeWreckage = null;
            _pendingRescueHero = null;
            _pendingRescueRecord = null;
            _pendingRescueContext = null;
            _isInitialRescueConversation = false;
            _awaitingConversationLaunchOnTick = false;
            _askedBattleLore = false;
            _askedIdentity = false;
            _askedWounds = false;
            StringCalculator.ClearDialogVariables();
        }
    }
}
