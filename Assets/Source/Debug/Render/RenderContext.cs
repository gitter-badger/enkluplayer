using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
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

        /// <summary>
        /// Stack of Matrix data.
        /// </summary>
        private readonly Stack<Matrix4x4> _matrices = new Stack<Matrix4x4>();

        /// <summary>
        /// Current matrix.
        /// </summary>
        private Matrix4x4 _current;
        
        /// <summary>
        /// Sets up for drawing. Must be followed with a Teardown.
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
        /// Must be proceeded by a Setup.
        /// </summary>
        protected virtual void Teardown()
        {
            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Resets state of render context.
        /// </summary>
        /// <returns></returns>
        public RenderContext Reset()
        {
            _color = Color.white;
            _current = Matrix4x4.identity;
            _matrices.Clear();

            return this;
        }

        /// <summary>
        /// Resets the current matrix.
        /// </summary>
        /// <returns></returns>
        public RenderContext ResetMatrix()
        {
            _current = Matrix4x4.identity;

            return this;
        }

        /// <summary>
        /// Pushes current matrix onto the stack, starts a new one.
        /// </summary>
        /// <returns></returns>
        public RenderContext PushMatrix()
        {
            _matrices.Push(_current);
            _current = Matrix4x4.identity;
            
            return this;
        }

        /// <summary>
        /// Pops the previous matrix off the stack.
        /// </summary>
        /// <returns></returns>
        public RenderContext PopMatrix()
        {
            _current = _matrices.Pop();

            return this;
        }

        /// <summary>
        /// Translates the current matrix.
        /// </summary>
        /// <param name="to">The target.</param>
        /// <returns></returns>
        public RenderContext Translate(Vector3 to)
        {
            _current *= Matrix4x4.Translate(to);

            return this;
        }
        
        /// <summary>
        /// Rotates along the X axis.
        /// </summary>
        /// <param name="radians">Radians to rotate.</param>
        /// <returns></returns>
        public RenderContext RotateX(float radians)
        {
            _current *= Matrix4x4.Rotate(Quaternion.Euler(
                radians * Mathf.Rad2Deg,
                0, 0));

            return this;
        }

        /// <summary>
        /// Rotates along the Y axis.
        /// </summary>
        /// <param name="radians">Radians to rotate.</param>
        /// <returns></returns>
        public RenderContext RotateY(float radians)
        {
            _current *= Matrix4x4.Rotate(Quaternion.Euler(
                0,
                radians * Mathf.Rad2Deg,
                0));

            return this;
        }

        /// <summary>
        /// Rotates along the Z axis.
        /// </summary>
        /// <param name="radians">Radians to rotate.</param>
        /// <returns></returns>
        public RenderContext RotateZ(float radians)
        {
            _current *= Matrix4x4.Rotate(Quaternion.Euler(
                0, 0,
                radians * Mathf.Rad2Deg));

            return this;
        }

        /// <summary>
        /// Rotates.
        /// </summary>
        /// <param name="rotation">The rotation.</param>
        /// <returns></returns>
        public RenderContext Rotate(Quaternion rotation)
        {
            _current *= Matrix4x4.Rotate(rotation);

            return this;
        }

        /// <summary>
        /// Rotates along each axis.
        /// </summary>
        /// <param name="x">Radians to rotate about x-axis.</param>
        /// <param name="y">Radians to rotate about y-axis.</param>
        /// <param name="z">Radians to rotate about z-axis.</param>
        /// <returns></returns>
        public RenderContext Rotate(float x, float y, float z)
        {
            return Rotate(Quaternion.Euler(
                x * Mathf.Rad2Deg,
                y * Mathf.Rad2Deg,
                z * Mathf.Rad2Deg));
        }

        /// <summary>
        /// Scales.
        /// </summary>
        /// <param name="to">Target scale.</param>
        /// <returns></returns>
        public RenderContext Scale(Vector3 to)
        {
            _current *= Matrix4x4.Scale(to);

            return this;
        }

        /// <summary>
        /// Sets the color to draw strokes with.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <returns></returns>
        public RenderContext Stroke(Color color)
        {
            return Stroke(color.r, color.g, color.b, color.a);
        }

        /// <summary>
        /// Sets stroke color.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        /// <returns></returns>
        public RenderContext Stroke(float r, float g, float b)
        {
            return Stroke(r, g, b, _color.a);
        }

        /// <summary>
        /// Sets stroke color.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        /// <param name="a">Alpha component.</param>
        /// <returns></returns>
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
                GL.Vertex(_current.MultiplyPoint3x4(from));
                GL.Vertex(_current.MultiplyPoint3x4(to));
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
                    GL.Vertex(_current.MultiplyPoint3x4(lines[i]));
                    GL.Vertex(_current.MultiplyPoint3x4(lines[i + 1]));
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
                    GL.Vertex(_current.MultiplyPoint3x4(lines[i]));
                }
            }
            Teardown();

            return this;
        }

        /// <summary>
        /// Renders triangles.
        /// </summary>
        /// <param name="vertices">Array of vertices..</param>
        /// /// <param name="indices">Indices into vertex array.</param>
        public void Triangles(ref Vector3[] vertices, ref int[] indices)
        {
            Setup(GL.LINES);
            {
                for (int i = 0, len = indices.Length; i < len; i += 3)
                {
                    var va = vertices[indices[i]];
                    var vb = vertices[indices[i + 1]];
                    var vc = vertices[indices[i + 2]];

                    var a = _current.MultiplyPoint3x4(va);
                    var b = _current.MultiplyPoint3x4(vb);
                    var c = _current.MultiplyPoint3x4(vc);

                    GL.Vertex(a); GL.Vertex(b);
                    GL.Vertex(b); GL.Vertex(c);
                    GL.Vertex(c); GL.Vertex(a);
                }
            }
            Teardown();
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
            var center = Vector3.zero;
            var right = w * Vector3.right / 2f;
            var forward = d * Vector3.forward / 2f;
            var up = h * Vector3.up / 2f;
            
            Setup(GL.LINES);
            {
                // bottom rect
                {
                    var bot = center - up;

                    GL.Vertex(_current.MultiplyPoint3x4(bot - right - forward));
                    GL.Vertex(_current.MultiplyPoint3x4(bot - right + forward));

                    GL.Vertex(_current.MultiplyPoint3x4(bot + right - forward));
                    GL.Vertex(_current.MultiplyPoint3x4(bot + right + forward));

                    GL.Vertex(_current.MultiplyPoint3x4(bot - right - forward));
                    GL.Vertex(_current.MultiplyPoint3x4(bot + right - forward));

                    GL.Vertex(_current.MultiplyPoint3x4(bot - right + forward));
                    GL.Vertex(_current.MultiplyPoint3x4(bot + right + forward));
                }
                
                // top rect
                {
                    var bot = center + up;

                    GL.Vertex(_current.MultiplyPoint3x4(bot - right - forward));
                    GL.Vertex(_current.MultiplyPoint3x4(bot - right + forward));

                    GL.Vertex(_current.MultiplyPoint3x4(bot + right - forward));
                    GL.Vertex(_current.MultiplyPoint3x4(bot + right + forward));

                    GL.Vertex(_current.MultiplyPoint3x4(bot - right - forward));
                    GL.Vertex(_current.MultiplyPoint3x4(bot + right - forward));

                    GL.Vertex(_current.MultiplyPoint3x4(bot - right + forward));
                    GL.Vertex(_current.MultiplyPoint3x4(bot + right + forward));
                }

                // connect rects
                {
                    GL.Vertex(_current.MultiplyPoint3x4(center + up - right - forward));
                    GL.Vertex(_current.MultiplyPoint3x4(center - up - right - forward));

                    GL.Vertex(_current.MultiplyPoint3x4(center + up - right + forward));
                    GL.Vertex(_current.MultiplyPoint3x4(center - up - right + forward));

                    GL.Vertex(_current.MultiplyPoint3x4(center + up + right + forward));
                    GL.Vertex(_current.MultiplyPoint3x4(center - up + right + forward));

                    GL.Vertex(_current.MultiplyPoint3x4(center + up + right - forward));
                    GL.Vertex(_current.MultiplyPoint3x4(center - up + right - forward));
                }
            }
            Teardown();

            return this;
        }

        /// <summary>
        /// Creates a sphere.
        /// </summary>
        /// <param name="size">The size of sphere.</param>
        public void Sphere()
        {
            Sphere(0);
        }

        /// <summary>
        /// Creates a sphere of variable smoothness.
        /// </summary>
        /// <param name="iterations">The number of iterations to use to smooth out the sphere.</param>
        public void Sphere(int iterations)
        {
            Vector3[] verts;
            int[] tris;
            GeometryHelper.GeoSphere(iterations, out verts, out tris);

            Triangles(ref verts, ref tris);
        }
    }
}