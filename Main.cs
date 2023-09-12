using FireEvent.Customs;
using Kitchen;
using KitchenData;
using KitchenLib;
using KitchenLib.Event;
using KitchenLib.References;
using KitchenMods;
using PreferenceSystem;
using PreferenceSystem.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static FireEvent.UpdateFireScoreCountUp;

// Namespace should have "Kitchen" in the beginning
namespace FireEvent
{
    public class Main : BaseMod, IModSystem
    {
        // GUID must be unique and is recommended to be in reverse domain name notation
        // Mod Name is displayed to the player and listed in the mods menu
        // Mod Version must follow semver notation e.g. "1.2.3"
        public const string MOD_GUID = "IcedMilo.PlateUp.FireEvent";
        public const string MOD_NAME = "Flame Festival";
        public const string MOD_VERSION = "1.2.3";
        public const string MOD_AUTHOR = "IcedMilo";
        public const string MOD_GAMEVERSION = ">=1.1.5";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.3" current and all future
        // e.g. ">=1.1.3 <=1.2.3" for all from/until

        // Boolean constant whose value depends on whether you built with DEBUG or RELEASE mode, useful for testing

        public static AssetBundle Bundle;

        internal static PreferenceSystemManager PrefManager;
        internal static IntArrayGenerator IntArrayGen;
        public const string GAME_MODE_ID = "gameMode";

        public const string COUNT_UP_TARGET_ID = "countUpTarget";
        public const string COUNT_UP_SCORING_MODE = "countUpScoringMode";
        public const string COUNT_UP_RANDOM_FIRE_INTERVAL_ID = "countUpRandomFireInterval";

        public const string COUNT_DOWN_STARTING_SCORE_ID = "countDownStartingScore";
        public const string COUNT_DOWN_BASE_DECAY_SPEED_ID = "countDownBaseDecaySpeed";
        public const string COUNT_DOWN_ITEM_DELIVERED_RECOVERY_ID = "countDownItemDeliveredRecovery";
        public const string COUNT_DOWN_GROUP_LEAVING_RECOVERY_ID = "countDownGroupLeavingRecovery";
        public const string COUNT_DOWN_PROXIMITY_MULTIPLIER_ID = "countDownProximityMultiplier";
        public const string COUNT_DOWN_LAST_DAY_ID = "countDownLastDay";
        public const string COUNT_DOWN_RANDOM_FIRE_INTERVAL_ID = "countDownRandomFireInterval";

        public const string AUTO_CARDS_TIPPING_CULTURE = "autoCardsTippingCulture";
        public const string AUTO_CARDS_INSTANT_SERVICE = "autoCardsInstantService";
        public const string GIVE_APPLIANCES_FIRE_EXTINGUISHER = "giveAppliancesFireExtinguisher";
        public const string MONEY_MODIFIERS_PRIVATE_DINING = "moneyModifiersPrivateDining";
        public const string MONEY_MODIFIERS_CHARITY_DONATION = "moneyModifiersCharityDonation";

        public const string SHOW_DECIMALS_ID = "showDecimals";

        internal static DummyPrivateDiningAppliance DummyPrivateDiningAppliance;

        public Main() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        private void AddGameData()
        {
            LogInfo("Attempting to register game data...");

            DummyPrivateDiningAppliance = AddGameDataObject<DummyPrivateDiningAppliance>();

            LogInfo("Done loading game data.");
        }

        protected override void OnUpdate()
        {
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);
            IntArrayGen = new IntArrayGenerator();

            IntArrayGen.Add(0, "Endless");
            IntArrayGen.AddRange(1, 15, 1, null, delegate (string prefKey, int value)
            {
                return value.ToString();
            });
            int[] lastDays = IntArrayGen.GetArray();
            string[] lastDayStrings = IntArrayGen.GetStrings();
            IntArrayGen.Clear();

            IntArrayGen.AddRange(10, 20000, 10, null, delegate (string prefKey, int value)
            {
                return value.ToString();
            });
            int[] countUpTargets = IntArrayGen.GetArray();
            string[] countUpTargetStrings = IntArrayGen.GetStrings();
            IntArrayGen.Clear();

            IntArrayGen.AddRange(0, 500, 10, null, delegate (string prefKey, int value)
            {
                return $"{value}%";
            });
            int[] proximityMultipliers = IntArrayGen.GetArray();
            string[] proximityMultiplierStrings = IntArrayGen.GetStrings();
            IntArrayGen.Clear();

            IntArrayGen.AddRange(5, 55, 5, null, delegate (string prefKey, int value)
            {
                return $"{value} mins";
            });
            IntArrayGen.AddRange(60, 2880, 60, null, delegate (string prefKey, int value)
            {
                return $"{Mathf.RoundToInt(value / 60f)} hour(s)";
            });
            int[] startingScores = IntArrayGen.GetArray();
            string[] startingScoreStrings = IntArrayGen.GetStrings();
            IntArrayGen.Clear();

