using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    public class UIReference
    {
        public string UIDataId;
    }

    public class UIData : StaticData
    {
        public string Uri;

        public PoolData Pooling;
    }

    public interface IUIElement
    {
        uint StackId { get; }

        void Created();
        void Added();
        void Revealed();
        void Covered();
        void Removed();
    }

    public interface IUIManager
    {
        IAsyncToken<IUIElement> Open(UIReference reference, out uint stackId);

        void Reveal(uint stackId);

        void Close(uint stackId);
    }

    public interface IUIElementFactory
    {
        IAsyncToken<IUIElement> Element(UIReference reference, uint id);
    }

    /// <summary>
    /// Basic implementation of IUIManager.
    /// </summary>
    public class UIManager : IUIManager
    {
        private class UIRecord
        {
            public uint StackId;
            public IAsyncToken<IUIElement> Load;
            public IUIElement Element;

            public UIRecord(uint stackId)
            {
                StackId = stackId;
            }
        }

        /// <summary>
        /// Id.
        /// </summary>
        private static uint IDS = 0;

        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IUIElementFactory _factory;

        /// <summary>
        /// Element stack.
        /// </summary>
        private readonly List<UIRecord> _records = new List<UIRecord>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public UIManager(IUIElementFactory factory)
        {
            _factory = factory;
        }

        public IAsyncToken<IUIElement> Open(UIReference reference, out uint stackId)
        {
            stackId = IDS++;

            // create record
            var record = new UIRecord(stackId);

            // start load immediately
            var load = _factory.Element(reference, stackId);
            
            // wait for previous load to complete
            var loads = _records.Count > 0
                ? new [] { _records.Last().Load, load }
                : new [] { load };
            record.Load = Async.Map(Async.All(loads), elements => elements.Last());

            // add record to the end
            _records.Add(record);

            return record
                .Load
                .OnSuccess(element =>
                {
                    record.Element = element;
                    element.Created();
                    element.Added();

                    // cover previous
                    var index = _records.IndexOf(record);
                    if (index > 0)
                    {
                        _records[index - 1].Element.Covered();
                    }

                    element.Revealed();
                })
                .Token();
        }

        public void Reveal(uint stackId)
        {
            // first, make sure element exists in stack at all
            var found = false;
            foreach (var record in _records)
            {
                if (record.StackId == stackId)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return;
            }

            // now run lifecycle events in order
            var depth = 0;
            while (_records.Count > 0)
            {
                var peek = _records.Last();
                if (peek.StackId == stackId)
                {
                    // make sure this isn't already on top and that the element is loaded
                    if (depth > 0 && null != peek.Element)
                    {
                        peek.Element.Revealed();
                    }

                    return;
                }

                _records.RemoveAt(_records.Count - 1);
                if (null != peek.Element)
                {
                    peek.Element.Removed();
                }

                depth++;
            }
        }

        public void Close(uint stackId)
        {
            // first, make sure element exists in stack at all
            var found = false;
            foreach (var record in _records)
            {
                if (record.StackId == stackId)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return;
            }

            // now run lifecycle events in order
            while (_records.Count > 0)
            {
                var record = _records[_records.Count - 1];
                _records.RemoveAt(_records.Count - 1);

                if (null != record.Element)
                {
                    record.Element.Removed();
                }

                if (record.StackId == stackId)
                {
                    return;
                }
            }
        }
    }
}