using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// MonoBehaviour based implementation of <c>IFontConfig</c>.
    /// </summary>
    public class FontConfig : MonoBehaviour, IFontConfig
    {
        /// <summary>
        /// Backing variable for Current property.
        /// </summary>
        private FontProfile _profile;

        /// <summary>
        /// List of all profiles.
        /// </summary>
        public FontProfile[] Profiles;

        /// <inheritdoc cref="IFontConfig"/>
        public FontProfile Current
        {
            get
            {
                if (null == _profile)
                {
                    if (Profiles.Length > 0)
                    {
                        _profile = Profiles[0];
                    }
                }

                return _profile;
            }
            set
            {
                _profile = value;
            }
        }

        /// <inheritdoc cref="IFontConfig"/>
        public bool Font(string fontName, out Font font)
        {
            var profile = Current;
            if (null != profile)
            {
                return profile.Font(fontName, out font);
            }

            font = null;
            return false;
        }
    }
}