            IntArrayGen.AddRange(1, 9, 1, null, delegate (string prefKey, int value)
            {
                return $"{value} second(s)/sec";
            });
            IntArrayGen.AddRange(10, 55, 5, null, delegate (string prefKey, int value)
            {
                return $"{value} seconds/sec";
            });
            IntArrayGen.AddRange(60, 3600, 60, null, delegate (string prefKey, int value)
            {
                return $"{Mathf.RoundToInt(value / 60f)} min(s)/sec";
            });
            int[] baseDecaySpeeds = IntArrayGen.GetArray();
            string[] baseDecaySpeedStrings = IntArrayGen.GetStrings();
            IntArrayGen.Clear();

            IntArrayGen.AddRange(0, 50, 10, null, delegate (string prefKey, int value)
            {
                return $"{value} seconds";
            });
            IntArrayGen.AddRange(60, 3600, 60, null, delegate (string prefKey, int value)
            {
                return $"{Mathf.RoundToInt(value / 60f)} min(s)";
            });
            int[] recoveries = IntArrayGen.GetArray();
            string[] recoveryStrings = IntArrayGen.GetStrings();
            IntArrayGen.Clear();

            IntArrayGen.Add(-1, "Disabled");
            IntArrayGen.AddRange(10, 50, 10, null, delegate (string prefKey, int value)
            {
                return $"{value} seconds";
            });
            IntArrayGen.AddRange(60, 300, 10, null, delegate (string prefKey, int value)
            {
                return $"{Mathf.FloorToInt(value / 60f)} min(s){(value % 60 != 0 ? $" {value % 60} seconds" : "")}";
            });
            int[] fireIntervals = IntArrayGen.GetArray();
            string[] fireIntervalStrings = IntArrayGen.GetStrings();
            IntArrayGen.Clear();

            FireGameMode[] gameModes = Enum.GetValues(typeof(FireGameMode)) as FireGameMode[];
            PrefManager
                .AddLabel("Active Game Mode")
                .AddOption<string>(
                    GAME_MODE_ID,
                    FireGameMode.None.ToString(),
                    gameModes.Select(x => x.ToString()).ToArray(),
                    gameModes.Select(x => x.ToString()).ToArray())
                .AddSpacer()

                .AddSubmenu("Count Up Settings", "countUpSettings")
                    .AddLabel("Scoring Mode")
                    .AddOption<string>(
                        COUNT_UP_SCORING_MODE,
                        Enum.GetNames(typeof(ScoringMode)).ToArray()[0],
                        Enum.GetNames(typeof(ScoringMode)).ToArray(),
                        Enum.GetNames(typeof(ScoringMode)).ToArray())
                    .AddLabel("Target Coins")
                    .AddOption<int>(
                        COUNT_UP_TARGET_ID,
                        0,
                        countUpTargets,
                        countUpTargetStrings)
                    .AddLabel("Random Fire Interval Median")
                    .AddOption<int>(
                        COUNT_UP_RANDOM_FIRE_INTERVAL_ID,
                        -1,
                        fireIntervals,
                        fireIntervalStrings)
                    .AddSpacer()
                    .AddSpacer()
                .SubmenuDone()
                
