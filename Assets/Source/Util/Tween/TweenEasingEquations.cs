﻿using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Util
{
    /// <summary>
    /// Easing equations.
    ///
    /// http://www.robertpenner.com/easing_terms_of_use.html
    /// </summary>
    public static class TweenEasingEquations
    {
        /// <summary>
        /// Retrieves an equation for a type.
        /// </summary>
        /// <param name="type">The type of equation to retrieve.</param>
        /// <returns></returns>
        public static Func<float, float> Equation(string type)
        {
            switch (type)
            {
                case TweenEasingTypes.BounceIn:
                {
                    return BounceIn;
                }
                case TweenEasingTypes.BounceOut:
                {
                    return BounceOut;
                }
                case TweenEasingTypes.BounceInOut:
                {
                    return BounceInOut;
                }
                case TweenEasingTypes.QuadraticIn:
                {
                    return QuadraticIn;
                }
                case TweenEasingTypes.QuadraticOut:
                {
                    return QuadraticOut;
                }
                case TweenEasingTypes.QuadraticInOut:
                {
                    return QuadraticInOut;
                }
                case TweenEasingTypes.CubicIn:
                {
                    return CubicIn;
                }
                case TweenEasingTypes.CubicOut:
                {
                    return CubicOut;
                }
                case TweenEasingTypes.CubicInOut:
                {
                    return CubicInOut;
                }
                case TweenEasingTypes.QuarticIn:
                {
                    return QuarticIn;
                }
                case TweenEasingTypes.QuarticOut:
                {
                    return QuarticOut;
                }
                case TweenEasingTypes.QuarticInOut:
                {
                    return QuarticInOut;
                }
                case TweenEasingTypes.QuinticIn:
                {
                    return QuinticIn;
                }
                case TweenEasingTypes.QuinticOut:
                {
                    return QuinticOut;
                }
                case TweenEasingTypes.QuinticInOut:
                {
                    return QuinticInOut;
                }
                case TweenEasingTypes.ExpoIn:
                {
                    return ExpoIn;
                }
                case TweenEasingTypes.ExpoOut:
                {
                    return ExpoOut;
                }
                case TweenEasingTypes.ExpoInOut:
                {
                    return ExpoInOut;
                }
                default:
                {
                    return Linear;
                }
            }
        }

        public static float Linear(float t)
        {
            return t;
        }

        public static float BounceIn(float t)
        {
            return 1 - BounceOut(1 - t);
        }

        public static float BounceOut(float t)
        {
            if ((t /= 1) < 0.363636364f)
            {
                return 7.5625f * t * t;
            }

            if (t < 0.727272727f)
            {
                return (7.5625f * (t -= 0.545454545f) * t + .75f);
            }

            if (t < 0.909090909f)
            {
                return (7.5625f * (t -= 0.818181818f) * t + .9375f);
            }

            return (7.5625f * (t -= 0.954545455f) * t + .984375f);
        }

        public static float BounceInOut(float t)
        {
            if (t < 0.5f)
            {
                return BounceIn(t * 2) * 0.5f;
            }

            return BounceOut(t * 2 - 1) * 0.5f + 0.5f;
        }

        public static float QuadraticIn(float t)
        {
            return t * t;
        }

        public static float QuadraticOut(float t)
        {
            return -t * (t - 2);
        }

        public static float QuadraticInOut(float t)
        {
            if ((t *= 2) < 1)
            {
                return 0.5f * t * t;
            }

            return -0.5f * (--t * (t - 2) - 1);
        }

        public static float CubicIn(float t)
        {
            return t * t * t;
        }

        public static float CubicOut(float t)
        {
            return --t * t * t + 1;
        }

        public static float CubicInOut(float t)
        {
            if ((t *= 2) < 1)
            {
                return 0.5f * t * t * t;
            }

            return 0.5f * ((t -= 2) * t * t + 2);
        }

        public static float QuarticIn(float t)
        {
            return t * t * t * t;
        }

        public static float QuarticOut(float t)
        {
            return -(--t * t * t * t - 1);
        }

        public static float QuarticInOut(float t)
        {
            if ((t *= 2) < 1)
            {
                return 0.5f * t * t * t * t;
            }

            return 0.5f * ((t -= 2) * t * t * t - 2);
        }

        public static float QuinticIn(float t)
        {
            return t * t * t * t * t;
        }

        public static float QuinticOut(float t)
        {
            return (t = t - 1) * t * t * t * t + 1;
        }

        public static float QuinticInOut(float t)
        {
            if ((t *= 2) < 1)
            {
                return 0.5f * t * t * t * t * t;
            }

            return 0.5f * ((t -= 2) * t * t * t * t + 2);
        }

        public static float ExpoIn(float t)
        {
            return Math.Abs(t) < Mathf.Epsilon ? 0 : (float) Math.Pow(2, 10 * (t - 1));
        }

        public static float ExpoOut(float t)
        {
            return Math.Abs(t - 1) < Mathf.Epsilon ? 1 : (float) -Math.Pow(2, -10 * t) + 1;
        }

        public static float ExpoInOut(float t)
        {
            if (Math.Abs(t) < Mathf.Epsilon)
            {
                return 0;
            }

            if (Math.Abs(t - 1) < Mathf.Epsilon)
            {
                return 1;
            }

            if ((t *= 2) < 1)
            {
                return 0.5f * (float) Math.Pow(2, 10 * (t - 1));
            }

            return 0.5f * (float) (-Math.Pow(2, -10 * (t - 1)) + 2);
        }
    }
}