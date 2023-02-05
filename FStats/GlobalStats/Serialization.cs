using Modding.Patches;
using Modding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace FStats.GlobalStats
{
    /// <summary>
    /// Class to manage loading and unloading the global stat manager.
    /// </summary>
    internal static class GlobalStatSerialization
    {
        private static readonly ILogger _logger = new SimpleLogger("FStats.GlobalStats.Serialization");
        private static readonly string _globalStatsPath = Path.Combine(UnityEngine.Application.persistentDataPath, "FStats.GlobalStats.json");
        private static readonly string _globalStatsBackupPath = _globalStatsPath + ".bak";


        private static GlobalStatData SerializableData { get; set; } = null;


        public static GlobalStatManager Load()
        {
            GlobalStatData dat = LoadData();
            SerializableData = dat;

            Dictionary<string, StatController> stats = new();
            foreach ((string typeName, JToken token) in dat.Data)
            {
                if (Type.GetType(typeName) is not Type statControllerType) continue;

                if (token.ToObject(statControllerType) is not StatController sc)
                {
                    _logger.LogWarn($"Failed to deserialize stat controller of type {typeName}");
                    continue;
                }

                stats[typeName] = sc;
            }

            return new(stats);
        }

        private static GlobalStatData LoadData()
        {
            _logger.Log("Loading Global Stat Data");

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
                typeof(GlobalStatData),
                new JsonSerializerSettings
                {
                    ContractResolver = ShouldSerializeContractResolver.Instance,
                    TypeNameHandling = TypeNameHandling.Auto,
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    Converters = JsonConverterTypes.ConverterTypes
                }
            );

            if (obj is GlobalStatData dat) return dat ?? new();

            _logger.LogWarn("Could not load Global Stat Data from saves; creating new");
            return new();
        }


        public static void Save(GlobalStatManager mgr)
        {
            if (!FStatsMod.GS.TrackGlobalStats
                || (FStatsMod.GS.PreventSavingGlobalStats != SettingType.Never))
            {
                _logger.Log($"Not saving global stats because of global settings ({FStatsMod.GS.TrackGlobalStats}, {FStatsMod.GS.PreventSavingGlobalStats})");
                return;
            }


            _logger.Log("Saving Global Stats");
            foreach ((string typeName, StatController sc) in mgr.TrackedStats)
            {
                SerializableData.Data[typeName] = JToken.FromObject
                (
                    sc,
                    JsonSerializer.Create
                    (
                        new JsonSerializerSettings
                        {
                            ContractResolver = ShouldSerializeContractResolver.Instance,
                            TypeNameHandling = TypeNameHandling.Auto,
                            Converters = JsonConverterTypes.ConverterTypes
                        }
                    )
                );
            }


            if (File.Exists(_globalStatsPath))
            {
                if (File.Exists(_globalStatsBackupPath)) File.Delete(_globalStatsBackupPath);
                File.Move(_globalStatsPath, _globalStatsBackupPath);
            }

            using FileStream fileStream = File.Create(_globalStatsPath);
            using StreamWriter streamWriter = new(fileStream);

            streamWriter.Write(JsonConvert.SerializeObject(
                SerializableData,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    ContractResolver = ShouldSerializeContractResolver.Instance,
                    TypeNameHandling = TypeNameHandling.Auto,
                    Converters = JsonConverterTypes.ConverterTypes
                }));
        }
    }
}
