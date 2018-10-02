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
        /// Returns the position of this transform relative to another entity. This value should not
        /// be cached as elements aren't guaranteed to sit under the same world anchor.
        ///
        /// TODO: Make this more friendly/understandable for people unfamiliar with anchoring woes.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Vec3 positionRelativeTo(IEntityJs entity);

        /// <summary>
        /// World position. DO NOT cache this value, as it shifts with world anchor readjustment.
        /// </summary>
        [DenyJsAccess]
        Vec3 worldPosition { get; }
    }
}