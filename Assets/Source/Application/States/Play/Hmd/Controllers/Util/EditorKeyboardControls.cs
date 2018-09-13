using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Hacky controls used in the editor.
    /// </summary>
    public class HmdEditorKeyboardControls : InjectableMonoBehaviour
    {
        /// <summary>
        /// The main camera.
        /// </summary>
        [Inject]
        public MainCamera Camera { get; set; }

        /// <summary>
        /// Speed of camera.
        /// </summary>
        public float Speed = 0.1f;

        /// <inheritdoc cref="MonoBehaviour"/>
        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                Camera.transform.position -= Speed * Camera.transform.right;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                Camera.transform.position += Speed * Camera.transform.right;
            }

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    Camera.transform.position -= Speed * Camera.transform.up;
                }

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    Camera.transform.position += Speed * Camera.transform.up;
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    Camera.transform.position -= Speed * Camera.transform.forward;
                }

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    Camera.transform.position += Speed * Camera.transform.forward;
                }
            }
        }
    }
}