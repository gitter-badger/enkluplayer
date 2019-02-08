using CreateAR.EnkluPlayer.Scripting;
using Enklu.Data;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    public class ContextJsApi
    {
        private readonly RenderContext _context = new RenderContext();
        
        public ContextJsApi()
        {
            ResetState();
        }

        [DenyJsAccess]
        public void ResetState()
        {
            _context.Reset();
        }
        
        public ContextJsApi stroke(Col4 col)
        {
            _context.Stroke(col.r, col.g, col.b, col.a);

            return this;
        }

        public ContextJsApi stroke(double r, double g, double b)
        {
            _context.Stroke((float) r, (float) g, (float) b);

            return this;
        }

        public ContextJsApi stroke(double r, double g, double b, double a)
        {
            _context.Stroke(new Color((float) r, (float) g, (float) b, (float) a));

            return this;
        }
        
        public ContextJsApi line(Vec3 from, Vec3 to)
        {
            _context.Line(from.ToVector(), to.ToVector());

            return this;
        }

        public ContextJsApi line(
            double xf, double yf, double zf,
            double xt, double yt, double zt)
        {
            _context.Line(
                new Vector3((float) xf, (float) yf, (float) zf),
                new Vector3((float) xt, (float) yt, (float) zt));

            return this;
        }

        public ContextJsApi lines(Vec3[] points)
        {
            var vectors = new Vector3[points.Length];
            for (int i = 0, len = points.Length; i < len; i++)
            {
                vectors[i] = points[i].ToVector();
            }

            _context.Lines(vectors);

            return this;
        }

        public ContextJsApi linestrip(Vec3[] points)
        {
            var vectors = new Vector3[points.Length];
            for (int i = 0, len = points.Length; i < len; i++)
            {
                vectors[i] = points[i].ToVector();
            }

            _context.LineStrip(vectors);

            return this;
        }

        public ContextJsApi triangles(object[] vertices, object[] indices)
        {
            var verticesLen = vertices.Length;
            var verts = new Vector3[verticesLen];
            var i = 0;
            for (i = 0; i < verticesLen; i++)
            {
                verts[i] = ((Vec3) vertices[i]).ToVector();
            }

            var indicesLen = indices.Length;
            var tris = new int[indicesLen];
            for (i = 0; i < indicesLen; i++)
            {
                var index = indices[i];
                if (null == index)
                {
                    continue;
                }

                var fl = (float) (double) indices[i];
                tris[i] = Mathf.RoundToInt(fl);
            }

            _context.Triangles(ref verts, ref tris);

            return this;
        }

        public ContextJsApi resetMatrix()
        {
            _context.ResetMatrix();
            
            return this;
        }

        public ContextJsApi pushMatrix()
        {
            _context.PushMatrix();

            return this;
        }

        public ContextJsApi popMatrix()
        {
            _context.PopMatrix();

            return this;
        }

        public ContextJsApi translate(Vec3 to)
        {
            _context.Translate(to.ToVector());

            return this;
        }

        public ContextJsApi translate(double x, double y, double z)
        {
            _context.Translate(new Vector3((float) x, (float) y, (float) z));

            return this;
        }

        public ContextJsApi rotateX(double rad)
        {
            _context.RotateX((float) rad);

            return this;
        }

        public ContextJsApi rotateY(double rad)
        {
            _context.RotateY((float) rad);

            return this;
        }

        public ContextJsApi rotateZ(double rad)
        {
            _context.RotateZ((float) rad);

            return this;
        }

        public ContextJsApi rotate(double x, double y, double z)
        {
            _context.Rotate((float) x, (float) y, (float) z);

            return this;
        }

        public ContextJsApi scale(double scalar)
        {
            _context.Scale((float) scalar * Vector3.one);

            return this;
        }

        public ContextJsApi scale(Vec3 scale)
        {
            _context.Scale(scale.ToVector());
            
            return this;
        }

        public ContextJsApi box(double w, double h, double d)
        {
            _context.Prism((float) w, (float) h, (float) d);

            return this;
        }

        public ContextJsApi box(double size)
        {
            return box(size, size, size);
        }

        public ContextJsApi sphere()
        {
            _context.Sphere();

            return this;
        }

        public ContextJsApi sphere(double iterations)
        {
            _context.Sphere(Mathf.RoundToInt((float) iterations));

            return this;
        }

        public ContextJsApi octohedron()
        {
            _context.Octohedron();

            return this;
        }

        public ContextJsApi dodecahedron()
        {
            _context.Dodecahedron();

            return this;
        }
    }
}