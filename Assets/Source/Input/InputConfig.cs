using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Configuration for input.
    /// </summary>
    public class InputConfig : MonoBehaviour
    {
        [Tooltip("Multiplier for translation speed.")]
        [Range(0f, 1f)]
        public float TranslateMultiplier = 0.1f;

        [Tooltip("Multiplier for rotation speed.")]
        [Range(0f, 1f)]
        public float RotateMultiplier = 0.1f;
    }
}