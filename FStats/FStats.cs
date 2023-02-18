using FStats.GlobalStats;
using Modding;
using System;
using System.Collections.Generic;

namespace FStats
{
    public class FStatsMod : Mod, ILocalSettings<LocalSettings>, IGlobalSettings<GlobalSettings>, IMenuMod
    {
        internal static FStatsMod instance;

        public static LocalSettings LS = new();
        public LocalSettings OnSaveLocal() => LS;
        public void OnLoadLocal(LocalSettings ls)
        {
            GlobalStats?.Unload();
            LS?.Unload();
            LS = ls;
        }

        public static GlobalSettings GS = new();
        public static GlobalStatManager GlobalStats { get; set; }
        public GlobalSettings OnSaveGlobal()
        {
            try
            {
                GlobalStatSerialization.Save(GlobalStats);
            }
            catch (Exception e)
            {
                LogError("Error saving global stats: " + e);
            }

            GlobalSettings gs = new();
            gs.LoadFrom(GS);
            if (gs.PreventSavingGlobalStats == SettingType.ThisSession)
            {
                gs.PreventSavingGlobalStats = SettingType.Never;
            }
            return gs;
        }
        public void OnLoadGlobal(GlobalSettings gs)
        {
            GS.LoadFrom(gs);

            try
            {
                GlobalStats = GlobalStatSerialization.Load();
            }
            catch(Exception e)
            {
                LogError("Error loading global stats: " + e);
                GlobalStats = null;
            }
        }

        bool IMenuMod.ToggleButtonInsideMenu => false;
        List<IMenuMod.MenuEntry> IMenuMod.GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry) => ModMenu.GetMenuData();

        static FStatsMod()
        {
            // Load during the static constructor so it is available during deserialization
            AreaName.LoadData();
        }

        public FStatsMod() : base(null)
        {
            instance = this;
        }
        
        public override string GetVersion()
        {
            return Vasi.VersionUtil.GetVersion<FStatsMod>();
        }

        public override int LoadPriority() => 3;

        public override void Initialize()
        {
            Log("Initializing Mod...");

            On.GameManager.StartNewGame += GameManager_StartNewGame;
            On.GameManager.ContinueGame += GameManager_ContinueGame;
            ModHooks.NewGameHook += ModHooks_NewGameHook;
            EndScreen.EndScreenManager.Hook();

            RegisterInternalGlobalStats();
            if (GlobalStats is not null)
            {
                ModHooks.FinishedLoadingModsHook += () => GlobalStats.AddGlobalStats();
            }
        }

        private void RegisterInternalGlobalStats()
        {
            API.RegisterGlobalStat<StatControllers.Common>();
            API.RegisterGlobalStat<StatControllers.TimeByAreaStat>();
            API.RegisterGlobalStat<StatControllers.ModConditional.ICChecksDisplay>();
            API.RegisterGlobalStat<StatControllers.ModConditional.ICChecksPerMinuteDisplay>();
            API.RegisterGlobalStat<StatControllers.ModConditional.ItemSyncData>();
            API.RegisterGlobalStat<StatControllers.HeroActionStats>();
            API.RegisterGlobalStat<StatControllers.DirectionalStats>();
            API.RegisterGlobalStat<StatControllers.CombatStats>();
            API.RegisterGlobalStat<StatControllers.TransitionStats>();
            API.RegisterGlobalStat<StatControllers.GlobalSkillTimeline>();
            API.RegisterGlobalStat<StatControllers.ModConditional.BenchwarpStats>();
            API.RegisterGlobalStat<StatControllers.MiscStats>();
        }

        /// <summary>
        /// During stat controller initialization, this will be set to true if it is a new game and false otherwise.
        /// At other times this will be null.
        /// </summary>
        public static InitializationState? InitializationState { get; private set; }

        private void StartStats(bool newGame)
        {
            if (LS is null) return;
            if (LocalSettings.Loaded)
            {
                LogDebug("Not starting stats: Local Settings already loaded");
                return;
            }

            InitializationState = newGame ? FStats.InitializationState.NewGame : FStats.InitializationState.ExistingGame;

            try
            {
                LS.Initialize(newGame);
                if (newGame)
                {
                    List<string> loadedStats = GlobalStats?.InitializeAll() ?? new();
                    LS.ActiveGlobalStats = loadedStats;
                }
                else
                {
                    List<string> associated = LS.ActiveGlobalStats ?? new();
                    List<string> active = GlobalStats?.Initialize(associated) ?? new();

                    LS.ActiveGlobalStats = active;
                }
            }
            catch (Exception e)
            {
                LogError("Error initializing stat controllers:\n" + e);
            }
            finally
            {
                InitializationState = null;
            }
        }


        private void ModHooks_NewGameHook()
        {
            // Called by ItemChanger on new game with a changed start
            StartStats(true);
        }

        private void GameManager_ContinueGame(On.GameManager.orig_ContinueGame orig, GameManager self)
        {
            orig(self);

            StartStats(false);
        }

        private void GameManager_StartNewGame(On.GameManager.orig_StartNewGame orig, GameManager self, bool permadeathMode, bool bossRushMode)
        {
            orig(self, permadeathMode, bossRushMode);

            StartStats(true);
        }
    }
}