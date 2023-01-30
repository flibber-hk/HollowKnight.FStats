using FStats.Interfaces;
using Modding.Patches;
using Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FStats
{
    [JsonConverter(typeof(InvalidStatConverter))]
    internal class InvalidStatHolder : StatController
    {
        public JToken JSON { get; init; }

        public class InvalidStatConverter : JsonConverter<InvalidStatHolder>
        {
            public override bool CanRead => false;
            public override bool CanWrite => true;

            public override InvalidStatHolder ReadJson(
                JsonReader reader, Type objectType, InvalidStatHolder existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, InvalidStatHolder value, JsonSerializer serializer)
            {
                value.JSON.WriteTo(writer);
            }
        }
    }

    public class SafeStatControllerListConverter : JsonConverter<List<StatController>>
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override List<StatController> ReadJson(
            JsonReader reader, Type objectType, List<StatController> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken jt = JToken.Load(reader);
            if (jt.Type == JTokenType.Null) return null;
            else if (jt.Type == JTokenType.Array)
            {
                JArray ja = (JArray)jt;
                List<StatController> list = new();
                foreach (JToken jSc in ja)
                {
                    StatController sc;
                    try
                    {
                        sc = jSc.ToObject<StatController>(serializer);
                    }
                    catch (Exception)
                    {
                        sc = new InvalidStatHolder
                        {
                            JSON = jSc,
                        };
                    }
                    list.Add(sc);
                }
                return list;
            }

            throw new JsonSerializationException("Unable to deserialize Stat Controller list");
        }

        public override void WriteJson(JsonWriter writer, List<StatController> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class GlobalStatManager : IStatCollection
    {
        private static readonly ILogger _logger = new SimpleLogger("FStats.GlobalStatManager");

        private int _loadedCount;

        [JsonProperty]
        [JsonConverter(typeof(SafeStatControllerListConverter))]
        private List<StatController> TrackedStats { get; set; } = new();

        public T Get<T>() where T : StatController
        {
            return TrackedStats.OfType<T>().FirstOrDefault();
        }

        IEnumerable<StatController> IStatCollection.EnumerateActiveStats()
        {
            for (int i = 0; i < _loadedCount; i++)
            {
                yield return TrackedStats[i];
            }
        }

        internal void AddGlobalStats()
        {
            foreach ((Type t, Func<StatController> maker) in API.GlobalStatTypes)
            {
                if (TrackedStats.Any(x => x.GetType() == t)) continue;
                _logger.LogDebug($"Adding new {t.FullName}");
                TrackedStats.Add(maker());
            }
        }

        #region Loading
        private static readonly string _globalStatsPath = Path.Combine(UnityEngine.Application.persistentDataPath, "FStats.GlobalStats.json");
        private static readonly string _globalStatsBackupPath = _globalStatsPath + ".bak";

        internal static GlobalStatManager Load()
        {
            _logger.Log("Loading Global Stats");

            string currentStatsPath;

            if (File.Exists(_globalStatsPath)) currentStatsPath = _globalStatsPath;
            else if (File.Exists(_globalStatsBackupPath)) currentStatsPath = _globalStatsBackupPath;
            else
            {
                _logger.LogDebug("Creating new");
                return new();
            }

            string json;
            {
                using FileStream fileStream = File.OpenRead(currentStatsPath);
                using StreamReader reader = new(fileStream);
                json = reader.ReadToEnd();
            }

            object obj = JsonConvert.DeserializeObject(
                json,
                typeof(GlobalStatManager),
                new JsonSerializerSettings
                {
                    ContractResolver = ShouldSerializeContractResolver.Instance,
                    TypeNameHandling = TypeNameHandling.Auto,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    Converters = JsonConverterTypes.ConverterTypes
                }
            );

            if (obj is GlobalStatManager mgr) return mgr ?? new();

            _logger.LogWarn("Could not load Global Stat Manager from saves; creating new");
            return new();
        }

        internal static void Save(GlobalStatManager mgr) 
        {
            if (!FStatsMod.GS.TrackGlobalStats
                || FStatsMod.GS.PreventSavingGlobalStats)
            {
                _logger.Log($"Not saving global stats because of global settings ({FStatsMod.GS.TrackGlobalStats}, {FStatsMod.GS.PreventSavingGlobalStats})");
                return;
            }


            _logger.Log("Saving Global Stats");

            if (File.Exists(_globalStatsPath))
            {
                if (File.Exists(_globalStatsBackupPath)) File.Delete(_globalStatsBackupPath);
                File.Move(_globalStatsPath, _globalStatsBackupPath);
            }

            using FileStream fileStream = File.Create(_globalStatsPath);
            using StreamWriter streamWriter = new(fileStream);

            streamWriter.Write(JsonConvert.SerializeObject(
                mgr,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = ShouldSerializeContractResolver.Instance,
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = JsonConverterTypes.ConverterTypes
                }));
        }
        #endregion

        public void Initialize(int count)
        {
            if (!FStatsMod.GS.TrackGlobalStats) return;

            if (_loadedCount > 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                TrackedStats[i]?.Initialize();
            }
            _loadedCount = count;
        }

        public int InitializeAll()
        {
            if (!FStatsMod.GS.TrackGlobalStats) return 0;

            if (_loadedCount > 0)
            {
                return _loadedCount;
            }

            // Possible issue if they start the file, then install the dependency, then re-enter the file
            // but probably doesn't matter in most cases
            foreach (StatController controller in TrackedStats)
            {
                if (controller is null) continue;
                controller.Initialize();
                controller.FileCount += 1;
            }
            _loadedCount = TrackedStats.Count;
            return _loadedCount;
        }

        public void Unload()
        {
            for (int i = _loadedCount - 1; i >= 0; i--)
            {
                TrackedStats[i]?.Unload();
            }
            _loadedCount = 0;
        }

        public List<DisplayInfo> GenerateDisplays()
        {
            if (!FStatsMod.GS.TrackGlobalStats
                || !FStatsMod.GS.ShowGlobalStats)
            {
                return new();
            }

            List<DisplayInfo> infos = new();

            for (int i = 0; i < _loadedCount; i++)
            {
                StatController sc = TrackedStats[i];
                if (!FStatsMod.GS.ShouldDisplay(sc)) continue;

                foreach (DisplayInfo info in sc.GetGlobalDisplayInfos())
                {
                    infos.Add(info);
                }
            }

            foreach (DisplayInfo info in infos)
            {
                info.StatColumns = info.StatColumns?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList() ?? new();
            }

            infos = infos
                .Where(x => !string.IsNullOrEmpty(x.Title))
                .OrderBy(x => x.Priority)
                .ToList();

            API.FilterScreens(infos);

            return infos;
        }
    }
}
