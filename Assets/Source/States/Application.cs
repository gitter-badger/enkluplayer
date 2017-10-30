﻿using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
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
        /// Http service.
        /// </summary>
        private readonly IHttpService _http;

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
            IHttpService http,

            InitializeApplicationState initialize,
            WaitingForConnectionApplicationState wait,
            EditApplicationState edit,
            PreviewApplicationState preview,
            PlayApplicationState play,
            HierarchyApplicationState hierarchy)
        {
            _host = host;
            _messages = messages;
            _http = http;

            _states = new FiniteStateMachine(new IState[]
            {
                initialize,
                wait,
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
            // TODO: move into Initialize state?
            _unsubscribeList.Add(_messages.Subscribe(
                MessageTypes.AUTHORIZED,
                @event =>
                {
                    var message = (AuthorizedEvent)@event;

                    Log.Info(this, "Application authorized.");

                    // DEMO
                    _http.UrlBuilder.BaseUrl = "192.168.1.2";

                    // setup http service
                    _http.UrlBuilder.Replacements.Add(Commons.Unity.DataStructures.Tuple.Create(
                        "userId",
                        message.profile.id));
                    _http.Headers.Add(Commons.Unity.DataStructures.Tuple.Create(
                        "Authorization",
                        string.Format("Bearer {0}", message.credentials.token)));
                }));

            _unsubscribeList.Add(_messages.Subscribe(
                MessageTypes.DISCONNECTED,
                _ =>
                {
                    Log.Info(this, "Disconnected.");

                    _states.Change<WaitingForConnectionApplicationState>();
                }));

            _unsubscribeList.Add(_messages.SubscribeOnce(
                MessageTypes.READY,
                _ =>
                {
                    Log.Info(this, "Application ready.");

                    // now that the Application is ready, make ready the host
                    _host.Start();

                    _states.Change<WaitingForConnectionApplicationState>();
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
                MessageTypes.PLAY,
                _ =>
                {
                    Log.Info(this, "Play requested.");

                    _states.Change<PlayApplicationState>();
                }));

            _unsubscribeList.Add(_messages.Subscribe(
                MessageTypes.HIERARCHY,
                _ =>
                {
                    Log.Info(this, "Hierarchy requested.");

                    _states.Change<HierarchyApplicationState>();
                }));
        }

        /// <summary>
        /// Unsubscribes from all subscriptions.
        /// </summary>
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