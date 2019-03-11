using System;
using System.Collections.Generic;
using Enklu.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Manages colors.
    /// </summary>
    public class ColorConfigMonoBehaviour : MonoBehaviour
    {
        public ColorConfig ColorConfig;
        
        [FormerlySerializedAs("CurrentProfileName")] [SerializeField] 
        private string _currentProfileName;

        [FormerlySerializedAs("Profiles")] [SerializeField]
        private List<ColorConfig.ColorProfile> _profiles = new List<ColorConfig.ColorProfile>();
        
        /// <inheritdoc />
        public string CurrentProfileName
        {
            get { return _currentProfileName; }
        }

        /// <inheritdoc />
        public ColorConfig.ColorProfile CurrentProfile
        {
            get
            {
                if (Profiles != null
                    && Profiles[0] != null)
                {
                    for (int i = 0, count = Profiles.Count; i < count; ++i)
                    {
                        var profile = Profiles[i];
                        if (profile != null
                            && profile.Name == CurrentProfileName)
                        {
                            return profile;
                        }
                    }

                    return Profiles[0];
                }

                return null;
            }
        }

        /// <inheritdoc />
        public List<ColorConfig.ColorProfile> Profiles
        {
            get { return _profiles; }
        }
    }
}