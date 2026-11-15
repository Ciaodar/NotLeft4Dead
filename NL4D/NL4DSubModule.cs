using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace NL4D
{
    /// <summary>
    /// Entry point for Not Left 4 Dead (NL4D).
    /// Handles optional MCM bridge loading, config initialization, and campaign behavior registration.
    /// </summary>
    public class NL4DSubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            // Activate optional MCM bridge if MCMv5 is available in the environment.
            TryActivateOptionalMcmBridge();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            // Initialize core settings (MCM provider if active, otherwise JSON fallback).
            Config.Nl4dSettingsManager.Initialize();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarterObject;
                campaignStarter.AddBehavior(new Behaviors.WreckageSurvivorBehavior());
            }
        }

        private static void TryActivateOptionalMcmBridge()
        {
            try
            {
                // Verify whether MCMv5 is present in the AppDomain or can be resolved.
                Assembly? mcmAssembly = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(a => string.Equals(a.GetName().Name, "MCMv5", StringComparison.OrdinalIgnoreCase));

                if (mcmAssembly == null)
                {
                    try
                    {
                        mcmAssembly = Assembly.Load("MCMv5");
                    }
                    catch
                    {
                        // MCMv5 is not installed or available; run purely on JSON fallback.
                        return;
                    }
                }

                if (mcmAssembly == null)
                    return;

                string assemblyLocation = typeof(NL4DSubModule).Assembly.Location;
                if (string.IsNullOrWhiteSpace(assemblyLocation))
                    return;

                string? binDir = Path.GetDirectoryName(assemblyLocation);
                if (string.IsNullOrWhiteSpace(binDir))
                    return;

                string bridgePath = Path.Combine(binDir, "NL4D.MCMBridge.dll");
                if (!File.Exists(bridgePath))
                    return;

                Assembly bridgeAssembly = AppDomain.CurrentDomain
                                              .GetAssemblies()
                                              .FirstOrDefault(a => string.Equals(a.GetName().Name, "NL4D.MCMBridge", StringComparison.OrdinalIgnoreCase))
                                          ?? Assembly.LoadFrom(bridgePath);

                Type? bootstrapType = bridgeAssembly.GetType("NL4D.MCMBridge.BridgeBootstrap");
                MethodInfo? tryRegister = bootstrapType?.GetMethod("TryRegister", BindingFlags.Public | BindingFlags.Static);
                tryRegister?.Invoke(null, null);
            }
            catch
            {
                // Optional bridge failure must never crash core mod initialization.
            }
        }
    }
}
