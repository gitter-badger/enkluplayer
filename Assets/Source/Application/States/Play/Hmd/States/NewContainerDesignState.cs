using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Creates a new anchor.
    /// </summary>
    public class NewContainerDesignState : IArDesignState
    {
        /// <summary>
        /// Manages UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Constructor.
        /// </summary>
        public NewContainerDesignState(
            IUIManager ui)
        {
            _ui = ui;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _frame = _ui.CreateFrame();

            // open placement menu
            int id;
            _ui
                .Open<PlaceContainerUIView>(new UIReference
                {
                    UIDataId = "Container.Place"
                }, out id)
                .OnSuccess(el =>
                {
                    el.OnOk += Place_OnOk;
                    el.OnCancel += () => _design.ChangeState<MainDesignState>();
                    el.Initialize(_design.Config);
                }).OnFailure(el =>
                {
                    Debug.Log(el.Message);
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            // 
        }

        /// <inheritdoc />
        public void Exit()
        {
            _frame.Release();
        }

        /// <inheritdoc />
        public void Initialize(HmdDesignController designer, GameObject unityRoot, Element dynamicRoot, Element staticRoot)
        {
            _design = designer;
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            // 
        }

        /// <summary>
        /// Called when user wishes to place an anchor in a spot.
        /// </summary>
        /// <param name="data">The ElementData to save.</param>
        private void Place_OnOk(ElementData data)
        {
            //kill menu
            _ui.Pop();

            //TODO: open progress indicator

            //create container
            _design
            .Elements
            .Create(data)
            .OnSuccess(element =>
            {
                Log.Info(this, "Successfully created content : {0}.", element);
            })
            .OnFailure(exception =>
            {
                Log.Error(this, "Could not place content : {0}.", exception);
            })
            .OnFinally(_ =>
            {
                // TODO: close progress indicator

                _design.ChangeState<MainDesignState>();
            });
        }
    }
}