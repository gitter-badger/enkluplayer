using System;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Util
{
    /// <summary>
    /// Base class for all Tweens.
    /// </summary>
    public abstract class Tween
    {
        /// <summary>
        /// The easing function.
        /// </summary>
        protected readonly Func<float, float> _ease;

        /// <summary>
        /// The current time.
        /// </summary>
        private float _time = -1f;

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
                
                _time = Mathf.Max(0f, value);

                // parameterized t
                var t = _ease(Mathf.Clamp((_time - Data.DelaySec) / Data.DurationSec, 0, 1));
                Update(t);

                // start callback
                if (prev < Mathf.Epsilon && _time > 0f
                    || Math.Abs(prev + 1f) < Mathf.Epsilon)
                {
                    if (null != OnStart)
                    {
                        OnStart();
                    }
                }

                // completion callback
                if (Math.Abs(1f - t) < Mathf.Epsilon)
                {
                    if (!IsComplete)
                    {
                        if (null != OnComplete)
                        {
                            OnComplete();
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
        /// Called the first time the time is set, and every time the tween moves from zero time to a non-zero time.
        /// </summary>
        public event Action OnStart;

        /// <summary>
        /// Called when the tween completes.
        /// </summary>
        public event Action OnComplete;

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
            _originalValue = data.CustomFrom ? Convert.ToSingle(data.From) : _prop.Value;
            _diff = Convert.ToSingle(data.To) - _originalValue;
        }

        /// <inheritdoc />
        protected override void Update(float parameter)
        {
            _prop.Value = _originalValue + parameter * _diff;
        }
    }

    /// <summary>
    /// Tween for a Col4.
    /// </summary>
    public class Col4Tween : Tween
    {
        /// <summary>
        /// The prop.
        /// </summary>
        private readonly ElementSchemaProp<Col4> _prop;

        /// <summary>
        /// The original value of the prop.
        /// </summary>
        private readonly Col4 _originalValue;

        /// <summary>
        /// The difference between start and ending values.
        /// </summary>
        private readonly Col4 _diff;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Col4Tween(ElementSchema schema, TweenData data)
            : base(schema, data)
        {
            _prop = schema.Get<Col4>(Data.Prop);
            _originalValue = data.CustomFrom ? (Col4) data.From : _prop.Value;
            _diff = (Col4) data.To - _originalValue;
        }

        /// <inheritdoc />
        protected override void Update(float parameter)
        {
            _prop.Value = _originalValue + parameter * _diff;
        }
    }

    /// <summary>
    /// Tween for a vec3.
    /// </summary>
    public class Vec3Tween : Tween
    {
        /// <summary>
        /// The prop.
        /// </summary>
        private readonly ElementSchemaProp<Vec3> _prop;

        /// <summary>
        /// The original value of the prop.
        /// </summary>
        private readonly Vec3 _originalValue;

        /// <summary>
        /// The difference between start and ending values.
        /// </summary>
        private readonly Vec3 _diff;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Vec3Tween(ElementSchema schema, TweenData data)
            : base(schema, data)
        {
            _prop = schema.Get<Vec3>(Data.Prop);
            _originalValue = data.CustomFrom ? (Vec3) data.From : _prop.Value;
            _diff = (Vec3) data.To - _originalValue;
        }

        /// <inheritdoc />
        protected override void Update(float parameter)
        {
            _prop.Value = _originalValue + parameter * _diff;
        }
    }
}