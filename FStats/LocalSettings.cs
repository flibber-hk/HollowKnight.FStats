using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStats
{
    public class LocalSettings
    {
        /// <summary>
        /// Whether these local settings saw the save file being created
        /// </summary>
        public bool InitializedOnNewGame { get; set; }

        internal static bool Loaded = false;

        public List<StatController> Data;

        public LocalSettings()
        {
            Data = new()
            {
                new StatControllers.Common(),
                new StatControllers.TimeByAreaStat(),
                new StatControllers.ModConditional.ICChecksDisplay(),
                new StatControllers.ModConditional.ItemSyncData(),
                new StatControllers.ModConditional.ICChecksPerMinuteDisplay(),
                new StatControllers.HeroActionStats(),
                new StatControllers.DirectionalStats(),
                new StatControllers.CombatStats(),
                new StatControllers.SkillTimeline(),
                new StatControllers.ModConditional.BenchwarpStats(),
                new StatControllers.MiscStats(),
            };
        }

        public T Get<T>() where T : StatController
        {
            return Data.OfType<T>().FirstOrDefault();
        }

        public void Initialize(bool newGame)
        {
            if (Loaded)
            {
                FStatsMod.instance.LogError("Attempted to load local settings when an instance is already loaded");
                return;
            }

            FStatsMod.instance.Log($"Initializing LS - new game: {newGame}");
            if (newGame) InitializedOnNewGame = true;

            foreach (StatController sc in Data)
            {
                sc.Initialize();
            }

            Loaded = true;
        }
        public void Unload()
        {
            if (!Loaded) return;

            FStatsMod.instance.Log("Unloading LS");

            foreach (StatController sc in Data)
            {
                sc.Unload();
            }

            Loaded = false;
        }
    }
}
