namespace CreateAR.SpirePlayer.IUX
{
    public interface ILayerManager
    {
        /// <summary>
        /// Top-most layer which is set to Modal.
        /// </summary>
        Layer ModalLayer { get; }

        /// <summary>
        /// Creates a new layer.
        /// </summary>
        /// <returns></returns>
        Layer Request(ILayerable owner);

        /// <summary>
        /// Release an existing layer.
        /// </summary>
        /// <param name="layer">The Layer to remove.</param>
        void Release(Layer layer);
    }
}
