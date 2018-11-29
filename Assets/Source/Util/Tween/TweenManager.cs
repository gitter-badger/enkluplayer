using System;
using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
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
        public string Prop;

        public object From;
        public object To;

        public TweenEasingType Easing = TweenEasingType.Linear;
        public float DurationSec = 1f;
        public float DelaySec = 0f;

        public Action OnStart;
        public Action OnComplete;
    }

    public class TweenManager
    {
        private class TweenRecord
        {
            public readonly Tween Tween;
            public bool IsPaused;
            
            public TweenRecord(Tween tween)
            {
                Tween = tween;
            }
        }

        private readonly List<TweenRecord> _tweens = new List<TweenRecord>();
        private readonly List<Tween> _queuedRemoves = new List<Tween>();

        public Tween Float(Element element, TweenData data)
        {
            return new FloatTween(element.Schema, data);
        }

        public void Start(Tween tween)
        {
            _tweens.Insert(0, new TweenRecord(tween));
        }

        public void Abort(Tween tween)
        {
            _queuedRemoves.Add(tween);
        }

        public void Pause(Tween tween)
        {
            var record = Record(tween);
            if (null != record)
            {
                record.IsPaused = true;
            }
        }

        public void Resume(Tween tween)
        {
            var record = Record(tween);
            if (null != record)
            {
                record.IsPaused = false;
            }
        }

        public void Update(float dt)
        {
            // remove queued first
            var ilen = _queuedRemoves.Count;
            if (ilen > 0)
            {
                for (var i = 0; i < ilen; i++)
                {
                    var tween = _queuedRemoves[i];
                    for (var j = _tweens.Count - 1; j >= 0; j--)
                    {
                        if (_tweens[j].Tween == tween)
                        {
                            _tweens.RemoveAt(j);

                            break;
                        }
                    }
                }

                _queuedRemoves.Clear();
            }

            // advance tweens
            for (int i = 0, len = _tweens.Count; i < len; i++)
            {
                var record = _tweens[i];
                if (record.IsPaused)
                {
                    continue;
                }

                var tween = record.Tween;
                tween.Time += dt;

                if (tween.IsComplete)
                {
                    _tweens.RemoveAt(i);
                }
            }
        }

        private TweenRecord Record(Tween tween)
        {
            for (int i = 0, len = _tweens.Count; i < len; i++)
            {
                var tw = _tweens[i];
                if (tw.Tween == tween)
                {
                    return tw;
                }
            }

            return null;
        }
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

        public bool IsComplete { get; private set; }

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
        
        public FloatTween(ElementSchema schema, TweenData data)
            : base(schema, data)
        {
            _prop = schema.Get<float>(Data.Prop);
            _originalValue = _prop.Value;
            _diff = (float) data.To - _originalValue;
        }

        protected override void Update(float parameter)
        {
            _prop.Value = _originalValue + parameter * _diff;
        }
    }
}