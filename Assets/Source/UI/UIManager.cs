﻿using System.Collections.Generic;
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
            public readonly uint StackId;

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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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