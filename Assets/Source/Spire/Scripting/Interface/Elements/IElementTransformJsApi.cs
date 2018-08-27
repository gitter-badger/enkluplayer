using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// Provides common transform specifics for an element.
    /// </summary>
    public interface IElementTransformJsApi
    {
        /// <summary>
        /// Position.
        /// </summary>
        Vec3 position { get; set; }

        /// <summary>
        /// Rotation.
        /// </summary>
        Quat rotation { get; set; }
        
        /// <summary>
        /// Scale.
        /// </summary>
        Vec3 scale { get; set; }
    }
}