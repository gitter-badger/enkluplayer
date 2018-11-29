using System;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Util
{
    /// <summary>
    /// Base class for all Tweens.
    /// </summary>
    public abstract class Tween
    {
        /// <summary>
        /// The schema to affect.
        /// </summary>
        public ElementSchema Schema { get; private set; }

        /// <summary>
        /// The tween data.
        /// </summary>
        public TweenData Data { get; private set; }
        
        /// <summary>
        /// Explicitly sets the time of a tween.
        /// </summary>
        public float Time
        {
            get { return _time; }
            set
            {
                var prev = _time;
                
                _time = value;

                // parameterized t
                var t = _ease(Mathf.Clamp((_time - Data.DelaySec) / Data.DurationSec, 0, 1));
                Update(t);

                // start callback
                if (Math.Abs(prev) < Mathf.Epsilon && _time > 0f)
                {
                    if (null != Data.OnStart)
                    {
                        Data.OnStart();
                    }
                }

                // completion callback
                if (Math.Abs(1f - t) < Mathf.Epsilon)
                {
                    if (!IsComplete)
                    {
                        if (null != Data.OnComplete)
                        {
                            Data.OnComplete();
                        }
                    }

                    IsComplete = true;
                }
                else
                {
                    IsComplete = false;
                }
            }
        }

        /// <summary>
        /// True iff the time is equal to or greater than the duration.
        /// </summary>
        public bool IsComplete { get; private set; }

        /// <summary>
        /// The easing function.
        /// </summary>
        protected readonly Func<float, float> _ease;

        /// <summary>
        /// The current time.
        /// </summary>
        private float _time;

        /// <summary>
        /// Constructor.
        /// </summary>
        protected Tween(ElementSchema schema, TweenData data)
        {
            Schema = schema;
            Data = data;

            _ease = TweenEasingEquations.Equation(Data.Easing);
        }

        /// <summary>
        /// Called with an evaluated parameter.
        /// </summary>
        /// <param name="time">The time.</param>
        protected abstract void Update(float time);
    }

    /// <summary>
    /// Tween for a float.
    /// </summary>
    public class FloatTween : Tween
    {
        /// <summary>
        /// The prop.
        /// </summary>
        private readonly ElementSchemaProp<float> _prop;

        /// <summary>
        /// The original value of the prop.
        /// </summary>
        private readonly float _originalValue;

        /// <summary>
        /// The difference between start and ending values.
        /// </summary>
        private readonly float _diff;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FloatTween(ElementSchema schema, TweenData data)
            : base(schema, data)
        {
            _prop = schema.Get<float>(Data.Prop);
            _originalValue = data.CustomFrom ? (float) data.From : _prop.Value;
            _diff = (float) data.To - _originalValue;
        }

        /// <inheritdoc />
        protected override void Update(float parameter)
        {
            _prop.Value = _originalValue + parameter * _diff;
        }
    }
}