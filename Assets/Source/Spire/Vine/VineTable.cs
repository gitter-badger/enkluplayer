using UnityEngine;

namespace CreateAR.SpirePlayer.Vine
{
    /// <summary>
    /// Lookup table for vines.
    /// </summary>
    public class VineTable : MonoBehaviour, IVineTable
    {
        /// <summary>
        /// References.
        /// </summary>
        public VineReference[] References;

        /// <inheritdoc />
        public VineReference Vine(string identifier)
        {
            for (int i = 0, len = References.Length; i < len; i++)
            {
                var reference = References[i];
                if (reference.Identifier == identifier)
                {
                    return reference;
                }
            }

            return null;
        }
    }
}