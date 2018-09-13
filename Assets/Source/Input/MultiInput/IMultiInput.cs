using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Interface which abstracts away touches and clicks into <c>InputState</c>
    /// objects.
    /// </summary>
    public interface IMultiInput
    {
        /// <summary>
        /// The camera to use.
        /// </summary>
        Camera Camera { get; set; }

        /// <summary>
        /// Input points.
        /// </summary>
        List<InputPoint> Points { get; }
        
        /// <summary>
        /// Call to update input.
        /// </summary>
        /// <param name="dt">Time elapsed since last Update.</param>
        void Update(float dt);
    }
}