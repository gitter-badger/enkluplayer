using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// API for Transform component.
    /// </summary>
    public class TransformJsApi
    {
        /// <summary>
        /// The transform in question.
        /// </summary>
        private readonly Transform _transform;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="transform">Transform component.</param>
        public TransformJsApi(Transform transform)
        {
            _transform = transform;
        }

        /// <summary>
        /// Retrieves position.
        /// </summary>
        /// <returns></returns>
        public Vector3Js getPosition()
        {
            return new Vector3Js(_transform.position);
        }

        /// <summary>
        /// Sets position.
        /// </summary>
        /// <param name="vec">Position.</param>
        public void setPosition(Vector3Js vec)
        {
            _transform.position = vec.ToVector3();
        }

        /// <summary>
        /// Retrieves the scale.
        /// </summary>
        /// <returns></returns>
        public Vector3Js getScale()
        {
            return new Vector3Js(_transform.localScale);
        }

        /// <summary>
        /// Sets the scale.
        /// </summary>
        /// <param name="vec"></param>
        public void setScale(Vector3Js vec)
        {
            _transform.localScale = vec.ToVector3();
        }

        /// <summary>
        /// Retrieves the rotation.
        /// </summary>
        /// <returns></returns>
        public QuatJs getRotation()
        {
            return new QuatJs(_transform.rotation);
        }

        /// <summary>
        /// Sets the rotation.
        /// </summary>
        /// <param name="quatertion">Quaternion.</param>
        public void setRotation(QuatJs quatertion)
        {
            _transform.rotation = quatertion.ToQuaternion();
        }
    }
}