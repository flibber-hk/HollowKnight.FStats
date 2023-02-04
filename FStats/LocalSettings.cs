using FStats.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FStats
{
    public class LocalSettings : IStatCollection
    {
        /// <summary>
        /// Whether these local settings saw the save file being created.
        /// </summary>
        public bool InitializedOnNewGame { get; set; }

        /// <summary>
        /// The names of Global Stat Controllers associated with this save file.
        /// </summary>
        public List<string> ActiveGlobalStats { get; set; }

        internal static bool Loaded { get; private set; } = false;

        public List<StatController> Data;

        IEnumerable<StatController> IStatCollection.EnumerateActiveStats()
        {
            foreach (StatController sc in Data ?? Enumerable.Empty<StatController>()) yield return sc;
        }


        private static List<StatController> GenerateStatControllers()
        {
            List<StatController> controllers = new()
            {
                new StatControllers.Common(),
                new StatControllers.TimeByAreaStat(),
                new StatControllers.ModConditional.ICChecksDisplay(),
                new StatControllers.ModConditional.ItemSyncData(),
                new StatControllers.ModConditional.ICChecksPerMinuteDisplay(),
                new StatControllers.HeroActionStats(),
                new StatControllers.DirectionalStats(),
                new StatControllers.CombatStats(),
                new StatControllers.TransitionStats(),
                new StatControllers.SkillTimeline(),
                new StatControllers.ModConditional.BenchwarpStats(),
                new StatControllers.MiscStats(),
                new StatControllers.ExtensionStats(),
            };

            controllers.AddRange(API.BuildAdditionalStats());

            return controllers;
        }

        public T Get<T>() where T : StatController
        {
            return Data?.OfType<T>().FirstOrDefault();
        }

        public void Initialize(bool newGame)
        {
            // We need to set up the stat controllers here because the LS are constructed during mod construction,
            // which is too early for API stat controllers.
            Data ??= GenerateStatControllers();

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

            foreach (StatController sc in Data ?? throw new InvalidOperationException("Unloading stats which have not been setup"))
            {
                sc.Unload();
            }

            Loaded = false;
        }
    }
}
