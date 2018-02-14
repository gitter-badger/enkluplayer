using System;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Provides strategies to point objects with.
    /// </summary>
    public class FaceComponent : MonoBehaviour
    {
        /// <summary>
        /// Describes potential ways an object may face.
        /// </summary>
        private enum FaceType
        {
            Horizontal,
            Absolute,
            Camera
        }

        /// <summary>
        /// Current FaceType.
        /// </summary>
        private FaceType _type;

        /// <summary>
        /// Informs the component which way to face. Options are case-insensitive.
        /// </summary>
        /// <param name="type">The string value of the type.</param>
        public void Face(string type)
        {
            if (string.IsNullOrEmpty(type) || type.Length < 2)
            {
                _type = 0;
            }
            else
            {
                // Capitalcase
                type = type[0].ToString().ToUpperInvariant() + type.Substring(1).ToLowerInvariant();

                _type = EnumExtensions.Parse<FaceType>(type);
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            switch (_type)
            {
                case FaceType.Horizontal:
                {
                    var mainCamera = Camera.main;
                    if (null == mainCamera)
                    {
                        return;
                    }

                    var delta = transform.position - mainCamera.transform.position;
                    delta.y = 0;
                    delta.Normalize();

                    transform.LookAt(
                        transform.position + delta,
                        Vector3.up);

                    var localEuler = transform.localEulerAngles;
                    localEuler.z = 0;
                    transform.localEulerAngles = localEuler;

                    break;
                }
                case FaceType.Camera:
                {

                    var mainCamera = Camera.main;
                    if (null == mainCamera)
                    {
                        return;
                    }

                    var delta = transform.position - mainCamera.transform.position;
                    transform.LookAt(
                        transform.position + delta,
                        mainCamera.transform.up);

                    var localEuler = transform.localEulerAngles;
                    localEuler.z = 0;
                    transform.localEulerAngles = localEuler;

                    break;
                }
                case FaceType.Absolute:
                {
                    // do nothing

                    break;
                }
                default:
                {
                    transform.rotation = Quaternion.identity;

                    break;
                }
            }
        }
    }
}