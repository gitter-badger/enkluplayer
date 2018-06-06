namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Bundles up all the states and flows between them. This is useful for all
    /// the targets we must support.
    /// </summary>
    public class ApplicationStatePackage
    {
        /// <summary>
        /// All possible states.
        /// </summary>
        public IState[] States { get; private set; }

        /// <summary>
        /// All possible flows through the states.
        /// </summary>
        public IStateFlow[] Flows { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ApplicationStatePackage(IState[] states, IStateFlow[] flows)
        {
            States = states;
            Flows = flows;
        }
    }
}