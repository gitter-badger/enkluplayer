﻿using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls editing content.
    /// </summary>
    public class EditContentDesignState : IArDesignState
    {
        /// <summary>
        /// Handles voice commands.
        /// </summary>
        private readonly IVoiceCommandManager _voice;

        /// <summary>
        /// Design controller.
        /// </summary>
        private HmdDesignController _design;
        
        /// <summary>
        /// Root element for dynamic content.
        /// </summary>
        private Element _dynamicRoot;
        
        /// <summary>
        /// Menu to adjust prop.
        /// </summary>
        private AdjustContentController _adjustContent;

        /// <summary>
        /// Menu to edit prop.
        /// </summary>
        private EditContentController _editContent;

        /// <summary>
        /// Menu to place objects.
        /// </summary>
        private MoveContentController _move;

        /// <summary>
        /// Content controller.
        /// </summary>
        private ContentDesignController _controller;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditContentDesignState(IVoiceCommandManager voice)
        {
            _voice = voice;
        }

        /// <inheritdoc />
        public void Initialize(
            HmdDesignController design,
            GameObject unityRoot,
            Element dynamicRoot,
            Element staticRoot)
        {
            _design = design;
            _dynamicRoot = dynamicRoot;

            // adjust content
            {
                _adjustContent = unityRoot.AddComponent<AdjustContentController>();
                _adjustContent.OnExit += PropAdjust_OnExit;
                _adjustContent.enabled = false;
                _adjustContent.SliderRotate.OnVisibilityChanged += AdjustControl_OnVisibilityChanged;
                _adjustContent.SliderX.OnVisibilityChanged += AdjustControl_OnVisibilityChanged;
                _adjustContent.SliderY.OnVisibilityChanged += AdjustControl_OnVisibilityChanged;
                _adjustContent.SliderZ.OnVisibilityChanged += AdjustControl_OnVisibilityChanged;
            }

            // edit content
            {
                _editContent = unityRoot.AddComponent<EditContentController>();
                _editContent.OnMove += Edit_OnMove;
                _editContent.OnReparent += Edit_OnReparent;
                _editContent.OnDelete += Edit_OnDelete;
                _editContent.enabled = false;
            }

            // place content
            {
                _move = unityRoot.AddComponent<MoveContentController>();
                _move.OnConfirm += Move_OnConfirmController;
                _move.OnCancel += Move_OnCancel;
                _move.enabled = false;
            }
        }

        /// <inheritdoc />
        public void Uninitialize()
        {
            Object.Destroy(_adjustContent);
            Object.Destroy(_editContent);
            Object.Destroy(_move);
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "Entering {0}.", GetType().Name);

            _controller = (ContentDesignController) context;

            // hide the splash on the controller
            _controller.HideSplashMenu();

            _dynamicRoot.Schema.Set("focus.visible", false);

            _adjustContent.Initialize(_controller);
            _adjustContent.enabled = true;

            _editContent.Initialize(_controller);
            _editContent.enabled = true;

            _voice.Register("back", Voice_OnBack);
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            CloseAll();

            _voice.Unregister("back");

            Log.Info(this, "Exiting {0}.", GetType().Name);
        }

        /// <summary>
        /// Closes all dialogs.
        /// </summary>
        private void CloseAll()
        {
            _adjustContent.enabled = false;
            _editContent.enabled = false;
        }
        
        /// <summary>
        /// Called when the place menu wants to confirm placement.
        /// </summary>
        /// <param name="controller">The prop controller.</param>
        private void Move_OnConfirmController(ContentDesignController controller)
        {
            _move.enabled = false;
            _dynamicRoot.Schema.Set("focus.visible", true);

            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the place menu wants to cancel placement.
        /// </summary>
        private void Move_OnCancel()
        {
            _move.enabled = false;
            _adjustContent.enabled = true;
            _editContent.enabled = true;
        }

        /// <summary>
        /// Called to move the content.
        /// </summary>
        /// <param name="elementController">The controller.</param>
        private void Edit_OnMove(ContentDesignController elementController)
        {
            CloseAll();
            
            elementController.EnableUpdates();
            _move.Initialize(elementController);
            _move.enabled = true;
        }

        /// <summary>
        /// Called to reparent.
        /// </summary>
        /// <param name="controller">The controller.</param>
        private void Edit_OnReparent(ContentDesignController controller)
        {
            CloseAll();

            _design.ChangeState<ReparentDesignState>(controller);
        }

        /// <summary>
        /// Called to delete the prop.
        /// </summary>
        /// <param name="elementController">The controller.</param>
        private void Edit_OnDelete(ContentDesignController elementController)
        {
            CloseAll();

            _design.Elements.Destroy(elementController.Element);

            _dynamicRoot.Schema.Set("focus.visible", true);

            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when the prop adjust wishes to exit.
        /// </summary>
        private void PropAdjust_OnExit(ContentDesignController controller)
        {
            // force final state update
            controller.FinalizeState();

            // close splash menu
            controller.ShowSplashMenu();

            // reenable dynamic
            _dynamicRoot.Schema.Set("focus.visible", true);

            // back to main
            _design.ChangeState<MainDesignState>();
        }

        /// <summary>
        /// Called when adjust control visibility is changed.
        /// </summary>
        /// <param name="interactable">Interactable.</param>
        private void AdjustControl_OnVisibilityChanged(IInteractable interactable)
        {
            var visible = interactable.Visible;

            _editContent.enabled = !visible;

            if (visible)
            {
                _controller.EnableUpdates();
            }
            else
            {
                _controller.DisableUpdates();
            }
        }

        /// <summary>
        /// Called when the voice manager receives a back command.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnBack(string command)
        {
            // back to main
            _design.ChangeState<MainDesignState>();
        }
    }
}