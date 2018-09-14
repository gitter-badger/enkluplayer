using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an object that applies and updates quality settings.
    /// </summary>
    public interface IAppQualityController
    {
        /// <summary>
        /// Sets up the player according to a specific element root
        /// configuration. Watches the root for updates.
        /// </summary>
        /// <param name="root">The element root.</param>
        void Setup(Element root);

        /// <summary>
        /// Stops watching for updates.
        /// </summary>
        void Teardown();
    }
}