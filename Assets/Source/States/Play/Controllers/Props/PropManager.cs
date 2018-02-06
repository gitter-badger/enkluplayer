using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Storage;
using CreateAR.SpirePlayer.IUX;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages PropData, WorldAnchorData, and creating PropControllers.
    /// 
    /// Content + PropData + PropController.
    /// </summary>
    public class PropManager : IPropManager, IPropSetUpdateDelegate, IPropUpdateDelegate
    {
        /// <summary>
        /// Handles storage needs.
        /// </summary>
        private readonly IStorageService _storage;

        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IElementFactory _elements;
        
        /// <summary>
        /// Backing variable for Sets prpoperty.
        /// </summary>
        private readonly List<PropSet> _sets = new List<PropSet>();

        /// <summary>
        /// PropData id to StorageBucket.
        /// </summary>
        private readonly Dictionary<string, StorageBucket> _props = new Dictionary<string, StorageBucket>();

        /// <summary>
        /// The current app id.
        /// </summary>
        private string _appId;

        /// <inheritdoc />
        public ReadOnlyCollection<PropSet> Sets { get; private set; }

        /// <inheritdoc />
        public PropSet Active { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PropManager(
            IStorageService storage,
            IElementFactory elements)
        {
            Sets = new ReadOnlyCollection<PropSet>(_sets);

            _storage = storage;
            _elements = elements;
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Initialize(string appId)
        {
            _appId = appId;

            var token = new AsyncToken<Void>();

            LogVerbose("Initialize().");
            LogVerbose("Refreshing list of all buckets.");
            
            // get a list of all our buckets
            _storage
                .Refresh()
                .OnSuccess(_ =>
                {
                    // get buckets for just our app
                    var props = _storage.FindAll(PropTag(_appId));

                    // load all PropData from buckets
                    var tokens = new List<IAsyncToken<PropData>>();
                    for (var i = 0; i < props.Length; i++)
                    {
                        tokens.Add(props[i].Value<PropData>());
                    }

                    // wait for all loads to complete
                    Async
                        .All(tokens.ToArray())
                        .OnSuccess(datas =>
                        {
                            LogVerbose("All buckets loaded.");

                            // keep lookup from prop id to bucket
                            for (var i = 0; i < datas.Length; i++)
                            {
                                _props[datas[i].Id] = props[i];
                            }

                            // create propsets of course
                            CreatePropSets(ParitionProps(datas, props));

                            // select a default propset
                            if (_sets.Count > 0)
                            {
                                Active = _sets[0];
                            }

                            token.Succeed(Void.Instance);
                        })
                        .OnFailure(token.Fail);
                })
                .OnFailure(token.Fail);
            
            return token;
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            _props.Clear();
        }

        /// <inheritdoc />
        public IAsyncToken<PropSet> Create()
        {
            // create an empty propset
            var propSet = new PropSet(
                _elements,
                this, this,
                Guid.NewGuid().ToString(),
                new PropData[0]);

            _sets.Add(propSet);

            return new AsyncToken<PropSet>(propSet);
        }

        /// <inheritdoc />
        public IAsyncToken<PropSet> Destroy(string id)
        {
            var set = ById(id);
            if (null == set)
            {
                return new AsyncToken<PropSet>(new Exception(string.Format(
                    "Could not find PropSet with id {0}.",
                    id)));
            }

            var tokens = new List<IAsyncToken<Void>>();
            var props = set.Props.ToArray();
            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];

                tokens.Add(set.Destroy(prop.Data.Id));
            }

            _sets.Remove(set);

            return Async.Map(
                Async.All(tokens.ToArray()),
                _ => set);
        }
        
        /// <inheritdoc />
        public IAsyncToken<Void> Add(PropSet set, PropData data)
        {
            var id = data.Id;
            if (_props.ContainsKey(id))
            {
                return new AsyncToken<Void>(new Exception(string.Format(
                    "PropData with id {0} already exists.",
                    id)));
            }

            return Async.Map(
                _storage.Create(data, PropTag(_appId)),
                bucket =>
                {
                    _props[id] = bucket;

                    return Void.Instance;
                });
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Remove(PropSet set, PropData data)
        {
            var id = data.Id;

            StorageBucket bucket;
            if (!_props.TryGetValue(id, out bucket))
            {
                return new AsyncToken<Void>(new Exception(string.Format(
                    "Could not find storage bucket for {0}.",
                    data)));
            }

            _props.Remove(id);

            return bucket
                .Delete()
                .OnFailure(exception => Log.Error(this, string.Format(
                    "Could not delete bucket {0} : {1}.",
                    id,
                    exception)));
        }

        /// <inheritdoc />
        public void Update(PropData data)
        {
            var id = data.Id;

            StorageBucket bucket;
            if (!_props.TryGetValue(id, out bucket))
            {
                Log.Error(this, string.Format(
                    "Could not find storage bucket to update {0}.",
                    data));

                return;
            }

            bucket
                .Save(data)
                .OnFailure(exception =>
                {
                    Log.Error(this, string.Format(
                        "Could not save bucket {0} : {1}.",
                        id,
                        exception));

                    // TODO: refresh specific bucket, not all buckets
                });
        }

        /// <summary>
        /// Creates <c>PropSet</c> instances from props.
        /// </summary>
        /// <param name="propSetToProps">Lookup from <c>PropSet</c> id to list of <c>PropData</c>.</param>
        private void CreatePropSets(Dictionary<string, List<PropData>> propSetToProps)
        {
            foreach (var pair in propSetToProps)
            {
                var id = pair.Key;
                var data = pair.Value.ToArray();

                _sets.Add(new PropSet(
                    _elements, this, this,
                    id,
                    data));
            }
        }

        /// <summary>
        /// Returns a dictionary from PropSet id -> PropData.
        /// </summary>
        /// <param name="datas">The prop data.</param>
        /// <param name="props">The prop buckets.</param>
        /// <returns></returns>
        private Dictionary<string, List<PropData>> ParitionProps(
            PropData[] datas,
            StorageBucket[] props)
        {
            var propSetToProps = new Dictionary<string, List<PropData>>();
            for (var i = 0; i < datas.Length; i++)
            {
                var prop = props[i];
                var data = datas[i];

                var substrings = prop.Key.Split('.');
                if (3 != substrings.Length)
                {
                    Log.Error(this,
                        "Invalid prop StorageBucket, malformed tag : {0}.",
                        prop.Key);
                    continue;
                }

                var propSetId = substrings[2];
                List<PropData> list;
                if (!propSetToProps.TryGetValue(propSetId, out list))
                {
                    list = propSetToProps[propSetId] = new List<PropData>();
                }

                list.Add(data);
            }

            return propSetToProps;
        }

        /// <summary>
        /// Returns a <c>PropSet</c> by id.
        /// </summary>
        /// <param name="id">The id of the set.</param>
        /// <returns></returns>
        private PropSet ById(string id)
        {
            for (int i = 0, len = _sets.Count; i < len; i++)
            {
                var set = _sets[i];
                if (set.Id == id)
                {
                    return set;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the tag for all props in an app.
        /// </summary>
        /// <param name="appId">The specific app id.</param>
        /// <returns></returns>
        private static string PropTag(string appId)
        {
            return appId + ".propset";
        }

        /// <summary>
        /// Logging.
        /// </summary>
        [Conditional("VERBOSE_LOGGING")]
        private void LogVerbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}