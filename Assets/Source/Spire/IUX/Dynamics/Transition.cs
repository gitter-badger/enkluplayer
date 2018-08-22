using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
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
        protected override void LoadInternalBeforeChildren()
        {
            base.LoadInternalBeforeChildren();

            _propNameProp = Schema.GetOwn("prop", "alpha");
            _startValueProp = Schema.GetOwn("start", 0f);
            _endValueProp = Schema.GetOwn("end", 1f);
            _tweenProp = Schema.GetOwn("tween", TweenType.Pronounced.ToString());
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

            var now = DateTime.Now;
            for (var i = 0; i < _records.Count; i++)
            {
                var record = _records[i];

                if (record.IsDirty)
                {
                    record.Read();

                    // turned visible
                    if (record.Visible.Value)
                    {
                        StartTween(record);
                    }
                    // turned invisible
                    else
                    {
                        //
                    }
                }

                // update tween
                if (record.IsTweening)
                {
                    var propName = _propNameProp.Value;
                    var t = (float)(now.Subtract(record.StartTime).TotalSeconds / record.Duration);
                    if (t > 1)
                    {
                        record.Element.Schema.Set(propName, record.End);
                        record.IsTweening = false;
                    }
                    else
                    {
                        record.Element.Schema.Set(
                            propName,
                            record.Start + (record.End - record.Start) * t);
                    }
                }
            }
        }

        /// <summary>
        /// Starts tweening a record.
        /// </summary>
        /// <param name="record">The record to tween.</param>
        private void StartTween(TweenRecord record)
        {
            var start = _startValueProp.Value;
            var end = _endValueProp.Value;

            record.Element.Schema.Set(_propNameProp.Value, start);

            record.IsTweening = true;
            record.Duration = _tweens.DurationSeconds(_tweenProp.Value.ToEnum<TweenType>());
            record.Start = start;
            record.End = end;
            record.StartTime = DateTime.Now;
        }
    }
}