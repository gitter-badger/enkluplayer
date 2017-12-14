using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Launch point for tools.
    /// </summary>
    public class ToolModeApplicationState : IState
    {
        /// <summary>
        /// Processes voice commands.
        /// </summary>
        private readonly IVoiceCommandManager _voice;

        /// <summary>
        /// Routes messages about.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ToolModeApplicationState(
            IVoiceCommandManager voice,
            IMessageRouter messages)
        {
            _voice = voice;
            _messages = messages;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            if (!_voice.Register(VoiceKeywords.MESH_TOOL, Voice_OnWorldMeshCapture))
            {
                Log.Error(this, "Could not register mesh capture keyword with voice manager");
            }

            if (!_voice.Register(VoiceKeywords.HELP, Voice_OnHelp))
            {
                Log.Error(this, "Could not register help keyword with voice manager.");
            }

            _messages.Publish(MessageTypes.STATUS, "Waiting for voice commands... Say 'help' for list of commands.");
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            if (!_voice.Unregister(VoiceKeywords.MESH_TOOL, VoiceKeywords.HELP))
            {
                Log.Error(this,
                    "Could not unregister keywords with voice manager.");
            }
        }

        /// <summary>
        /// Called when the WorldMeshCapture keyword has been recognized.
        /// </summary>
        /// <param name="command">The keyword.</param>
        private void Voice_OnWorldMeshCapture(string command)
        {
            Log.Info(this, "World Mesh Capture voice command recognized.");

            _messages.Publish(MessageTypes.STATUS, "Initializing mesh capture state.");

            _messages.Publish(MessageTypes.MESHCAPTURE);
        }

        /// <summary>
        /// Called when the help keyword is recognized.
        /// </summary>
        /// <param name="command">The keyword.</param>
        private void Voice_OnHelp(string command)
        {
            _messages.Publish(MessageTypes.STATUS, "Available Commands:\nCapture\n");
        }
    }
}