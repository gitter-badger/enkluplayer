using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer.Assets
{
    /// <summary>
    /// Communicates progress of a load.
    /// </summary>
    public class LoadProgress
    {
        /// <summary>
        /// Backing variable for Value property.
        /// </summary>
        private float _value;
        
        /// <summary>
        /// List of all linked LoadProgress objects.
        /// </summary>
        private readonly List<LoadProgress> _chained = new List<LoadProgress>();

        /// <summary>
        /// Normalized load percentage, between 0 and 1.
        /// </summary>
        public float Value
        {
            get { return _value; }
            set
            {
                _value = value;

                for (int i = 0, len = _chained.Count; i < len; i++)
                {
                    _chained[i].Value = _value;
                }
            }
        }

        /// <summary>
        /// True iff the load is complete.
        /// </summary>
        public bool IsComplete
        {
            get { return Math.Abs(Value - 1f) < Mathf.Epsilon; }
        }

        /// <summary>
        /// Changes in this instance will be pushed with the passed in instance.
        /// </summary>
        /// <param name="progress">A LoadProgress instance.</param>
        internal void Chain(LoadProgress progress)
        {
            _chained.Add(progress);

            progress.Value = _value;
        }
    }
}