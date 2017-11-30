namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Interface for intention. This is for the purposes of abstracting away
    /// Unity interfaces.
    /// </summary>
    public interface IIntentionManager
    {
        /// <summary>
        /// If true, input is not updated.
        /// </summary>
        bool InputDisabled { get; }

        /// <summary>
        /// Current focus.
        /// </summary>
        IInteractable Focus { get; }

        /// <summary>
        /// Forward direction.
        /// </summary>
        Vec3 Origin { get; }

        /// <summary>
        /// Forward direction.
        /// </summary>
        Vec3 Forward { get; }

        /// <summary>
        /// Up direction.
        /// </summary>
        Vec3 Up { get; }

        /// <summary>
        /// Right direction.
        /// </summary>
        Vec3 Right { get; }

        /// <summary>
        /// Measure of stability;
        /// </summary>
        float Stability { get; }
    }
}