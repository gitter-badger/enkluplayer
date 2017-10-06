namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Defines how an ILayerable assigns its layer.
    /// </summary>
    public enum LayerMode
    {
        Default,
        Modal,
        Hide
    }

    /// <summary>
    /// Represents a layer of UI.
    /// </summary>
    public class Layer
    {
        /// <summary>
        /// A reference to the ILayerable on this layer.
        /// </summary>
        public readonly ILayerable Owner;

        /// <summary>
        /// Creates a new layer with a specific owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public Layer(ILayerable owner)
        {
            Owner = owner;
        }
    }
}