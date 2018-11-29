using System.Collections.Generic;
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

        /// <summary>
        /// Creates a tween for a float value.
        /// </summary>
        /// <param name="element">The element to affect..</param>
        /// <param name="data">The data for the tween.</param>
        /// <returns></returns>
        public Tween Float(Element element, TweenData data)
        {
            return new FloatTween(element.Schema, data);
        }

        /// <summary>
        /// Starts a tween.
        /// </summary>
        /// <param name="tween">The tween to start.</param>
        public void Start(Tween tween)
        {
            _active.Insert(0, new TweenRecord(tween));

            tween.Time = 0;
        }

        /// <summary>
        /// Stops a tween.
        /// </summary>
        /// <param name="tween">The tween to stop.</param>
        public void Stop(Tween tween)
        {
            _queuedRemoves.Add(tween);
        }

        /// <summary>
        /// Pauses a tween.
        /// </summary>
        /// <param name="tween">The tween to pause.</param>
        public void Pause(Tween tween)
        {
            var record = Record(tween);
            if (null != record)
            {
                record.IsPaused = true;
            }
        }

        /// <summary>
        /// Resumes a tween.
        /// </summary>
        /// <param name="tween">The tween to resume.</param>
        public void Resume(Tween tween)
        {
            var record = Record(tween);
            if (null != record)
            {
                record.IsPaused = false;
            }
        }

        /// <summary>
        /// Call to advance tweens.
        /// </summary>
        /// <param name="dt">The number of seconds to advance the tweens.</param>
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
                    _active.RemoveAt(i);
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