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
        Col4 GetAimColor(float aim);

        /// <summary>
        /// Returns the default custor distance in meters.
        /// </summary>
        /// <returns></returns>
        float GetDefaultDistanceForCursor();
        
        /// <summary>
        /// Returns the rate of the cursor spin.
        /// </summary>
        /// <returns></returns>
        float GetReticleSpinRateForCursor();

        /// <summary>
        /// Returns the ammount the cursor should magnet as a function of aim.
        /// </summary>
        /// <param name="aim"></param>
        /// <returns></returns>
        float GetMagnetFromAim(float aim);

        /// <summary>
        /// Returns the ammount the reticle spokes should seperate from aim.
        /// </summary>
        /// <param name="aim"></param>
        /// <returns></returns>
        float GetReticleSpreadFromAim(float aim);

        /// <summary>
        /// Returns the color of the reticle as a function of aim.
        /// </summary>
        /// <param name="aim"></param>
        /// <returns></returns>
        Col4 GetReticleColorFromAim(float aim);

        /// <summary>
        /// Returns the desired reticle scale as a function of aim.
        /// </summary>
        /// <param name="aim"></param>
        /// <returns></returns>
        float GetReticleScaleFromAim(float aim);
    }
}
