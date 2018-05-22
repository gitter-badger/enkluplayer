using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Basic implementation of IUIManager.
    /// </summary>
    public class UIManager : IUIManager
    {
        /// <summary>
        /// Internal record of UI element.
        /// </summary>
        private class UIRecord
        {
            /// <summary>
            /// Id in stack.
            /// </summary>
            public readonly int StackId;

            /// <summary>
            /// Load.
            /// </summary>
            public IAsyncToken<IUIElement> Load;

            /// <summary>
            /// Element: null until Load completes.
            /// </summary>
            public IUIElement Element;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="stackId">Stack id.</param>
            public UIRecord(int stackId)
            {
                StackId = stackId;
            }
        }

        /// <summary>
        /// Id.
        /// </summary>
        private static int IDS;

        /// <summary>
        /// Creates elements.
        /// </summary>
        private readonly IUIElementFactory _factory;

        /// <summary>
        /// Element stack.
        /// </summary>
        private readonly List<UIRecord> _records = new List<UIRecord>();
        
        /// <inheritdoc />
        public event Action<int> OnPush;
        
        /// <inheritdoc />
        public event Action OnPop;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="factory">The factory.</param>
        public UIManager(IUIElementFactory factory)
        {
            _factory = factory;
        }

        /// <inheritdoc />
        public IAsyncToken<T> Open<T>(UIReference reference) where T : IUIElement
        {
            int _;
            return Open<T>(reference, out _);
        }

        /// <inheritdoc />
        public IAsyncToken<T> Open<T>(UIReference reference, out int stackId) where T : IUIElement
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
            
            if (null != OnPush)
            {
                OnPush(stackId);
            }

            return Async.Map(
                record
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
                    }),
                element => (T) element);
        }

        /// <inheritdoc />
        public IAsyncToken<T> Replace<T>(UIReference reference) where T : IUIElement
        {
            Pop();

            return Open<T>(reference);
        }
        
        /// <inheritdoc />
        public bool Reveal(int stackId)
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
                return false;
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

                    return true;
                }

                _records.RemoveAt(_records.Count - 1);
                
                if (null != OnPop)
                {
                    OnPop();
                }

                peek.Load.Abort();

                if (null != peek.Element)
                {
                    peek.Element.Removed();
                }

                depth++;
            }

            return true;
        }

        /// <inheritdoc />
        public bool Close(int stackId)
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
                return false;
            }

            // now run lifecycle events in order
            while (_records.Count > 0)
            {
                var record = _records[_records.Count - 1];
                _records.RemoveAt(_records.Count - 1);
                
                if (null != OnPop)
                {
                    OnPop();
                }

                record.Load.Abort();

                if (null != record.Element)
                {
                    record.Element.Removed();
                }

                if (record.StackId == stackId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        public int Pop()
        {
            if (0 == _records.Count)
            {
                return -1;
            }

            var record = _records[_records.Count - 1];
            _records.RemoveAt(_records.Count - 1);
            
            if (null != OnPop)
            {
                OnPop();
            }

            record.Load.Abort();

            if (null != record.Element)
            {
                record.Element.Removed();
            }

            return record.StackId;
        }

        /// <inheritdoc />
        public UIManagerFrame CreateFrame()
        {
            return new UIManagerFrame(this);
        }
    }
}