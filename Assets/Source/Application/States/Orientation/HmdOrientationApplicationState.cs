using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State that orients a user to the HoloLens.
    /// </summary>
    public class OrientationApplicationState : IState
    {
        /// <summary>
        /// Manages UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Sends/receives messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Root object.
        /// </summary>
        private GameObject _root;

        /// <summary>
        /// Time at which state was entered.
        /// </summary>
        private DateTime _enterTime;

        /// <summary>
        /// Constructor.
        /// </summary>
        public OrientationApplicationState(IMessageRouter messages, IUIManager ui)
        {
            _messages = messages;
            _ui = ui;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _frame = _ui.CreateFrame();
        
            // open orientation UI view
            int id;
            _ui
                .Open<HmdOrientationUIView>(new UIReference
                {
                    UIDataId = "Orientation.Adjust"
                }, out id)
                .OnSuccess(el =>
                {
                    el.OnContinue += Orientation_OnContinue;
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            //in the editor, continue automatically.
            if (UnityEngine.Application.isEditor)
            {
                if (DateTime.Now.Subtract(_enterTime).TotalSeconds > 1f)
                {
                    Orientation_OnContinue();
                }
            }
        }

        /// <inheritdoc />
        public void Exit()
        {
            Log.Info(this, "Exit {0}.", GetType().Name);

            Object.Destroy(_root);
            _frame.Release();
        }

        /// <summary>
        /// Called when the orientation view receives a complete.
        /// </summary>
        private void Orientation_OnContinue()
        {
            _ui.Pop();
            _messages.Publish(MessageTypes.LOGIN);
        }
    }
}