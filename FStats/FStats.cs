using Modding;
using System;

namespace FStats
{
    public class FStatsMod : Mod, ILocalSettings<LocalSettings>, IGlobalSettings<GlobalSettings>
    {
        internal static FStatsMod instance;

        public static LocalSettings LS = new();
        public LocalSettings OnSaveLocal() => LS;
        public void OnLoadLocal(LocalSettings ls)
        {
            LS?.Unload();
            LS = ls;
            GlobalStats?.Unload();
        }

        public static GlobalSettings GS = new();
        public static GlobalStatManager GlobalStats { get; set; }
        public GlobalSettings OnSaveGlobal()
        {
            try
            {
                GlobalStatManager.Save(GlobalStats);
            }
            catch (Exception e)
            {
                LogError("Error saving global stats" + e);
            }

            return GS;
        }
        public void OnLoadGlobal(GlobalSettings gs)
        {
            GS.LoadFrom(gs);
            GlobalStats = GlobalStatManager.Load();
        }

        public FStatsMod() : base(null)
        {
            instance = this;
            AreaName.LoadData();
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
            ModHooks.FinishedLoadingModsHook += () => GlobalStats.AddGlobalStats();
        }

        private void RegisterInternalGlobalStats()
        {
            API.RegisterGlobalStat<StatControllers.Common>();
            API.RegisterGlobalStat<StatControllers.TimeByAreaStat>();
            API.RegisterGlobalStat<StatControllers.HeroActionStats>();
            API.RegisterGlobalStat<StatControllers.DirectionalStats>();
            API.RegisterGlobalStat<StatControllers.CombatStats>();
            API.RegisterGlobalStat<StatControllers.TransitionStats>();
            API.RegisterGlobalStat<StatControllers.ModConditional.BenchwarpStats>();
            API.RegisterGlobalStat<StatControllers.MiscStats>();
        }

        private void StartStats(bool newGame)
        {
            if (LS is null) return;
            if (LocalSettings.Loaded)
            {
                LogDebug("Not starting stats: Local Settings already loaded");
                return;
            }

            LS.Initialize(newGame);
            if (newGame)
            {
                int count = GlobalStats?.InitializeAll() ?? 0;
                LS.GlobalStatControllerCount = count;
            }
            else
            {
                int count = LS.GlobalStatControllerCount;
                GlobalStats?.Initialize(count);
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