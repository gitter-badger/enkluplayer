using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer
{
    public interface IWidget : IElement
    {
        /// <summary>
        /// Associated Game Object in the Unity Hierarchy.
        /// </summary>
        UnityEngine.GameObject GameObject { get; }

        /// <summary>
        /// Parent widget.
        /// </summary>
        IWidget Parent { get; set; }

        /// <summary>
        /// Render color (composition of local and parent colors).
        /// </summary>
        Col4 Color { get; }

        /// <summary>
        /// Local color (unaffected by parent color).
        /// </summary>
        Col4 LocalColor { get; set; }

        /// <summary>
        /// Visible flag (composition of local parent visibility).
        /// </summary>
        bool Visible { get; }

        /// <summary>
        /// Visibility Accessor/Mutator.
        /// </summary>
        bool LocalVisible { get; set; }

        /// <summary>
        /// [0..1] The current fade of the Widget. 
        /// </summary>
        float Tween { get; }

        /// <summary>
        /// The current layer of the Widget.
        /// </summary>
        Layer Layer { get; }
    }
}
