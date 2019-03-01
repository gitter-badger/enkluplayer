using System;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Implementation of linear back off, which incrementally updates the values.
    /// </summary>
    public class LinearBackOff : IBackOff
    {
        private static readonly double TOLERANCE = 0.001;

        private readonly double _baseValue;
        private readonly double _increment;
        private readonly double _max;
        private double _current = 0.0;
        private bool _maxReached = false;

        /// <summary>
        /// Creates a new <see cref="LinearBackOff"/> instance.
        /// </summary>
        /// <param name="baseValue">The baseline value to start with.</param>
        /// <param name="increment">The incremental value to apply each call.</param>
        /// <param name="max">The maximum value the next can return.</param>
        public LinearBackOff(double baseValue, double increment, double max = double.MaxValue)
        {
            if (max < 0 || max > double.MaxValue - baseValue)
            {
                max = double.MaxValue - baseValue - 1;
            }

            _baseValue = baseValue;
            _increment = increment;
            _max = max;
            _current = -increment;
        }

        /// <inheritdoc />
        public double Next()
        {
            // Determine if we've reached max, return max
            if (_maxReached)
            {
                return _max;
            }

            // Get next power of 2. Possible this returns at double.MaxValue
            // If so, set max, return
            var nextIncr = NextValue();
            if (Math.Abs(nextIncr - double.MaxValue) < TOLERANCE)
            {
                _maxReached = true;
                return _max;
            }

            // Apply Back Off, Check limits
            var next = _baseValue + nextIncr;
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
        private double NextValue()
        {
            // Ensure we're not going to cross max
            if ((double.MaxValue - _current) > _increment)
            {
                return double.MaxValue;
            }

            _current += _increment;
            return _current;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _maxReached = false;
            _current = 0.0;
        }
    }
}