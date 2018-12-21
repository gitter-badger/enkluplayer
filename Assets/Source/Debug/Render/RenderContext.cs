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
            var verts = _IcosahedronVertices;
            var tris = _IcosahedronTriangles;

            if (iterations > 0)
            {
                for (var i = 0; i < iterations; i++)
                {
                    Subdivide(ref verts, ref tris);
                }
            }

            Setup(GL.LINES);
            {
                for (int i = 0, len = tris.Length; i < len; i += 3)
                {
                    var va = verts[tris[i]].normalized;
                    var vb = verts[tris[i + 1]].normalized;
                    var vc = verts[tris[i + 2]].normalized;
                    
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
        /// Subdivides all triangles.
        /// </summary>
        public static void Subdivide(
            ref Vector3[] vertices,
            ref int[] triangles)
        {
            // cache of midpoint indices
            var midpointIndices = new Dictionary<string, int>();

            // create lists instead...
            List<int> indexList = new List<int>(4 * triangles.Length);
            List<Vector3> vertexList = new List<Vector3>(vertices);

            // subdivide each triangle
            for (var i = 0; i < triangles.Length - 2; i += 3)
            {
                // grab indices of triangle
                int i0 = triangles[i];
                int i1 = triangles[i + 1];
                int i2 = triangles[i + 2];

                // calculate new indices
                int m01 = GetMidpointIndex(midpointIndices, vertexList, i0, i1);
                int m12 = GetMidpointIndex(midpointIndices, vertexList, i1, i2);
                int m02 = GetMidpointIndex(midpointIndices, vertexList, i2, i0);

                indexList.AddRange(
                    new[] {
                        i0,m01,m02,
                        i1,m12,m01,
                        i2,m02,m12,
                        m02,m01,m12
                        });
            }

            // save
            triangles = indexList.ToArray();
            vertices = vertexList.ToArray();
        }

        /// <summary>
        /// Used by Subdivide method.
        /// </summary>
        private static int GetMidpointIndex(Dictionary<string, int> midpointIndices, List<Vector3> vertices, int i0, int i1)
        {
            // create a key
            string edgeKey = string.Format("{0}_{1}", Mathf.Min(i0, i1), Mathf.Max(i0, i1));

            int midpointIndex = -1;

            // if there is not index already...
            if (!midpointIndices.TryGetValue(edgeKey, out midpointIndex))
            {
                // grab the vertex values
                Vector3 v0 = vertices[i0];
                Vector3 v1 = vertices[i1];

                // calculate
                var midpoint = (v0 + v1) / 2f;

                // save
                if (vertices.Contains(midpoint))
                {
                    midpointIndex = vertices.IndexOf(midpoint);
                }
                else
                {
                    midpointIndex = vertices.Count;
                    vertices.Add(midpoint);
                }
            }

            return midpointIndex;
        }

        private static readonly Vector3[] _IcosahedronVertices =
        {
            new Vector3(-0.525731112119133606f, 0f, 0.850650808352039932f),
            new Vector3(0.525731112119133606f, 0f, 0.850650808352039932f),
            new Vector3(-0.525731112119133606f, 0f, -0.850650808352039932f),

            new Vector3(0.525731112119133606f, 0f, -0.850650808352039932f),
            new Vector3(0f, 0.850650808352039932f, 0.525731112119133606f),
            new Vector3(0f, 0.850650808352039932f, -0.525731112119133606f),

            new Vector3(0f, -0.850650808352039932f, 0.525731112119133606f),
            new Vector3(0f, -0.850650808352039932f, -0.525731112119133606f),
            new Vector3(0.850650808352039932f, 0.525731112119133606f, 0f),

            new Vector3(-0.850650808352039932f, 0.525731112119133606f, 0f),
            new Vector3(0.850650808352039932f, -0.525731112119133606f, 0f),
            new Vector3(-0.850650808352039932f, -0.525731112119133606f, 0f)
        };

        private static readonly int[] _IcosahedronTriangles =
        {
            1,4,0,
            4,9,0,
            4,5,9,
            8,5,4,
            1,8,4,
            1,10,8,
            10,3,8,
            8,3,5,
            3,2,5,
            3,7,2,
            3,10,7,
            10,6,7,
            6,11,7,
            6,0,11,
            6,1,0,
            10,1,6,
            11,0,9,
            2,11,9,
            5,2,9,
            11,2,7
        };
    }
}