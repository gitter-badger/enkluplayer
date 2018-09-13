using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Garbage.
    /// </summary>
    public class Rotator : MonoBehaviour
    {
        /// <summary>
        /// Speed to rotate.
        /// </summary>
        public float Speed;
        
        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            var current = transform.localRotation.eulerAngles;
            transform.localRotation = Quaternion.Euler(
                current.x,
                current.y + Speed * Time.smoothDeltaTime,
                current.z);
        }
    }
}