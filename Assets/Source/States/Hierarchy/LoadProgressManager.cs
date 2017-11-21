using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.Assets;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Used to display load progress.
    /// </summary>
    public class LoadProgressManager : InjectableMonoBehaviour, ILoadProgressManager
    {
        /// <summary>
        /// Internal object used to represent a load.
        /// </summary>
        private class LoadProgressRecord
        {
            /// <summary>
            /// Unique id.
            /// </summary>
            public uint Id { get; private set; }

            /// <summary>
            /// Display.
            /// </summary>
            public LoadProgressBehaviour Behaviour { get; private set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            public LoadProgressRecord(
                uint id,
                LoadProgressBehaviour behaviour)
            {
                Id = id;
                Behaviour = behaviour;
            }
        }

        /// <summary>
        /// Unique id generator.
        /// </summary>
        private static uint _ids = 0;

        /// <summary>
        /// List of all records we are currently tracking.
        /// </summary>
        private readonly List<LoadProgressRecord> _records = new List<LoadProgressRecord>();

        /// <summary>
        /// Prefab for load progress indication.
        /// </summary>
        public LoadProgressBehaviour LoadProgressPrefab;

        /// <summary>
        /// Pools!
        /// </summary>
        [Inject]
        public IAssetPoolManager Pools { get; set; }

        /// <inheritdoc cref="ILoadProgressManager"/>
        public uint ShowIndicator(Vec3 min, Vec3 max, LoadProgress progress)
        {
            var behaviour = Pools.Get<LoadProgressBehaviour>(LoadProgressPrefab.gameObject);
            if (null == behaviour)
            {
                Log.Warning(this, "Could not create LoadProgressBehaviour.");
                return 0;
            }

            var bounds = Bounds(min, max);
            behaviour.Bounds = bounds;
            behaviour.Progress = progress;

            var record = new LoadProgressRecord(
                ++_ids,
                behaviour);

            _records.Add(record);

            return record.Id;
        }

        /// <inheritdoc cref="ILoadProgressManager"/>
        public void UpdateIndicator(uint id, Vec3 min, Vec3 max)
        {
            var record = Record(id);
            if (null != record)
            {
                record.Behaviour.Bounds = Bounds(min, max);
            }
        }

        /// <inheritdoc cref="ILoadProgressManager"/>
        public void HideIndicator(uint id)
        {
            var record = Record(id);
            if (null != record)
            {
                _records.Remove(record);

                Pools.Put(record.Behaviour.gameObject);
            }
        }

        /// <summary>
        /// Retrieves a record by id.
        /// </summary>
        /// <param name="id">The unique id of the record.</param>
        /// <returns>The matching record.</returns>
        private LoadProgressRecord Record(uint id)
        {
            for (int i = 0, len = _records.Count; i < len; i++)
            {
                var record = _records[i];
                if (record.Id == id)
                {
                    return record;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a Bounds object from Vec3 min/max.
        /// </summary>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Maximum.</param>
        /// <returns>The <c>Bounds</c> object.</returns>
        private static Bounds Bounds(Vec3 min, Vec3 max)
        {
            return new Bounds(
                new Vector3(
                    max.x - min.x,
                    max.y - min.y,
                    max.z - min.z) / 2f,
                new Vector3(
                    max.x - min.x,
                    max.y - min.y,
                    max.z - min.z));
        }
    }
}