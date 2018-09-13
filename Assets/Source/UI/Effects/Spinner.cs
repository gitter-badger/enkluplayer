using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Crappy spinner implementation.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class Spinner : MonoBehaviour
    {
        /// <summary>
        /// Speed of rotation.
        /// </summary>
        public float Speed = 200f;

        /// <summary>
        /// Transform.
        /// </summary>
        private RectTransform _transform;

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
            _transform = GetComponent<RectTransform>();
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            _transform.Rotate(Vector3.forward, Time.deltaTime * Speed);
        }
    }
}