                .AddSubmenu("Count Down Settings", "countDownSettings")
                    .AddLabel("Starting Time")
                    .AddOption<int>(
                        COUNT_DOWN_STARTING_SCORE_ID,
                        720,
                        startingScores,
                        startingScoreStrings)
                    .AddLabel("Base Decay Speed")
                    .AddOption<int>(
                        COUNT_DOWN_BASE_DECAY_SPEED_ID,
                        60,
                        baseDecaySpeeds,
                        baseDecaySpeedStrings)
                    .AddLabel("Item Delivered Recovery")
                    .AddOption<int>(
                        COUNT_DOWN_ITEM_DELIVERED_RECOVERY_ID,
                        20,
                        recoveries,
                        recoveryStrings)
                    .AddLabel("Group Leaving Recovery")
                    .AddOption<int>(
                        COUNT_DOWN_GROUP_LEAVING_RECOVERY_ID,
                        300,
                        recoveries,
                        recoveryStrings)
                    .AddLabel("Proximity Multiplier")
                    .AddOption<int>(
                        COUNT_DOWN_PROXIMITY_MULTIPLIER_ID,
                        200,
                        proximityMultipliers,
                        proximityMultiplierStrings)
                    .AddLabel("Last Day")
                    .AddOption<int>(
                        COUNT_DOWN_LAST_DAY_ID,
                        15,
                        lastDays,
                        lastDayStrings)
                    .AddLabel("Random Fire Interval Median")
                    .AddOption<int>(
                        COUNT_DOWN_RANDOM_FIRE_INTERVAL_ID,
                        -1,
                        fireIntervals,
                        fireIntervalStrings)
                    .AddSpacer()
                    .AddSpacer()
                .SubmenuDone()
                .AddSpacer()
                .AddSubmenu("Auto Cards", "autoCards")
                    .AddLabel("Tipping Culture")
                    .AddOption<bool>(
                        AUTO_CARDS_TIPPING_CULTURE,
                        false,
                        new bool[] { false, true },
                        new string[] { "Disabled", "Enabled" })
                    .AddLabel("Instant Service")
                    .AddOption<bool>(
                        AUTO_CARDS_INSTANT_SERVICE,
                        false,
                        new bool[] { false, true },
                        new string[] { "Disabled", "Enabled" })
                    .AddSpacer()
                    .AddSpacer()
                .SubmenuDone()
                .AddSubmenu("Give Appliances", "giveAppliances")
                    .AddLabel("Fire Extinguisher")
                    .AddOption<bool>(
                        GIVE_APPLIANCES_FIRE_EXTINGUISHER,
                        false,
                        new bool[] { false, true },
                        new string[] { "Disabled", "Enabled" })
                    .AddSpacer()
                    .AddSpacer()
                .SubmenuDone()
                .AddSubmenu("Money Modifiers", "moneyModifiers")
                    .AddLabel("Private Dining")
                    .AddOption<bool>(
                        MONEY_MODIFIERS_PRIVATE_DINING,
                        false,
                        new bool[] { false, true },
                        new string[] { "Disabled", "Enabled" })
                    .AddLabel("Charity Donation")
                    .AddOption<float>(
                        MONEY_MODIFIERS_CHARITY_DONATION,
                        0f,
                        new float[] { 0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f },
                        new string[] { "Disabled", "5%", "10%", "15%", "20%", "25%", "30%", "35%", "40%", "45%", "50%", "55%", "60%", "65%", "70%", "75%", "80%", "85%", "90%", "95%", "100%" })
                    .AddSpacer()
                    .AddSpacer()
                .SubmenuDone()
                .AddSubmenu("Display", "display")
                    .AddLabel("Show Decimals")
                    .AddOption<bool>(
                        SHOW_DECIMALS_ID,
                        false,
                        new bool[] { false, true },
                        new string[] { "Disabled", "Enabled" })
                    .AddSpacer()
                    .AddSpacer()
                .SubmenuDone()
                .AddSpacer()
                .AddButton("Reset", delegate (int _)
                {
                    ResetFireGame.Reset();
                })
                .AddSpacer()
                .AddSpacer();

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);

            // TODO: Uncomment the following if you have an asset bundle.
            // TODO: Also, make sure to set EnableAssetBundleDeploy to 'true' in your ModName.csproj

            // LogInfo("Attempting to load asset bundle...");
            // Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).First();
            // LogInfo("Done loading asset bundle.");

            // Register custom GDOs
            AddGameData();

            HashSet<int> addCatchFireOnFailureAppliances = new HashSet<int>()
            {
                ApplianceReferences.HobStarting,
                ApplianceReferences.Hob,
                ApplianceReferences.HobSafe,
                ApplianceReferences.Oven
            };

            HashSet<int> addFireImmuneAppliances = new HashSet<int>()
            {
                ApplianceReferences.Nameplate,
                ApplianceReferences.WheelieBin
            };

            // Perform actions when game data is built
            Events.BuildGameDataEvent += delegate (object s, BuildGameDataEventArgs args)
            {
                foreach (int applianceID in addCatchFireOnFailureAppliances)
                {
                    if (!args.gamedata.TryGet(applianceID, out Appliance appliance, warn_if_fail: true))
                        continue;
                    if (appliance.Properties.Select(x => x.GetType()).Contains(typeof(CCatchFireOnFailure)))
                        continue;
                    appliance.Properties.Add(new CCatchFireOnFailure());
                }

                foreach (int applianceID in addFireImmuneAppliances)
                {
                    if (!args.gamedata.TryGet(applianceID, out Appliance appliance, warn_if_fail: true))
                        continue;
                    if (appliance.Properties.Select(x => x.GetType()).Contains(typeof(CFireImmune)))
                        continue;
                    appliance.Properties.Add(new CFireImmune());
                }

                //if (args.gamedata.TryGet(ApplianceReferences.HobDanger, out Appliance dangerHob))
                //{
                //    dangerHob.ShoppingTags |= ShoppingTags.Basic;
                //}
            };
        }
        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
