using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class LoadProgressBehaviour : MonoBehaviour
    {
        public Transform Container;
        public Transform Spinner;

        public float Speed = 0.1f;
        public Bounds Bounds = new Bounds(
            Vector3.zero,
            Vector3.one);
        
        private void Update()
        {
            // place + scale
            transform.position = Bounds.center;
            transform.localScale = Bounds.size;

            Container.localScale = new Vector3(
                1 / Bounds.size.x,
                1 / Bounds.size.y,
                1 / Bounds.size.z);

            var value = Speed * Time.deltaTime;

            Spinner.Rotate(
                Spinner.worldToLocalMatrix.MultiplyVector(Vector3.up),
                value);
        }
    }
}