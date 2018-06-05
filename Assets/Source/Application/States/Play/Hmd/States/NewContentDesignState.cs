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
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;
        
        /// <summary>
        /// Root of dynamic menus.
        /// </summary>
        private Element _dynamicRoot;
        
        /// <summary>
        /// New item menu.
        /// </summary>
        private NewContentController _newContent;

        /// <summary>
        /// Menu to place objects.
        /// </summary>
        private PlaceContentController _place;
        
        /// <inheritdoc />
        public void Initialize(
            HmdDesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
            _dynamicRoot = dynamicRoot;

            // new content
            {
                _newContent = unityRoot.AddComponent<NewContentController>();
                _newContent.enabled = false;
                _newContent.OnCancel += New_OnCancel;
                _newContent.OnConfirm += New_OnConfirm;
                dynamicRoot.AddChild(_newContent.Root);
            }

            // place content
            {
                _place = unityRoot.AddComponent<PlaceContentController>();
                _place.OnConfirm += Place_OnConfirm;
                _place.OnCancel += Place_OnCancel;
                _place.enabled = false;
            }
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            Object.Destroy(_newContent);
            Object.Destroy(_place);
        }
        
        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering {0}", GetType().Name);

            _newContent.enabled = true;
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            CloseAll();

            Log.Info(this, "Exited {0}", GetType().Name);
        }

        /// <summary>
        /// Closes all menus.
        /// </summary>
        private void CloseAll()
        {
            _newContent.enabled = false;
            _place.enabled = false;
        }
        
        /// <summary>
        /// Called when the new menu wants to create an element.
        /// </summary>
        private void New_OnConfirm(string assetId)
        {
            _newContent.enabled = false;

            _place.Initialize(assetId);
            _place.enabled = true;
        }

        /// <summary>
        /// Called when the new menu wants to cancel.
        /// </summary>
        private void New_OnCancel()
        {
            _newContent.enabled = false;
            _dynamicRoot.Schema.Set("focus.visible", true);
            
            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the place menu wants to confirm placement.
        /// </summary>
        /// <param name="contentData">The prop.</param>
        private void Place_OnConfirm(ElementData contentData)
        {
            _place.enabled = false;
            _dynamicRoot.Schema.Set("focus.visible", true);

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
                });

            // back to main
            _design.ChangeState<MainDesignState>();
        }
        
        /// <summary>
        /// Called when the place menu wants to cancel placement.
        /// </summary>
        private void Place_OnCancel()
        {
            _place.enabled = false;

            _newContent.enabled = true;
        }
    }
}