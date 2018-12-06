using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// An object that actually makes the GL calls.
    /// </summary>
    public class RenderContext
    {
        /// <summary>
        /// The color to draw with.
        /// </summary>
        protected Color _color;

        private readonly Stack<Matrix4x4> _matrices = new Stack<Matrix4x4>();
        private Matrix4x4 _current;

        /// <summary>
        /// Sets up for drawing. Must be followed with a Tearfown.
        /// </summary>
        /// <param name="mode">The GL mode to draw with.</param>
        protected virtual void Setup(int mode)
        {
            GL.PushMatrix();
            GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
            GL.Begin(mode);
            GL.Color(_color);
        }

        /// <summary>
        /// Must be paired with a preceeding Setup.
        /// </summary>
        protected virtual void Teardown()
        {
            GL.End();
            GL.PopMatrix();
        }

        public RenderContext Reset()
        {
            _color = Color.white;
            _current = Matrix4x4.identity;
            _matrices.Clear();

            return this;
        }

        public RenderContext ResetMatrix()
        {
            return this;
        }

        public RenderContext PushMatrix()
        {
            _matrices.Push(_current);
            _current = Matrix4x4.identity;
            
            return this;
        }

        public RenderContext PopMatrix()
        {
            _current = _matrices.Pop();

            return this;
        }

        public RenderContext Translate(Vector3 to)
        {
            _current *= Matrix4x4.Translate(to);

            return this;
        }

        public RenderContext RotateX(float radians)
        {
            _current *= Matrix4x4.Rotate(Quaternion.Euler(
                radians * Mathf.Rad2Deg,
                0, 0));

            return this;
        }

        public RenderContext RotateY(float radians)
        {
            _current *= Matrix4x4.Rotate(Quaternion.Euler(
                0,
                radians * Mathf.Rad2Deg,
                0));

            return this;
        }

        public RenderContext RotateZ(float radians)
        {
            _current *= Matrix4x4.Rotate(Quaternion.Euler(
                0, 0,
                radians * Mathf.Rad2Deg));

            return this;
        }

        public RenderContext Rotate(Quaternion rotation)
        {
            _current *= Matrix4x4.Rotate(rotation);

            return this;
        }

        public RenderContext Rotate(float x, float y, float z)
        {
            return Rotate(Quaternion.Euler(
                x * Mathf.Rad2Deg,
                y * Mathf.Rad2Deg,
                z * Mathf.Rad2Deg));
        }

        public RenderContext Scale(Vector3 to)
        {
            _current *= Matrix4x4.Scale(to);

            return this;
        }

        /// <summary>
        /// Sets the color to draw with.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <returns></returns>
        public RenderContext Stroke(Color color)
        {
            return Stroke(color.r, color.g, color.b, color.a);
        }

        public RenderContext Stroke(float r, float g, float b)
        {
            return Stroke(r, g, b, _color.a);
        }

        public RenderContext Stroke(float r, float g, float b, float a)
        {
            _color = new Color(r, g, b, a);

            return this;
        }
        
        /// <summary>
        /// Draws a line segment.
        /// </summary>
        /// <param name="from">The starting point.</param>
        /// <param name="to">The end point.</param>
        /// <returns></returns>
        public RenderContext Line(Vector3 from, Vector3 to)
        {
            Setup(GL.LINES);
            {
                GL.Vertex(from);
                GL.Vertex(to);
            }
            Teardown();

            return this;
        }

        /// <summary>
        /// Draws a series of lines
        /// </summary>
        /// <param name="lines">A list of pairs of line segments.</param>
        /// <returns></returns>
        public RenderContext Lines(Vector3[] lines)
        {
            Setup(GL.LINES);
            {
                for (int i = 0, len = lines.Length; i < len; i += 2)
                {
                    GL.Vertex(lines[i]);
                    GL.Vertex(lines[i + 1]);
                }
            }
            Teardown();

            return this;
        }

        /// <summary>
        /// Draws a line strip.
        /// </summary>
        /// <param name="lines">A list of waypoints to draw along.</param>
        /// <returns></returns>
        public RenderContext LineStrip(Vector3[] lines)
        {
            Setup(GL.LINE_STRIP);
            {
                for (int i = 0, len = lines.Length; i < len; i++)
                {
                    GL.Vertex(lines[i]);
                }
            }
            Teardown();

            return this;
        }

        /// <summary>
        /// Draws a cube.
        /// </summary>
        /// <param name="size">The size of the cube.</param>
        /// <returns></returns>
        public RenderContext Cube(float size)
        {
            return Prism(size, size, size);
        }

        /// <summary>
        /// Draws a prism.
        /// </summary>
        public RenderContext Prism(float w, float h, float d)
        {
            var center = _current.MultiplyPoint3x4(Vector3.zero);
            var right = _current.MultiplyPoint3x4(w * Vector3.right) / 2f;
            var forward = _current.MultiplyPoint3x4(d * Vector3.forward) / 2f;
            var up = _current.MultiplyPoint3x4(h * Vector3.up) / 2f;
            
            Setup(GL.LINES);
            {
                // bottom rect
                {
                    var bot = center - up;

                    GL.Vertex(bot - right - forward);
                    GL.Vertex(bot - right + forward);

                    GL.Vertex(bot + right - forward);
                    GL.Vertex(bot + right + forward);

                    GL.Vertex(bot - right - forward);
                    GL.Vertex(bot + right - forward);

                    GL.Vertex(bot - right + forward);
                    GL.Vertex(bot + right + forward);
                }
                
                // top rect
                {
                    var bot = center + up;

                    GL.Vertex(bot - right - forward);
                    GL.Vertex(bot - right + forward);

                    GL.Vertex(bot + right - forward);
                    GL.Vertex(bot + right + forward);

                    GL.Vertex(bot - right - forward);
                    GL.Vertex(bot + right - forward);

                    GL.Vertex(bot - right + forward);
                    GL.Vertex(bot + right + forward);
                }

                // connect rects
                {
                    GL.Vertex(center + up - right - forward);
                    GL.Vertex(center - up - right - forward);

                    GL.Vertex(center + up - right + forward);
                    GL.Vertex(center - up - right + forward);

                    GL.Vertex(center + up + right + forward);
                    GL.Vertex(center - up + right + forward);

                    GL.Vertex(center + up + right - forward);
                    GL.Vertex(center - up + right - forward);
                }
            }
            Teardown();

            return this;
        }

        /// <summary>
        /// Draws a plus sign.
        /// </summary>
        /// <param name="center">The center of the plus.</param>
        /// <param name="size">The size of the plus' extents.</param>
        /// <returns></returns>
        public RenderContext Plus(Vector3 center, float size)
        {
            Setup(GL.LINES);
            {
                GL.Vertex(center);
                GL.Vertex(center + size * Vector3.up);

                GL.Vertex(center);
                GL.Vertex(center + size * Vector3.right);

                GL.Vertex(center);
                GL.Vertex(center + size * Vector3.forward);

                GL.Vertex(center);
                GL.Vertex(center - size * Vector3.up);

                GL.Vertex(center);
                GL.Vertex(center - size * Vector3.right);

                GL.Vertex(center);
                GL.Vertex(center - size * Vector3.forward);
            }
            Teardown();

            return this;
        }
    }
}