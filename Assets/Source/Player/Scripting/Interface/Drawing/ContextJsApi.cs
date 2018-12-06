using CreateAR.EnkluPlayer.Scripting;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    public class ContextJsApi
    {
        private Color _color;

        public ContextJsApi()
        {
            ResetState();
        }

        [DenyJsAccess]
        public void ResetState()
        {
            _color = Color.white;
        }
        
        public ContextJsApi color(Col4 col)
        {
            color(col.r, col.g, col.b, col.a);

            return this;
        }

        public ContextJsApi color(double r, double g, double b)
        {
            color(r, g, b, _color.a);

            return this;
        }

        public ContextJsApi color(double r, double g, double b, double a)
        {
            _color = new Color((float) r, (float) g, (float) b, (float) a);

            return this;
        }

        public ContextJsApi alpha(double a)
        {
            color(_color.r, _color.g, _color.b, a);

            return this;
        }

        public ContextJsApi red(double r)
        {
            color(r, _color.g, _color.b, _color.a);

            return this;
        }

        public ContextJsApi green(double g)
        {
            color(_color.r, g, _color.b, _color.a);

            return this;
        }

        public ContextJsApi blue(double b)
        {
            color(_color.r, _color.g, b, _color.a);

            return this;
        }

        public ContextJsApi line(Vec3 from, Vec3 to)
        {
            line(from.x, from.y, from.z, to.x, to.y, to.z);

            return this;
        }

        public ContextJsApi line(
            double xf, double yf, double zf,
            double xt, double yt, double zt)
        {
            GL.Begin(GL.LINES);
            GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
            GL.Color(_color);
            {
                GL.Vertex(new Vector3((float) xf, (float) yf, (float) zf));
                GL.Vertex(new Vector3((float) xt, (float) yt, (float) zt));
            }
            GL.End();

            return this;
        }

        public ContextJsApi lines(Vec3[] points)
        {
            GL.Begin(GL.LINES);
            GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
            GL.Color(_color);
            {
                for (int i = 0, len = points.Length; i < len; i += 2)
                {
                    GL.Vertex(points[i].ToVector());
                    GL.Vertex(points[i + 1].ToVector());
                }
            }
            GL.End();

            return this;
        }

        public ContextJsApi linestrip(Vec3[] points)
        {
            GL.Begin(GL.LINE_STRIP);
            GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
            GL.Color(_color);
            {
                for (int i = 0, len = points.Length; i < len; i++)
                {
                    GL.Vertex(points[i].ToVector());
                }
            }
            GL.End();

            return this;
        }
    }
}