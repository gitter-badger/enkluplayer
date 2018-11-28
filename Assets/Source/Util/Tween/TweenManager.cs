using System;
using CreateAR.EnkluPlayer.IUX;
using UnityEditor;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Util
{
    public enum TweenEasingType
    {
        Linear,
        BounceIn,
        BounceOut,
        BounceInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        ExpoIn,
        ExpoOut,
        ExpoInOut,
        QuadraticIn,
        QuadraticOut,
        QuadraticInOut,
        QuarticIn,
        QuarticOut,
        QuarticInOut,
        QuinticIn,
        QuinticOut,
        QuinticInOut
    }

    public static class TweenEasingEquations
    {
        public static Func<float, float> Equation(TweenEasingType type)
        {
            switch (type)
            {
                default:
                {
                    return Linear;
                }
            }
        }

        public static float Linear(float t)
        {
            return t;
        }
    }

    public class TweenData
    {
        public string PropName;

        public object From;
        public object To;

        public TweenEasingType Easing;
        public float DurationSec;
        public float DelaySec;

        public Action OnComplete;
    }

    public class TweenManager
    {
        
    }

    public abstract class Tween
    {
        public ElementSchema Schema { get; private set; }
        public TweenData Data { get; private set; }

        public float Time
        {
            get { return _time; }
            set
            {
                _time = value;

                // parameterized t
                var t = _ease(Mathf.Clamp((_time - Data.DelaySec) / Data.DurationSec, 0, 1));
                Update(t);
            }
        }

        private float _time;
        protected readonly Func<float, float> _ease;

        protected Tween(ElementSchema schema, TweenData data)
        {
            Schema = schema;
            Data = data;

            _ease = TweenEasingEquations.Equation(Data.Easing);
        }

        protected abstract void Update(float time);
    }

    public class FloatTween : Tween
    {
        private readonly ElementSchemaProp<float> _prop;
        private readonly float _originalValue;
        private readonly float _diff;
        private bool _isComplete;
        
        public FloatTween(ElementSchema schema, TweenData data)
            : base(schema, data)
        {
            _prop = schema.Get<float>(Data.PropName);
            _originalValue = _prop.Value;
            _diff = (float) data.To - _originalValue;
        }

        protected override void Update(float parameter)
        {
            _prop.Value = _originalValue + parameter * _diff;

            if (Math.Abs(1f - parameter) < Mathf.Epsilon)
            {
                if (!_isComplete)
                {
                    if (null != Data.OnComplete)
                    {
                        Data.OnComplete();
                    }
                }

                _isComplete = true;
            }
            else
            {
                _isComplete = false;
            }
        }
    }
}