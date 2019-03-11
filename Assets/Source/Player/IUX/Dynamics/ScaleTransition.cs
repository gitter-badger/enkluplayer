using Enklu.Data;

namespace CreateAR.EnkluPlayer.IUX
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    /// <summary>
    /// Applies a scale transition to elements added + removed.
    /// </summary>
    public class ScaleTransition : Element, IUnityElement
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
            /// Start scale.
            /// </summary>
            public Vec3 Start;

            /// <summary>
            /// End scale.
            /// </summary>
            public Vec3 End;

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
            /// Marks as not-dirty/
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
        /// The GameObject.
        /// </summary>
        public GameObject GameObject { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ScaleTransition(GameObject gameObject, TweenConfig tweens)
        {
            GameObject = gameObject;

            _tweens = tweens;
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
                    var t = (float)(now.Subtract(record.StartTime).TotalSeconds / record.Duration);
                    if (t > 1)
                    {
                        record.Element.Schema.Set("scale", record.End);
                        record.IsTweening = false;
                    }
                    else
                    {
                        record.Element.Schema.Set(
                            "scale",
                            Vec3.Lerp(record.Start, record.End, t));
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
            var start = Vec3.Zero;

            record.Element.Schema.Set("scale", start);

            record.IsTweening = true;
            record.Duration = _tweens.DurationSeconds(TweenType.Responsive);
            record.Start = start;
            record.End = Vec3.One;
            record.StartTime = DateTime.Now;
        }
    }
}