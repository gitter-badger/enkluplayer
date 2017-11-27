using CreateAR.SpirePlayer.UI;
using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Center point surrounded by spokes.
    /// </summary>
    public class ReticleMonoBehaviour : WidgetMonoBehaviour, IReticle
    {
        private float _rotation;
        private float _scale;
        private float _spread;
        private float _centerAlpha;

        /// <summary>
        /// Widgets
        /// </summary>
        public List<Transform> Spokes = new List<Transform>();

        /// <summary>
        /// Center point
        /// </summary>
        public WidgetMonoBehaviour Center;

        /// <summary>
        /// The visible text on the primitive.
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; Refresh(); }
        }

        /// <summary>
        /// The visible text on the primitive.
        /// </summary>
        public float Scale
        {
            get { return _scale; }
            set { _scale = value; Refresh(); }
        }

        /// <summary>
        /// The visible text on the primitive.
        /// </summary>
        public float Spread
        {
            get { return _spread; }
            set { _spread = value; Refresh(); }
        }

        /// <summary>
        /// The visible text on the primitive.
        /// </summary>
        public float CenterAlpha
        {
            get { return _centerAlpha; }
            set { _centerAlpha = value; Refresh(); }
        }

        /// <summary>
        /// Initialization.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="schema"></param>
        /// <param name="children"></param>
        public override void Load(ElementData data, ElementSchema schema, IElement[] children)
        {
            base.Load(data, schema, children);

            if (Center != null)
            {
                Center.LoadFromMonoBehaviour(this);
            }
        }

        /// <summary>
        /// Updates the point widgets
        /// </summary>
        public void Refresh()
        {
            for (int i = 0, count = Spokes.Count; i < count; ++i)
            {
                var spoke = Spokes[i];
                if (spoke != null)
                {
                    var theta
                        = Mathf.PI
                          * 2.0f
                          * i
                          / count
                          + _rotation;

                    var pointLocalPosition
                        = new Vector3(
                            Mathf.Sin(theta) * _spread,
                            Mathf.Cos(theta) * _spread,
                            0.0f);

                    spoke
                            .transform
                            .localPosition
                        = pointLocalPosition;

                    spoke
                            .transform
                            .localScale
                        = Vector3.one
                          * _scale;

                    spoke
                            .transform
                            .localRotation
                        = Quaternion.AngleAxis(Mathf.Rad2Deg * -theta, Vector3.forward);
                }
            }

            if (Center != null)
            {
                Center.LocalColor = new Col4(1,1,1, _centerAlpha);

                Center
                        .transform
                        .localScale
                    = Vector3.one
                      * _scale;
            }
        }
    }
}
