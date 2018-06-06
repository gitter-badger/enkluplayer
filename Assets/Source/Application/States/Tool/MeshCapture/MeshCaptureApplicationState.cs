using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application state for capturing the world mesh.
    /// </summary>
    public class MeshCaptureApplicationState : IState, IMeshCaptureObserver
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IUIManager _ui;
        private readonly IVoiceCommandManager _voice;
        private readonly IMessageRouter _messages;
        private readonly IMeshCaptureService _capture;
        private readonly MeshCaptureExportService _exportService;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Camera settings snapshot.
        /// </summary>
        private CameraSettingsSnapshot _snapshot;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public MeshCaptureApplicationState(
            IUIManager ui,
            IVoiceCommandManager voice,
            IMessageRouter messages,
            IMeshCaptureService capture,
            MeshCaptureExportService exportService)
        {
            _ui = ui;
            _voice = voice;
            _messages = messages;
            _capture = capture;
            _exportService = exportService;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _frame = _ui.CreateFrame();

            // setup camera
            var camera = Camera.main;
            
            // save snapshot first
            _snapshot = CameraSettingsSnapshot.Snapshot(camera);

            // then set camera settings
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.85f;
            camera.farClipPlane = 1000f;
            camera.transform.position = Vector3.zero;

            // setup voice commands
            if (!_voice.Register(VoiceKeywords.EXIT, Voice_OnExit))
            {
                Log.Error(this, "Could not register exit voice command.");
            }

            // start capture
            _capture.Start(this);

            // start export pipeline
            _exportService.Start();

            _ui.Open<MeshCaptureSplashUIView>(new UIReference
                {
                    UIDataId = "MeshCapture.Splash"
                })
                .OnSuccess(el =>
                {
                    el.OnBack += Finalize;
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not open MeshCapture.Splash UI : {0}", exception);

                    _messages.Publish(MessageTypes.USER_PROFILE);
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

            _exportService.Stop();

            // kill voice commands
            if (!_voice.Unregister(VoiceKeywords.EXIT))
            {
                Log.Error(this, "Could not unregister exit voice command.");
            }
            
            // restore camera snapshot
            CameraSettingsSnapshot.Apply(Camera.main, _snapshot);
        }

        /// <inheritdoc />
        public void OnData(int id, MeshFilter filter)
        {
            //
        }

        /// <summary>
        /// Finalizes and returns to User Profile.
        /// </summary>
        private void Finalize()
        {
            // TODO: finalize

            _messages.Publish(MessageTypes.USER_PROFILE);
        }

        /// <summary>
        /// Called when the voice processor received a save command.
        /// </summary>
        /// <param name="save">The command received.</param>
        private void Voice_OnExit(string save)
        {
            _messages.Publish(MessageTypes.TOOLS);
        }
    }
}