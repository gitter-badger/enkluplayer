using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    public interface IWidgetConfig
    {
        /// <summary>
        /// Returns the ammount of rotation to denote stability
        /// </summary>
        float StabilityRotation { get; }

        /// <summary>
        /// Returns the fill decay at a specific time
        /// </summary>
        /// <param name="elapsed"></param>
        /// <returns></returns>
        float GetFillDelay(float elapsed);

        /// <summary>
        /// Accessor for stability multiplier.
        /// </summary>
        /// <param name="stability"></param>
        /// <returns></returns>
        float GetFillRateMultiplierFromStability(float stability);

        /// <summary>
        /// Accessor for stability multiplier.
        /// </summary>
        /// <param name="stability"></param>
        /// <returns></returns>
        float GetFillRateMultiplierFromAim(float aim);

        /// <summary>
        /// Returns fill duration in seconds.
        /// </summary>
        /// <returns></returns>
        float GetFillDuration();

        /// <summary>
        /// Accessor for stability multiplier.
        /// </summary>
        /// <param name="aim"></param>
        /// <returns></returns>
        float GetAimScale(float aim);

        /// <summary>
        /// Accessor for stability multiplier.
        /// </summary>
        /// <param name="aim"></param>
        /// <returns></returns>
        Color GetAimColor(float aim);
    }
}
