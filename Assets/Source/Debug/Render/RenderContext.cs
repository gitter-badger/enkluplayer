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
            _color = UnityEngine.Color.white;
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

        public RenderContext Rotate(float x, float y, float z)
        {
            _current *= Matrix4x4.Rotate(Quaternion.Euler(
                x * Mathf.Rad2Deg,
                y * Mathf.Rad2Deg,
                z * Mathf.Rad2Deg));

            return this;
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
        public RenderContext Color(Color color)
        {
            return Color(color.r, color.g, color.b, color.a);
        }

        public RenderContext Color(float r, float g, float b)
        {
            return Color(r, g, b, _color.a);
        }

        public RenderContext Color(float r, float g, float b, float a)
        {
            _color = new Color(r, g, b, a);

            return this;
        }

        public RenderContext Alpha(float a)
        {
            return Color(_color.r, _color.g, _color.b, a);
        }

        public RenderContext Red(float r)
        {
            return Color(r, _color.g, _color.b, _color.a);
        }

        public RenderContext Green(float r)
        {
            return Color(r, _color.g, _color.b, _color.a);
        }

        public RenderContext Blue(float r)
        {
            return Color(r, _color.g, _color.b, _color.a);
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
        /// <param name="center">The center of the cube.</param>
        /// <param name="size">The size of the cube.</param>
        /// <returns></returns>
        public RenderContext Cube(Vector3 center, float size)
        {
            return Prism(new Bounds(center, size * Vector3.one));
        }

        /// <summary>
        /// Draws a prism.
        /// </summary>
        /// <param name="bounds">The bounds to draw.</param>
        /// <returns></returns>
        public RenderContext Prism(Bounds bounds)
        {
            var min = bounds.min;
            var max = bounds.max;
            Setup(GL.LINES);
            {
                // bottom rect
                {
                    GL.Vertex3(min.x, min.y, min.z);
                    GL.Vertex3(min.x, min.y, max.z);

                    GL.Vertex3(min.x, min.y, max.z);
                    GL.Vertex3(max.x, min.y, max.z);

                    GL.Vertex3(max.x, min.y, max.z);
                    GL.Vertex3(max.x, min.y, min.z);

                    GL.Vertex3(max.x, min.y, min.z);
                    GL.Vertex3(min.x, min.y, min.z);
                }

                // render flat prisms differently
                if (Mathf.Abs(min.y - max.y) > Mathf.Epsilon)
                {
                    // top rect
                    {
                        GL.Vertex3(min.x, max.y, min.z);
                        GL.Vertex3(min.x, max.y, max.z);

                        GL.Vertex3(min.x, max.y, max.z);
                        GL.Vertex3(max.x, max.y, max.z);

                        GL.Vertex3(max.x, max.y, max.z);
                        GL.Vertex3(max.x, max.y, min.z);

                        GL.Vertex3(max.x, max.y, min.z);
                        GL.Vertex3(min.x, max.y, min.z);
                    }

                    // connect rects
                    {
                        GL.Vertex3(min.x, min.y, min.z);
                        GL.Vertex3(min.x, max.y, min.z);

                        GL.Vertex3(min.x, min.y, max.z);
                        GL.Vertex3(min.x, max.y, max.z);

                        GL.Vertex3(max.x, min.y, max.z);
                        GL.Vertex3(max.x, max.y, max.z);

                        GL.Vertex3(max.x, min.y, min.z);
                        GL.Vertex3(max.x, max.y, min.z);
                    }
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