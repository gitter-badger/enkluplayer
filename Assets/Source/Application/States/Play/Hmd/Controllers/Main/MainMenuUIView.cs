using System;
using System.Linq;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages the main menu.
    /// </summary>
    public class MainMenuUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..menu")]
        public MenuWidget Menu { get; set; }
        
        [InjectElements("..btn-new-asset")]
        public ButtonWidget BtnNewAsset { get; set; }

        [InjectElements("..btn-new-anchor")]
        public ButtonWidget BtnNewAnchor { get; set; }

        [InjectElements("..btn-new-text")]
        public ButtonWidget BtnNewText { get; set; }

        [InjectElements("..btn-new-container")]
        public ButtonWidget BtnNewContainer { get; set; }

        [InjectElements("..btn-new-light")]
        public ButtonWidget BtnNewLight{ get; set; }

        [InjectElements("..btn-resetdata")]
        public ButtonWidget BtnResetData{ get; set; }

        [InjectElements("..slt-play")]
        public SelectWidget SltPlay { get; set; }

        [InjectElements("..slt-logging")]
        public SelectWidget SltLogging { get; set; }

        [InjectElements("..btn-exp-new")]
        public ButtonWidget BtnExpNew { get; set; }

        [InjectElements("..btn-exp-load")]
        public ButtonWidget BtnExpLoad { get; set; }

        [InjectElements("..btn-exp-duplicate")]
        public ButtonWidget BtnExpDuplicate { get; set; }

        /// <summary>
        /// Called when we wish to go back.
        /// </summary>
        public event Action OnBack;
        
        /// <summary>
        /// Called when the new button is pressed.
        /// </summary>
        public event Action<int> OnNew;

        /// <summary>
        /// Called when the new button is pressed.
        /// </summary>
        public event Action<int> OnExperience;

        /// <summary>
        /// Called when user requests to reset all data.
        /// </summary>
        public event Action OnResetData;

        /// <summary>
        /// Called when the user changes the default play mode.
        /// </summary>
        public event Action<bool> OnDefaultPlayModeChanged;

        /// <summary>
        /// Called when _visible_ log level has been changed.
        /// </summary>
        public event Action<LogLevel> OnLogLevelChanged;

        /// <summary>
        /// Initializes the view with values
        /// </summary>
        /// <param name="play">True iff playmode.</param>
        public void Initialize(bool play)
        {
            SltPlay.Selection = SltPlay.Options.FirstOrDefault(option => play
                ? option.Value == "Play"
                : option.Value == "Edit");
        }

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            Menu.OnBack += _ =>
            {
                if (OnBack != null)
                {
                    OnBack();
                }
            };

            BtnNewAsset.Activator.OnActivated += _ => New(ElementTypes.CONTENT);
            BtnNewAnchor.Activator.OnActivated += _ => New(ElementTypes.WORLD_ANCHOR);
            BtnNewText.Activator.OnActivated += _ => New(ElementTypes.CAPTION);
            BtnNewContainer.Activator.OnActivated += _ => New(ElementTypes.CONTAINER);
            BtnNewLight.Activator.OnActivated += _ => New(ElementTypes.LIGHT);

            BtnResetData.Activator.OnActivated += _ =>
            {
                if (null != OnResetData)
                {
                    OnResetData();
                }
            };

            SltPlay.OnValueChanged += SelectPlay_OnChanged;
            SltLogging.OnValueChanged += _ =>
            {
                if (null != OnLogLevelChanged)
                {
                    OnLogLevelChanged(EnumExtensions.Parse(
                        SltLogging.Selection.Value,
                        LogLevel.Info));
                }
            };

            //TODO - assign corresponding action type 
            BtnExpNew.Activator.OnActivated += _ => Experience(1);
            BtnExpLoad.Activator.OnActivated += _ => Experience(2);
            BtnExpDuplicate.Activator.OnActivated += _ => Experience(3);
        }
        
        /// <summary>
        /// Helper method to call new callback.
        /// </summary>
        /// <param name="elementType">The element type.</param>
        private void New(int elementType)
        {
            if (null != OnNew)
            {
                OnNew(elementType);
            }
        }

        /// <summary>
        /// Helper method to call experience callback.
        /// </summary>
        /// <param name="elementType">The element type.</param>
        private void Experience(int elementType)
        {
            if (null != OnExperience)
            {
                OnExperience(elementType);
            }
        }

        /// <summary>
        /// Called when selection has changed.
        /// </summary>
        /// <param name="select">The select widget.</param>
        private void SelectPlay_OnChanged(SelectWidget @select)
        {
            if (null != OnDefaultPlayModeChanged)
            {
                OnDefaultPlayModeChanged(@select.Selection.Value == "Play");
            }
        }
    }
}