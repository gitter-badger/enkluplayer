using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
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
        private readonly List<PropSet> _sets = new List<PropSet>();

        /// <inheritdoc />
        public ReadOnlyCollection<PropSet> Sets { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PropManager()
        {
            Sets = new ReadOnlyCollection<PropSet>(_sets);
        }

        /// <inheritdoc />
        public IAsyncToken<PropSet> Create()
        {
            return new AsyncToken<PropSet>(new NotImplementedException());
        }

        /// <inheritdoc />
        public IAsyncToken<PropSet> Destroy(string id)
        {
            return new AsyncToken<PropSet>(new NotImplementedException());
        }

        /// <inheritdoc />
        public IAsyncToken<PropSet> Load(string id)
        {
            return new AsyncToken<PropSet>(new NotImplementedException());
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Unload(string id)
        {
            return new AsyncToken<Void>(new NotImplementedException());
        }

        /// <inheritdoc />
        public IAsyncToken<Void> Save()
        {
            return new AsyncToken<Void>(new NotImplementedException());
        }

        /// <inheritdoc />
        public bool Add(PropData data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Remove(PropData data)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Update(PropData data)
        {
            throw new NotImplementedException();

            Save()
                .OnFailure(exception => Log.Warning(this, string.Format(
                    "Could not update {0} : {1}.",
                    data,
                    exception)));
        }
    }
}