using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CreateAR.SpirePlayer.UI;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public interface IInteractivePrimitive : IPrimitive
    {
        /// <summary>
        /// Returns true if the primitive is targeted by the specified ray.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns></returns>
        bool IsTargeted(Ray ray);

        /// <summary>
        /// Enables or disables interaction.
        /// </summary>
        /// <param name="isInteractable"></param>
        void SetInteractionEnabled(bool isInteractable, bool isFocused);

        /// <summary>
        /// Gets the bounding radius for interactions
        /// </summary>
        /// <returns></returns>
        float GetBoundingRadius();
    }
}
