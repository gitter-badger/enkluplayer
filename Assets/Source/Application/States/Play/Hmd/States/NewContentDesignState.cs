using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State for creating new content.
    /// </summary>
    public class NewContentDesignState : IArDesignState
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
        /// Id of new content UI.
        /// </summary>
        private int _newContentId;

        /// <summary>
        /// Id of place content UI.
        /// </summary>
        private int _placeContentId;

        /// <summary>
        /// Constructor.
        /// </summary>
        public NewContentDesignState(IUIManager ui)
        {
            _ui = ui;
        }

        /// <inheritdoc />
        public void Initialize(
            HmdDesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
        }
        
        /// <inheritdoc />
        public void Uninitialize()
        {
            // 
        }
        
        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering {0}", GetType().Name);

            _frame = _ui.CreateFrame();

            OpenGrid();
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            _frame.Release();

            Log.Info(this, "Exited {0}", GetType().Name);
        }

        /// <summary>
        /// Opens grid of new elements.
        /// </summary>
        private void OpenGrid()
        {
            _ui
                .Open<NewContentUIView>(new UIReference
                {
                    UIDataId = "Content.New"
                }, out _newContentId)
                .OnSuccess(el =>
                {
                    el.OnCancel += New_OnCancel;
                    el.OnConfirm += New_OnConfirm;
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not open NewContentUIView : {0}", exception);

                    _design.ChangeState<MainDesignState>();
                });
        }

        /// <summary>
        /// Closes new element grid.
        /// </summary>
        private void CloseGrid()
        {
            _ui.Close(_newContentId);
        }

        /// <summary>
        /// Opens placement UI.
        /// </summary>
        /// <param name="assetId">The id of the asset to place.</param>
        private void OpenPlacement(string assetId)
        {
            _ui
                .Open<PlaceContentUIView>(new UIReference
                {
                    UIDataId = "Content.Place"
                }, out _placeContentId)
                .OnSuccess(el =>
                {
                    el.OnConfirm += Place_OnConfirm;
                    el.OnCancel += Place_OnCancel;
                    el.Initialize(assetId);
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not open PlaceContentUIView : {0}", exception);

                    ClosePlacement();
                    OpenGrid();
                });
        }

        /// <summary>
        /// Close the placement UI.
        /// </summary>
        private void ClosePlacement()
        {
            _ui.Close(_placeContentId);
        }

        /// <summary>
        /// Called when the new menu wants to create an element.
        /// </summary>
        private void New_OnConfirm(string assetId)
        {
            CloseGrid();
            OpenPlacement(assetId);
        }

        /// <summary>
        /// Called when the new menu wants to cancel.
        /// </summary>
        private void New_OnCancel()
        {
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the place menu wants to confirm placement.
        /// </summary>
        /// <param name="contentData">The prop.</param>
        private void Place_OnConfirm(ElementData contentData)
        {
            ClosePlacement();
            
            // TODO: progress indicator

            // create!
            _design
                .Elements
                .Create(contentData)
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
        
        /// <summary>
        /// Called when the place menu wants to cancel placement.
        /// </summary>
        private void Place_OnCancel()
        {
            ClosePlacement();
            OpenGrid();
        }
    }
}