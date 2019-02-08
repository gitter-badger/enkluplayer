using System;
using Enklu.Data;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Color represented by hue, saturation, and brightness.
    /// </summary>
    public class HsbColor
    {
        /// <summary>
        /// Hue.
        /// </summary>
        public float H;

        /// <summary>
        /// Saturation.
        /// </summary>
        public float S;

        /// <summary>
        /// Brightness.
        /// </summary>
        public float B;

        /// <summary>
        /// Alpha.
        /// </summary>
        public float A;

        /// <summary>
        /// Crreates a new color.
        /// </summary>
        /// <param name="h">Hue.</param>
        /// <param name="s">Saturation.</param>
        /// <param name="b">Brightness.</param>
        /// <param name="a">Alpha.</param>
        public HsbColor(float h, float s, float b, float a)
        {
            H = h;
            S = s;
            B = b;
            A = a;
        }

        /// <summary>
        /// Creates a new color.
        /// </summary>
        /// <param name="h">Hue.</param>
        /// <param name="s">Saturation.</param>
        /// <param name="b">Brightness</param>
        public HsbColor(float h, float s, float b)
            : this(h, s, b, 1f)
        {
            //
        }

        /// <summary>
        /// Creates a new color from an RGB color.
        /// </summary>
        /// <param name="col">Color.</param>
        public HsbColor(Col4 col)
        {
            var temp = FromColor(col);
            H = temp.H;
            S = temp.S;
            B = temp.B;
            A = temp.A;
        }

        /// <summary>
        /// Creates an Rgb color.
        /// </summary>
        /// <returns></returns>
        public Col4 ToColor()
        {
            return ToColor(this);
        }

        /// <summary>
        /// Helpful ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "H:" + H + " S:" + S + " B:" + B;
        }

        /// <summary>
        /// Creates an HsbColor from an RGB color.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns></returns>
        public static HsbColor FromColor(Col4 color)
        {
            var ret = new HsbColor(0f, 0f, 0f, color.a);

            var r = color.r;
            var g = color.g;
            var b = color.b;

            var max = Mathf.Max(r, Mathf.Max(g, b));

            if (max <= 0)
                return ret;

            var min = Mathf.Min(r, Mathf.Min(g, b));
            var dif = max - min;

            if (max > min)
            {
                if (Math.Abs(g - max) < Mathf.Epsilon)
                    ret.H = (b - r) / dif * 60f + 120f;
                else if (Math.Abs(b - max) < Mathf.Epsilon)
                    ret.H = (r - g) / dif * 60f + 240f;
                else if (b > g)
                    ret.H = (g - b) / dif * 60f + 360f;
                else
                    ret.H = (g - b) / dif * 60f;
                if (ret.H < 0)
                    ret.H = ret.H + 360f;
            }
            else
            {
                ret.H = 0;
            }

            ret.H *= 1f / 360f;
            ret.S = dif / max * 1f;
            ret.B = max;

            return ret;
        }

        /// <summary>
        /// Convert to RGB color.
        /// </summary>
        /// <param name="hsbColor">The HsbColor.</param>
        /// <returns></returns>
        public static Col4 ToColor(HsbColor hsbColor)
        {
            var r = hsbColor.B;
            var g = hsbColor.B;
            var b = hsbColor.B;
            if (Math.Abs(hsbColor.S) > Mathf.Epsilon)
            {
                var max = hsbColor.B;
                var dif = hsbColor.B * hsbColor.S;
                var min = hsbColor.B - dif;

                var h = hsbColor.H * 360f;

                if (h < 60f)
                {
                    r = max;
                    g = h * dif / 60f + min;
                    b = min;
                }
                else if (h < 120f)
                {
                    r = -(h - 120f) * dif / 60f + min;
                    g = max;
                    b = min;
                }
                else if (h < 180f)
                {
                    r = min;
                    g = max;
                    b = (h - 120f) * dif / 60f + min;
                }
                else if (h < 240f)
                {
                    r = min;
                    g = -(h - 240f) * dif / 60f + min;
                    b = max;
                }
                else if (h < 300f)
                {
                    r = (h - 240f) * dif / 60f + min;
                    g = min;
                    b = max;
                }
                else if (h <= 360f)
                {
                    r = max;
                    g = min;
                    b = -(h - 360f) * dif / 60 + min;
                }
                else
                {
                    r = 0;
                    g = 0;
                    b = 0;
                }
            }

            return new Col4(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.A);
        }

        /// <summary>
        /// Lerps between two HsbColors.
        /// </summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <param name="t">Value in [0, 1]</param>
        /// <returns></returns>
        public static HsbColor Lerp(HsbColor a, HsbColor b, float t)
        {
            float h, s;

            //check special case black (color.b==0): interpolate neither hue nor saturation!
            //check special case grey (color.s==0): don't interpolate hue!
            if (Math.Abs(a.B) < Mathf.Epsilon)
            {
                h = b.H;
                s = b.S;
            }
            else if (Math.Abs(b.B) < Mathf.Epsilon)
            {
                h = a.H;
                s = a.S;
            }
            else
            {
                if (Math.Abs(a.S) < Mathf.Epsilon)
                {
                    h = b.H;
                }
                else if (Math.Abs(b.S) < Mathf.Epsilon)
                {
                    h = a.H;
                }
                else
                {
                    // works around bug with LerpAngle
                    var angle = Mathf.LerpAngle(a.H * 360f, b.H * 360f, t);
                    while (angle < 0f)
                        angle += 360f;
                    while (angle > 360f)
                        angle -= 360f;
                    h = angle / 360f;
                }
                s = Mathf.Lerp(a.S, b.S, t);
            }
            return new HsbColor(h, s, Mathf.Lerp(a.B, b.B, t), Mathf.Lerp(a.A, b.A, t));
        }
    }
}