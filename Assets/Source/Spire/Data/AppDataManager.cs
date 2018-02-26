using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Default implementation of <c>IAppDataManager</c>.
    /// </summary>
    public class AppDataManager : IAdminAppDataManager
    {
        /// <summary>
        /// Type to list of data.
        /// </summary>
        private readonly Dictionary<Type, List<StaticData>> _dataByType = new Dictionary<Type, List<StaticData>>();
        
        /// <inheritdoc cref="IAppDataManager"/>
        public event Action<StaticData> OnRemoved;

        /// <inheritdoc cref="IAppDataManager"/>
        public event Action<StaticData> OnUpdated;
        
        /// <inheritdoc cref="IAppDataManager"/>
        public T Get<T>(string id) where T : StaticData
        {
            var list = GetList<T>();
            foreach (var element in list)
            {
                if (element.Id == id)
                {
                    return (T) element;
                }
            }

            return null;
        }

        /// <inheritdoc cref="IAppDataManager"/>
        public T[] GetAll<T>() where T : StaticData
        {
            return GetList<T>().Cast<T>().ToArray();
        }

        /// <inheritdoc cref="IAppDataManager"/>
        public T GetByName<T>(string name) where T : StaticData
        {
            var list = GetList<T>();
            for (int i = 0, len = list.Count; i < len; i++)
            {
                if (list[i].Name == name)
                {
                    return (T) list[i];
                }
            }

            return null;
        }
        
        /// <inheritdoc cref="IAdminAppDataManager"/>
        public void Set<T>(params T[] data) where T : StaticData
        {
            var list = GetList<T>();
            list.Clear();

            list.AddRange(data);
        }

        /// <inheritdoc cref="IAdminAppDataManager"/>
        public void Add<T>(params T[] data) where T : StaticData
        {
            GetList<T>().AddRange(data);
        }

        /// <inheritdoc cref="IAdminAppDataManager"/>
        public void Remove<T>(params T[] data) where T : StaticData
        {
            var list = GetList<T>();
            for (var i = data.Length - 1; i >= 0; i--)
            {
                var datum = data[i];
                list.Remove(datum);

                if (null != OnRemoved)
                {
                    OnRemoved(datum);
                }
            }
        }

        /// <inheritdoc cref="IAdminAppDataManager"/>
        public void Update<T>(params T[] data) where T : StaticData
        {
            var list = GetList<T>();
            for (int i = 0, ilen = data.Length; i < ilen; i++)
            {
                var instance = data[i];

                var found = false;
                for (int j = 0, jlen = list.Count; j < jlen; j++)
                {
                    var existing = list[j];
                    if (instance.Id == existing.Id)
                    {
                        found = true;

                        list[j] = instance;

                        if (null != OnUpdated)
                        {
                            OnUpdated(instance);
                        }
                        break;
                    }
                }

                if (!found)
                {
                    Log.Error(this,
                        "Received update for unknown StaticData : {0}.",
                        instance);
                }
            }
        }
        /// <summary>
        /// Retrieves list for type.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns></returns>
        private List<StaticData> GetList<T>()
        {
            List<StaticData> list;
            if (!_dataByType.TryGetValue(typeof(T), out list))
            {
                list = _dataByType[typeof(T)] = new List<StaticData>();
            }

            return list;
        }
    }
}