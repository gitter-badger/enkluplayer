using System;
using System.Collections.Generic;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Util;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Js API for a tween.
    /// </summary>
    public class TweenJs
    {
        /// <summary>
        /// Current state of tween.
        /// </summary>
        private enum PlayState
        {
            NotStarted,
            Started,
            Paused,
            Stopped
        }

        /// <summary>
        /// Track complete and starts separately from <c>TweenData</c>.
        /// </summary>
        private readonly List<IJsCallback> _onComplete = new List<IJsCallback>();
        private readonly List<IJsCallback> _onStart = new List<IJsCallback>();

        /// <summary>
        /// Manages tweens.
        /// </summary>
        private readonly ITweenManager _tweens;

        /// <summary>
        /// Current state of the tween.
        /// </summary>
        private PlayState _state = PlayState.NotStarted;

        /// <summary>
        /// Current tween.
        /// </summary>
        private Tween _tween;

        /// <summary>
        /// The element to affect.
        /// </summary>
        private readonly Element _element;

        /// <summary>
        /// The type of prop.
        /// </summary>
        private readonly string _type;

        /// <summary>
        /// Tween data.
        /// </summary>
        public TweenData Data { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TweenJs(ITweenManager tweens, Element element, string propName, string type)
        {
            _tweens = tweens;

            _element = element;
            _type = type;

            Data = new TweenData
            {
                Prop = propName
            };
        }

        public TweenJs to(object target)
        {
            if (PlayState.NotStarted != _state)
            {
                return this;
            }

            Data.To = target;

            return this;
        }

        public TweenJs from(object start)
        {
            if (PlayState.NotStarted != _state)
            {
                return this;
            }

            Data.From = start;
            Data.CustomFrom = true;

            return this;
        }

        public TweenJs easing(string easing)
        {
            if (PlayState.NotStarted != _state)
            {
                return this;
            }

            Data.Easing = easing;

            return this;
        }

        public TweenJs duration(float sec)
        {
            if (PlayState.NotStarted != _state)
            {
                return this;
            }

            Data.DurationSec = sec;

            return this;
        }

        public TweenJs delay(float sec)
        {
            if (PlayState.NotStarted != _state)
            {
                return this;
            }

            Data.DelaySec = sec;

            return this;
        }

        public TweenJs onComplete(IJsCallback func)
        {
            if (PlayState.NotStarted != _state)
            {
                return this;
            }

            _onComplete.Add(func);

            return this;
        }

        public TweenJs onStart(IJsCallback func)
        {
            if (PlayState.NotStarted != _state)
            {
                return this;
            }

            _onStart.Add(func);

            return this;
        }

        public void start()
        {
            // tween might already be created
            if (null != _tween)
            {
                _tweens.Stop(_tween);
                _tweens.Start(_tween);

                _state = PlayState.Started;

                return;
            }

            if (PlayState.NotStarted != _state)
            {
                return;
            }

            _state = PlayState.Started;

            switch (_type)
            {
                case TweenManagerJsApi.FLOAT:
                {
                    _tween = _tweens.Float(_element, Data);

                    break;
                }
                case TweenManagerJsApi.COL4:
                {
                    _tween = _tweens.Col4(_element, Data);

                    break;
                }
                case TweenManagerJsApi.VEC3:
                {
                    _tween = _tweens.Vec3(_element, Data);

                    break;
                }
                default:
                {
                    throw new Exception(string.Format("Unknown tween type : {0}.", _type));
                }
            }

            // add starts
            for (int i = 0, len = _onStart.Count; i<len; i++)
            {
                var callback = _onStart[i];
                _tween.OnStart += () => callback.Apply(this);
            }
            _onStart.Clear();

            // add completes
            for (int i = 0, len = _onComplete.Count; i<len; i++)
            {
                var callback = _onComplete[i];
                _tween.OnComplete += () => callback.Apply(this);
            }
            _onComplete.Clear();

            _tweens.Start(_tween);
        }

        public void stop()
        {
            if (PlayState.Started != _state
                && PlayState.Paused != _state)
            {
                return;
            }

            _state = PlayState.Stopped;

            _tweens.Stop(_tween);
        }

        public void pause()
        {
            if (PlayState.Started != _state)
            {
                return;
            }

            _state = PlayState.Paused;

            _tweens.Pause(_tween);
        }

        public void resume()
        {
            if (PlayState.Paused != _state)
            {
                return;
            }

            _state = PlayState.Started;

            _tweens.Resume(_tween);
        }
    }
}