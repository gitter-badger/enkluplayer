namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Controls debug rendering.
    /// </summary>
    public class DebugRenderingJsApi
    {
        public bool enabled
        {
            get { return Render.Renderer.Enabled; }
            set { Render.Renderer.Enabled = value; }
        }

        public string filter
        {
            get { return Render.Renderer.Filter; }
            set { Render.Renderer.Filter = value; }
        }
    }
}