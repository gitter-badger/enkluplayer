using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Util
{
    /// <summary>
    /// Simple component that destroys itself after a time.
    /// </summary>
    public class SelfDestructBehavior : MonoBehaviour
    {
        /// <summary>
        /// Time at which behavior was created.
        /// </summary>
        private DateTime _start;

        /// <summary>
        /// How many seconds until self-destruct.
        /// </summary>
        [Range(1f, 10f)]
        [Tooltip("How many seconds until self-destruct.")]
        public float Seconds = 1;

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
            _start = DateTime.Now;
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            if (DateTime.Now.Subtract(_start).TotalSeconds > Seconds)
            {
                Destroy(gameObject);
            }
        }
    }
}
