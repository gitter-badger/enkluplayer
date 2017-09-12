using UnityEngine;

namespace CreateAR.SpirePlayer
{
    [RequireComponent(typeof(RectTransform))]
    public class Spinner : MonoBehaviour
    {
        public float Speed = 0.5f;

        private RectTransform _transform;

        private void Awake()
        {
            _transform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            _transform.Rotate(Vector3.forward, Time.deltaTime * Speed);
        }
    }
}