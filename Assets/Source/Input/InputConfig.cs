using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class InputConfig : MonoBehaviour
    {
        [Range(0f, 1f)]
        public float TranslateMultiplier = 0.1f;

        [Range(0f, 1f)]
        public float RotateMultiplier = 0.1f;
    }
}