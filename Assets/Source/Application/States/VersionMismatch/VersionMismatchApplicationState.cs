using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State used when there is a version mismatch with the server-- this is
    /// the end of the line!
    /// </summary>
    public class VersionMismatchApplicationState : IState
    {
        /// <summary>
        /// Manipulates UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Constructor.
        /// </summary>
        public VersionMismatchApplicationState(IUIManager ui)
        {
            _ui = ui;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _ui
                .Open<ICommonErrorView>(new UIReference
                {
                    UIDataId = UIDataIds.ERROR
                })
                .OnSuccess(el =>
                {
                    el.Message = "This version of Enklu is ancient! Please upgrade to access your experiences.";
                    el.DisableAction();
                })
                .OnFailure(ex => Log.Fatal(this, "COuld not open error popup : {0}.", ex));
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            
        }
    }
}