using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CreateAR.Commons.Unity.Logging;
using LightJson;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Logs metrics to a file.
    /// </summary>
    public class FileMetricsTarget : IMetricsTarget
    {
        /// <summary>
        /// The type used to configure this.
        /// </summary>
        public const string TYPE = "json";

        /// <summary>
        /// Keeps a record of values.
        /// </summary>
        public class KeyRecord
        {
            /// <summary>
            /// Name of the key.
            /// </summary>
            public string Name;

            /// <summary>
            /// Min value.
            /// </summary>
            public float Min;

            /// <summary>
            /// Max value.
            /// </summary>
            public float Max;

            /// <summary>
            /// Average value.
            /// </summary>
            public float Ave;

            /// <summary>
            /// Raw values.
            /// </summary>
            public float[] Values = new float[0];

            /// <summary>
            /// Creates a new record.
            /// </summary>
            /// <param name="name">The name of the key.</param>
            /// <param name="initialValue">The value to start with.</param>
            public KeyRecord(string name, float initialValue)
            {
                Name = name;
                
                AddValue(initialValue);
            }

            /// <summary>
            /// Adds a value.
            /// </summary>
            /// <param name="value">The value.</param>
            public void AddValue(float value)
            {
                Values = Values.Add(value);

                Min = float.MaxValue;
                Max = float.MinValue;

                var sum = 0f;
                for (int i = 0, len = Values.Length; i < len; i++)
                {
                    var val = Values[i];
                    if (val < Min)
                    {
                        Min = val;
                    }

                    if (val > Max)
                    {
                        Max = val;
                    }

                    sum += val;
                }

                Ave = sum / Values.Length;
            }
        }

        /// <summary>
        /// For Json encoding.
        /// </summary>
        public class RecordAccumulator
        {
            /// <summary>
            /// Records.
            /// </summary>
            public KeyRecord[] Records;
        }

        /// <summary>
        /// Records.
        /// </summary>
        private readonly List<KeyRecord> _records = new List<KeyRecord>();

        /// <summary>
        /// Path to write to.
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FileMetricsTarget()
        {
            var dir = Path.Combine(
                UnityEngine.Application.persistentDataPath,
                "Metrics");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _path = Path.Combine(
                dir,
                string.Format("Metrics-{0:MM-dd-yyy.HH.mm.ss.fff}.json", DateTime.Now));
        }

        /// <inheritdoc />
        public void Send(string key, float value)
        {
            KeyRecord record = null;

            var found = false;
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                record = _records[i];
                if (record.Name == key)
                {
                    found = true;
                    record.AddValue(value);

                    break;
                }
            }

            if (!found)
            {
                record = new KeyRecord(key, value);
                _records.Add(record);
            }
            
            Flush();
        }

        /// <summary>
        /// Writes to disk.
        /// </summary>
        private void Flush()
        {
            var value = new JsonObject(new RecordAccumulator
            {
                Records = _records.ToArray()
            }).ToString(true);

            File.WriteAllText(_path, value);
        }
    }
}