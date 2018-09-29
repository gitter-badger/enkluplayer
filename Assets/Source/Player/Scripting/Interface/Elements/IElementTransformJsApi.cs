namespace CreateAR.EnkluPlayer.Scripting
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

        /// <summary>
        /// World position. Do not cache this value as world anchors will shift.
        /// </summary>
        Vec3 worldPosition { get; }
    }
}