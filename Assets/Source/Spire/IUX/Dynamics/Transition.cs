using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Applies an alpha transition to elements added + removed and set to visible.
    /// </summary>
    public class Transition : Element, IUnityElement
    {
        /// <summary>
        /// Internal record-keeping.
        /// </summary>
        private class TweenRecord
        {
            /// <summary>
            /// The element in question.
            /// </summary>
            public readonly Element Element;

            /// <summary>
            /// The cached visible property.
            /// </summary>
            public readonly ElementSchemaProp<bool> Visible;

            /// <summary>
            /// True iff the element should be tweening.
            /// </summary>
            public bool IsTweening;

            /// <summary>
            /// Start time of the tween.
            /// </summary>
            public DateTime StartTime;

            /// <summary>
            /// Start value.
            /// </summary>
            public float Start;

            /// <summary>
            /// End value.
            /// </summary>
            public float End;
            
            /// <summary>
            /// Tween duration, in seconds.
            /// </summary>
            public float Duration;

            /// <summary>
            /// True iff we want to invert the tween.
            /// </summary>
            public bool Invert;

            /// <summary>
            /// True iff visibility has changed.
            /// </summary>
            public bool IsDirty { get; private set; }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="element">The element in question.</param>
            public TweenRecord(Element element)
            {
                Element = element;
                Visible = Element.Schema.Get<bool>("visible");
                Visible.OnChanged += Visible_OnChanged;

                IsDirty = Visible.Value;
            }

            /// <summary>
            /// Marks as not-dirty.
            /// </summary>
            public void Read()
            {
                IsDirty = false;
            }

            /// <summary>
            /// Called when visibility has changed.
            /// </summary>
            private void Visible_OnChanged(
                ElementSchemaProp<bool> element,
                bool prev,
                bool next)
            {
                IsDirty = true;
            }
        }

        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly TweenConfig _tweens;

        /// <summary>
        /// A record for each element that has been added.
        /// </summary>
        private readonly List<TweenRecord> _records = new List<TweenRecord>();

        /// <summary>
        /// Prop that tells what prop name to adjust.
        /// </summary>
        private ElementSchemaProp<string> _propNameProp;

        /// <summary>
        /// Prop with start value.
        /// </summary>
        private ElementSchemaProp<float> _startValueProp;

        /// <summary>
        /// Prop with end value.
        /// </summary>
        private ElementSchemaProp<float> _endValueProp;

        /// <summary>
        /// Name of the tween.
        /// </summary>
        private ElementSchemaProp<string> _tweenProp;

        /// <summary>
        /// Visibility.
        /// </summary>
        private ElementSchemaProp<bool> _visibleProp;

        /// <summary>
        /// The GameObject.
        /// </summary>
        public GameObject GameObject { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Transition(GameObject gameObject, TweenConfig tweens)
        {
            GameObject = gameObject;

            _tweens = tweens;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("<{0} Id={1} Guid={2} />",
                GetType().Name,
                Id,
                Guid);
        }

        /// <inheritdoc />
        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();

            _propNameProp = Schema.GetOwn("prop", "alpha");
            _startValueProp = Schema.GetOwn("start", 0f);
            _endValueProp = Schema.GetOwn("end", 1f);
            _tweenProp = Schema.GetOwn("tween", TweenType.Pronounced.ToString());
            _visibleProp = Schema.GetOwn("visible", true);
            _visibleProp.OnChanged += Visible_OnChanged;

            GameObject.name = ToString();
        }

        /// <inheritdoc />
        protected override void LoadInternalAfterChildren()
        {
            base.LoadInternalAfterChildren();

            // starting invisible
            if (!_visibleProp.Value)
            {
                var prop = _propNameProp.Value;
                var value = _startValueProp.Value;
                for (int i = 0, len = _records.Count; i < len; i++)
                {
                    var record = _records[i];
                    record.Read();

                    // snap
                    record.Element.Schema.Set(prop, value);
                }

                GameObject.SetActive(false);
            }
        }

        /// <inheritdoc />
        protected override void UnloadInternalAfterChildren()
        {
            base.UnloadInternalAfterChildren();

            _visibleProp.OnChanged -= Visible_OnChanged;
        }

        /// <inheritdoc />
        protected override void AddChildInternal(Element element)
        {
            base.AddChildInternal(element);

            var child = element as IUnityElement;
            if (null != child)
            {
                child.GameObject.transform.SetParent(
                    GameObject.transform,
                    false);
            }

            // set initial value
            element.Schema.Set(
                _propNameProp.Value,
                _visibleProp.Value
                    ? _startValueProp.Value
                    : _endValueProp.Value);

            _records.Add(new TweenRecord(element));
        }

        /// <inheritdoc />
        protected override void RemoveChildInternal(Element element)
        {
            base.RemoveChildInternal(element);

            for (var i = _records.Count - 1; i >= 0; i--)
            {
                if (_records[i].Element == element)
                {
                    _records.RemoveAt(i);
                }
            }
        }

        /// <inheritdoc />
        protected override void LateUpdateInternal()
        {
            base.LateUpdateInternal();

            var isTweening = false;
            var now = DateTime.Now;
            for (var i = 0; i < _records.Count; i++)
            {
                var record = _records[i];

                if (record.IsDirty)
                {
                    record.Read();

                    // this element has turned visible
                    if (record.Visible.Value)
                    {
                        StartTween(record);
                    }
                }

                // update tween
                if (record.IsTweening)
                {
                    var propName = _propNameProp.Value;
                    var elapsedSeconds = (float) now.Subtract(record.StartTime).TotalSeconds;
                    var t = elapsedSeconds / record.Duration;
                    if (t > 1)
                    {
                        record.Element.Schema.Set(
                            propName,
                            record.Invert
                                ? record.Start
                                : record.End);
                        record.IsTweening = false;
                    }
                    else
                    {
                        if (record.Invert)
                        {
                            t = 1f - t;
                        }

                        var value = record.Start + t * (record.End - record.Start);

                        record.Element.Schema.Set(propName, value);

                        isTweening = true;
                    }
                }
            }

            // all tweens are done
            if (!isTweening)
            {
                GameObject.SetActive(_visibleProp.Value);
            }
            else
            {
                GameObject.SetActive(true);
            }
        }

        /// <inheritdoc />
        protected override void DestroyInternal()
        {
            base.DestroyInternal();

            UnityEngine.Object.Destroy(GameObject);
            GameObject = null;
        }

        /// <summary>
        /// Starts tweening a record.
        /// </summary>
        /// <param name="record">The record to tween.</param>
        /// <param name="invert">If true, inverts start and end.</param>
        private void StartTween(TweenRecord record, bool invert = false)
        {
            var start = _startValueProp.Value;
            var end = _endValueProp.Value;

            record.IsTweening = true;
            record.Duration = _tweens.DurationSeconds(_tweenProp.Value.ToEnum<TweenType>());
            record.StartTime = DateTime.Now;
            record.Invert = invert;
            record.Start = start;
            record.End = end;

            record.Element.Schema.Set(_propNameProp.Value, record.Start);
        }

        /// <summary>
        /// Called when visibility changes.
        /// </summary>
        private void Visible_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            // adjust all tween records
            for (int j = 0, jlen = _records.Count; j < jlen; j++)
            {
                StartTween(_records[j], !next);
            }
        }
    }
}