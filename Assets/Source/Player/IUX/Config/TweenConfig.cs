using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Contains configuration for each <c>TweenType</c>.
    /// </summary>
    public class TweenConfig : MonoBehaviour, ITweenConfig
    {
        /// <summary>
        /// Backing Unity serialized field.
        /// </summary>
        [FormerlySerializedAs("Profiles")] [SerializeField]
        public TweenProfile[] _profiles;

        /// <inheritdoc />
        public TweenProfile[] Profiles
        {
            get { return _profiles; }
        }

        /// <inheritdoc />
        public float DurationSeconds(TweenType type)
        {
            for (int i = 0, len = Profiles.Length; i < len; i++)
            {
                var profile = Profiles[i];
                if (profile.Type == type)
                {
                    return profile.DurationSeconds;
                }
            }

            return -1f;
        }
    }
}