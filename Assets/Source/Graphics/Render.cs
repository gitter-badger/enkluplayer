namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Static utility for easy rendering.
    /// </summary>
    public static class Render
    {
        /// <summary>
        /// Renderer. Get/set by Application.
        /// </summary>
        public static DebugRenderController Renderer;

        /// <inheritdoc cref="DebugRenderController"/>
        public static IFilteredRendererHandle Handle(string category)
        {
            if (null == Renderer)
            {
                return null;
            }

            return Renderer.Handle(category);
        }

        /// <inheritdoc cref="DebugRenderController"/>
        public static IFilteredRendererHandle2D Handle2D(string category)
        {
            if (null == Renderer)
            {
                return null;
            }

            return Renderer.Handle2D(category);
        }
    }
}