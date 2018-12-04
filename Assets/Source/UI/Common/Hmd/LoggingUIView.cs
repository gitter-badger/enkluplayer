﻿using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// UI view with live logs.
    /// </summary>
    public class LoggingUIView : MonoBehaviourIUXController, ILogTarget
    {
        /// <summary>
        /// Keeps history.
        /// </summary>
        private readonly HistoryLogTarget _history = new HistoryLogTarget(new DefaultLogFormatter
        {
            Level = true,
            ObjectToString = false,
            Timestamp = true,
            TypeName = true
        })
        {
            Filter = LogLevel.Debug
        };

        /// <summary>
        /// Token for loading user preferences.
        /// </summary>
        private IAsyncToken<SynchronizedObject<UserPreferenceData>> _loadToken;
        
        /// <summary>
        /// Injected elements.
        /// </summary>
        [InjectElements("..btn-close")]
        public ButtonWidget BtnClose { get; set; }
        [InjectElements("..slt-level")]
        public SelectWidget SltLevel { get; set; }
        [InjectElements("..txt-box")]
        public TextWidget TxtBox { get; set; }

        /// <summary>
        /// Injected dependencies.
        /// </summary>
        [Inject]
        public UserPreferenceService Prefs { get; set; }

        /// <summary>
        /// Called when the logging view should be closed.
        /// </summary>
        public event Action OnClose;

        /// <inheritdoc />
        public LogLevel Filter
        {
            get { return _history.Filter; }
            set { _history.Filter = value; }
        }

        /// <inheritdoc />
        private void Start()
        {
            Log.AddLogTarget(this);
        }

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            TxtBox.Label = "Loading...";

            BtnClose.OnActivated += _ =>
            {
                if (null != OnClose)
                {
                    OnClose();
                }
            };

            // setup logging
            _loadToken = Prefs.ForCurrentUser();
            _loadToken
                .OnSuccess(prefs =>
                {
                    Filter = prefs.Data.LogLevel;

                    // add listener here so we know we have what we need
                    SltLevel.OnValueChanged += widget => prefs.Queue((prev, next) =>
                    {
                        prev.LogLevel = EnumExtensions.Parse<LogLevel>(SltLevel.Selection.Value);

                        next(prev);
                    });

                    TxtBox.Label = _history.GenerateDump();
                })
                .OnFailure(ex => Log.Error(this, "Could not load user prefs : {0}", ex))
                .OnFinally(_ => Log.AddLogTarget(this));
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (null != _loadToken)
            {
                _loadToken.Abort();
                _loadToken = null;
            }

            Log.RemoveLogTarget(this);
        }

        /// <inheritdoc />
        public void OnLog(LogLevel level, object caller, string message)
        {
            // pass everything to history
            _history.OnLog(level, caller, message);

            // forward to text box
            if (null != TxtBox)
            {
                TxtBox.Label = _history.GenerateDump();
            }
        }
    }
}