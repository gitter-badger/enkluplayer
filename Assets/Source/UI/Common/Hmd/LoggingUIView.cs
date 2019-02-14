using System;
using System.Collections.ObjectModel;
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
        public LogLevel Filter { get; set; }

        /// <inheritdoc />
        public void OnLog(LogLevel level, object caller, string message)
        {
            if (level < Filter)
            {
                return;
            }

            UpdateTextField();
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
                    SltLevel.Selection = FindLevelOption(SltLevel.Options, Filter);

                    // add listener here so we know we have what we need
                    SltLevel.OnValueChanged += widget => prefs.Queue((prev, next) =>
                    {
                        Filter = prev.LogLevel = EnumExtensions.Parse<LogLevel>(SltLevel.Selection.Value);
                        UpdateTextField();

                        next(prev);
                    });

                    UpdateTextField();
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

        /// <summary>
        /// Updates text field from logs.
        /// </summary>
        private void UpdateTextField()
        {
            // forward to text box
            if (null != TxtBox)
            {
                TxtBox.Label = Log.History.GenerateDump(
                    Filter,
                    LogDumpOptions.Reverse);
            }
        }

        /// <summary>
        /// Finds the appropriate option for the input log level.
        /// </summary>
        /// <param name="options">Options.</param>
        /// <param name="level">The log level to look for.</param>
        /// <returns></returns>
        private static Option FindLevelOption(ReadOnlyCollection<Option> options, LogLevel level)
        {
            for (int i = 0, len = options.Count; i < len; i++)
            {
                var option = options[i];
                if (option.Value == level.ToString())
                {
                    return option;
                }
            }

            return options[0];
        }
    }
}