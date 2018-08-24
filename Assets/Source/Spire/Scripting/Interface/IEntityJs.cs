namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// A base interface for anything available in a scene, including Elements and the Player
    /// </summary>
    public interface IEntityJs
    {
        /// <summary>
        /// The transform interface.
        /// </summary>
        IElementTransformJsApi transform { get; }
    }
}