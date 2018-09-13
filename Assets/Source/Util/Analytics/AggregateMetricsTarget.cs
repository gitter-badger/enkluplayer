using System;
using System.Collections.Generic;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// <c>IMetricsTarget</c> implementation that simply forwards to other targets.
    /// </summary>
    public class AggregateMetricsTarget : IMetricsTarget
    {
        /// <summary>
        /// The list of other targets.
        /// </summary>
        private readonly List<IMetricsTarget> _targets = new List<IMetricsTarget>();

        /// <summary>
        /// Adds a target.
        /// </summary>
        /// <param name="target">The target to add.</param>
        public void Add(IMetricsTarget target)
        {
            if (this == target)
            {
                throw new Exception("Cannot add AggregateMetricsTarget to self.");
            }

            if (!_targets.Contains(target))
            {
                _targets.Add(target);
            }
        }

        /// <inheritdoc />
        public void Send(string key, float value)
        {
            for (var i = 0; i < _targets.Count; i++)
            {
                _targets[i].Send(key, value);
            }
        }
    }
}