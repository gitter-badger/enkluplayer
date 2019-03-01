using System;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Utility class which calculates a next back off value
    /// </summary>
    public class ExponentialBackOff : IBackOff
    {
        private static readonly double TOLERANCE = 0.001;

        private readonly double _baseValue;
        private readonly double _max;
        private double _current = 0.0;
        private bool _maxReached = false;

        /// <summary>
        /// Creates a new <see cref="ExponentialBackOff"/> instance.
        /// </summary>
        /// <param name="baseValue">The base value to include </param>
        /// <param name="max">The maximum value <see cref="Next"/> can return. A value
        /// less than <c>0</c> will set the max to <see cref="double.MaxValue"/></param>
        public ExponentialBackOff(double baseValue, double max = double.MaxValue)
        {
            if (max < 0 || max > double.MaxValue - baseValue)
            {
                max = double.MaxValue - baseValue - 1;
            }

            _baseValue = baseValue;
            _max = max;
            _current = 0.0;
        }

        /// <summary>
        /// Calculates the next values in the back off.
        /// </summary>
        public double Next()
        {
            // Determine if we've reached max, return max
            if (_maxReached)
            {
                return _max;
            }

            // Get next power of 2. Possible this returns at double.MaxValue
            // If so, set max, return
            var nextPow = NextPowerOfTwo();
            if (Math.Abs(nextPow - double.MaxValue) < TOLERANCE)
            {
                _maxReached = true;
                return _max;
            }

            // Apply Back Off, Check limits
            var next = _baseValue + nextPow - 1.0;
            if (next > _max)
            {
                _maxReached = true;
                return _max;
            }

            return next;
        }

        /// <summary>
        /// Updates and advances our current
        /// </summary>
        private double NextPowerOfTwo()
        {
            // Initial state, return 1 (2^0)
            if (Math.Abs(_current) < TOLERANCE)
            {
                _current = 1.0;
                return _current;
            }

            // Ensure we're not going to cross max
            if ((double.MaxValue / _current) < 2.0)
            {
                return double.MaxValue;
            }

            _current *= 2.0;
            return _current;
        }

        /// <summary>
        /// Resets the back off tracking to it's initial state.
        /// </summary>
        public void Reset()
        {
            _maxReached = false;
            _current = 0.0;
        }
    }
}