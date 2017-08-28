using CreateAR.Commons.Unity.DebugRenderer;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Static utility for easy rendering.
    /// </summary>
    public static class Render
    {
        /// <summary>
        /// Renderer. Get/set by Application.
        /// </summary>
        public static DebugRenderer Renderer;

        /// <inheritdoc cref="DebugRenderer"/>
        public static IFilteredRendererHandle Handle(string category)
        {
            if (null == Renderer)
            {
                return null;
            }

            return Renderer.Handle(category);
        }

        /// <inheritdoc cref="DebugRenderer"/>
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