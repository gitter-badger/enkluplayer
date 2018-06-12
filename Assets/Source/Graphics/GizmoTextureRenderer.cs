using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Renders a texture centered at the GameObject's position.
    /// </summary>
    public class GizmoTextureRenderer : MonoBehaviour
    {
        /// <summary>
        /// The texture.
        /// </summary>
        public Texture2D Texture;

        /// <summary>
        /// Size in screenspace.
        /// </summary>
        public Vector2 ScreenSize;

        /// <inheritdoc cref="MonoBehaviour"/>
        private void OnRenderObject()
        {
            var cam = GetCamera();
            var screen = cam.WorldToScreenPoint(transform.position);
            GL.PushMatrix();
            {
                GL.LoadPixelMatrix(0, Screen.width, 0, Screen.height);
                Graphics.DrawTexture(
                    new Rect(
                        screen.x - ScreenSize.x / 2f,
                        screen.y - ScreenSize.y / 2f,
                        ScreenSize.x,
                        -ScreenSize.y),
                    Texture);
            }
            GL.PopMatrix();
        }

        /// <summary>
        /// Retrieves a good camera to use.
        /// </summary>
        /// <returns></returns>
        private Camera GetCamera()
        {
            return Camera.main ?? Camera.current;
        }
    }
}