using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
using UnityEngine;

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
        }

        public static GlobalSettings GS = new();
        public GlobalSettings OnSaveGlobal() => GS;
        public void OnLoadGlobal(GlobalSettings gs) => GS.LoadFrom(gs);

        public FStatsMod() : base(null)
        {
            instance = this;
            AreaName.LoadData();
        }
        
        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }

        public override int LoadPriority() => 3;

        public override void Initialize()
        {
            Log("Initializing Mod...");

            On.GameManager.StartNewGame += GameManager_StartNewGame;
            On.GameManager.ContinueGame += GameManager_ContinueGame;
            ModHooks.NewGameHook += ModHooks_NewGameHook;
            EndScreen.EndScreen.Hook();
        }

        private void ModHooks_NewGameHook()
        {
            // Called by ItemChanger on new game with a changed start
            LS.InitializedOnNewGame = true;
        }

        private void GameManager_ContinueGame(On.GameManager.orig_ContinueGame orig, GameManager self)
        {
            orig(self);
            LS?.Initialize(newGame: false);
        }

        private void GameManager_StartNewGame(On.GameManager.orig_StartNewGame orig, GameManager self, bool permadeathMode, bool bossRushMode)
        {
            orig(self, permadeathMode, bossRushMode);
            LS?.Initialize(newGame: true);
        }
    }
}