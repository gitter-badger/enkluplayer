using System.Collections.Generic;
using System.Linq;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Util
{
    /// <summary>
    /// Creates and manages tweens.
    /// </summary>
    public class TweenManager : ITweenManager
    {
        /// <summary>
        /// For internal record keeping.
        /// </summary>
        private class TweenRecord
        {
            /// <summary>
            /// Tween value.
            /// </summary>
            public readonly Tween Tween;

            /// <summary>
            /// True iff the tween is paused.
            /// </summary>
            public bool IsPaused;
            
            /// <summary>
            /// Tween record.
            /// </summary>
            /// <param name="tween">The tween to track.</param>
            public TweenRecord(Tween tween)
            {
                Tween = tween;
            }
        }

        /// <summary>
        /// List of active tweens.
        /// </summary>
        private readonly List<TweenRecord> _active = new List<TweenRecord>();

        /// <summary>
        /// List of tweens to remove, lazily.
        /// </summary>
        private readonly List<Tween> _queuedRemoves = new List<Tween>();

        /// <inheritdoc />
        public Tween Float(Element element, TweenData data)
        {
            return new FloatTween(element.Schema, data);
        }

        /// <inheritdoc />
        public Tween Col4(Element element, TweenData data)
        {
            return new Col4Tween(element.Schema, data);
        }

        /// <inheritdoc />
        public Tween Vec3(Element element, TweenData data)
        {
            return new Vec3Tween(element.Schema, data);
        }

        /// <inheritdoc />
        public void Start(Tween tween)
        {
            _active.Add(new TweenRecord(tween));

            tween.Time = 0;
        }

        /// <inheritdoc />
        public void Stop(Tween tween)
        {
            _queuedRemoves.Add(tween);
        }

        /// <inheritdoc />
        public void StopAll()
        {
            _queuedRemoves.AddRange(_active.Select(rec => rec.Tween));
        }

        /// <inheritdoc />
        public void Pause(Tween tween)
        {
            var record = Record(tween);
            if (null != record)
            {
                record.IsPaused = true;
            }
        }

        /// <inheritdoc />
        public void Resume(Tween tween)
        {
            var record = Record(tween);
            if (null != record)
            {
                record.IsPaused = false;
            }
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            // remove queued first
            var ilen = _queuedRemoves.Count;
            if (ilen > 0)
            {
                for (var i = 0; i < ilen; i++)
                {
                    var tween = _queuedRemoves[i];
                    for (var j = _active.Count - 1; j >= 0; j--)
                    {
                        if (_active[j].Tween == tween)
                        {
                            _active.RemoveAt(j);

                            break;
                        }
                    }
                }

                _queuedRemoves.Clear();
            }

            // advance tweens
            for (int i = 0, len = _active.Count; i < len; i++)
            {
                var record = _active[i];
                if (record.IsPaused)
                {
                    continue;
                }

                var tween = record.Tween;
                tween.Time += dt;

                if (tween.IsComplete)
                {
                    _queuedRemoves.Add(tween);
                }
            }
        }

        /// <summary>
        /// Retrieves a record for a tween.
        /// </summary>
        /// <param name="tween">The tween to fetch a record for.</param>
        /// <returns></returns>
        private TweenRecord Record(Tween tween)
        {
            for (int i = 0, len = _active.Count; i < len; i++)
            {
                var tw = _active[i];
                if (tw.Tween == tween)
                {
                    return tw;
                }
            }

            return null;
        }
    }
}