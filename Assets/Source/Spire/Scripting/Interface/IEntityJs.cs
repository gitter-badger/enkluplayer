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

        /// <summary>
        /// Returns whether another element is a direct or indirect parent of this element.
        /// </summary>
        /// <param name="parent">Potential upstream element to check</param>
        /// <returns></returns>
        bool isChildOf(IEntityJs parent);
    }
}