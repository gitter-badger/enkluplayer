using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Root application.
    /// </summary>
    public class Application
    {
        /// <summary>
        /// The host.
        /// </summary>
        private readonly IApplicationHost _host;

        /// <summary>
        /// For routing messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Controls application states.
        /// </summary>
        private readonly FiniteStateMachine _states;

        /// <summary>
        /// List of methods to unsubscribe.
        /// </summary>
        private readonly List<Action> _unsubscribeList = new List<Action>();

        /// <summary>
        /// Creates a new Application.
        /// </summary>
        public Application(
            IApplicationHost host,
            IMessageRouter messages,

            InitializeApplicationState initialize,
            EditApplicationState edit,
            PreviewApplicationState preview,
            PlayApplicationState play,
            HierarchyApplicationState hierarchy)
        {
            _host = host;
            _messages = messages;

            _states = new FiniteStateMachine(new IState[]
            {
                initialize,
                edit,
                preview,
                play,
                hierarchy
            });
        }

        /// <summary>
        /// Initializes the application.
        /// </summary>
        public void Initialize()
        {
            // application-wide messages
            Subscribe();

            // move to the default application state
            _states.Change<InitializeApplicationState>();
        }

        /// <summary>
        /// Uninitializes the application.
        /// </summary>
        public void Uninitialize()
        {
            Unsubscribe();

            _host.Stop();
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt">The time since last time Update was called.</param>
        public void Update(float dt)
        {
            _states.Update(dt);
        }
        
        /// <summary>
        /// Subscribes to application events.
        /// </summary>
        private void Subscribe()
        {
            _unsubscribeList.Add(_messages.SubscribeOnce(
                MessageTypes.READY,
                _ =>
                {
                    Log.Info(this, "Application ready.");

                    // now that the Application is ready, make ready the host
                    _host.Start();
                }));

            _unsubscribeList.Add(_messages.Subscribe(
                MessageTypes.PREVIEW,
                @event =>
                {
                    Log.Info(this, "Preview requested.");

                    _states.Change<PreviewApplicationState>((PreviewEvent) @event);
                }));

            _unsubscribeList.Add(_messages.Subscribe(
                MessageTypes.EDIT,
                _ =>
                {
                    Log.Info(this, "Edit requested.");

                    _states.Change<EditApplicationState>();
                }));

            _unsubscribeList.Add(_messages.Subscribe(
                MessageTypes.PLAY,
                _ =>
                {
                    Log.Info(this, "Play requested.");

                    _states.Change<PlayApplicationState>();
                }));

            _unsubscribeList.Add(_messages.Subscribe(
                MessageTypes.HIERARCHY,
                @event =>
                {
                    Log.Info(this, "Hierarchy requested.");

                    _states.Change<HierarchyApplicationState>((HierarchyEvent) @event);
                }));
        }

        private void Unsubscribe()
        {
            for (int i = 0, len = _unsubscribeList.Count; i < len; i++)
            {
                _unsubscribeList[i]();
            }
            _unsubscribeList.Clear();
        }
    }
}