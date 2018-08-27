using System;
using System.Linq;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages the main menu.
    /// </summary>
    public class MainMenuUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Enum for Experience submenu.
        /// </summary>
        public enum ExperienceSubMenu
        {
            New,
            Load,
            Duplicate
        }

        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..menu")]
        public MenuWidget Menu { get; set; }

        /// <summary>
        /// New element Sub Menu.
        /// </summary>
        [InjectElements("..submenu-new")]
        public SubMenuWidget NewSubMenu { get; set; }

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

        [InjectElements("..btn-clearanchors")]
        public ButtonWidget BtnClearAnchors { get; set; }

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
        /// Quality settings.
        /// </summary>
        [InjectElements("..slt-texturequality")]
        public SelectWidget SltTextureQuality { get; set; }

        [InjectElements("..slt-anisotropic")]
        public SelectWidget SltAnisotropic{ get; set; }

        [InjectElements("..slt-aa")]
        public SelectWidget SltAntiAliasing { get; set; }

        [InjectElements("..tgl-softparticles")]
        public ToggleWidget TglSoftParticles { get; set; }

        [InjectElements("..tgl-realtimereflectionprobes")]
        public ToggleWidget TglRealtimeReflectionProbes { get; set; }

        [InjectElements("..tgl-billboards")]
        public ToggleWidget TglBillboards { get; set; }

        [InjectElements("..slt-shadows")]
        public SelectWidget SltShadows { get; set; }

        [InjectElements("..slt-shadowmask")]
        public SelectWidget SltShadowmask { get; set; }

        [InjectElements("..slt-shadowresolution")]
        public SelectWidget SltShadowResolution { get; set; }

        [InjectElements("..slt-shadowprojection")]
        public SelectWidget SltShadowProjection { get; set; }

        [InjectElements("..slt-blendweights")]
        public SelectWidget SltBlendWeights { get; set; }

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
        public event Action<ExperienceSubMenu> OnExperience;

        /// <summary>
        /// Called when user requests to reset all data.
        /// </summary>
        public event Action OnResetData;

        /// <summary>
        /// Called when the user wishes to clear all local anchors.
        /// </summary>
        public event Action OnClearAnchors;

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

            BtnClearAnchors.Activator.OnActivated += _ =>
            {
                if (null != OnClearAnchors)
                {
                    OnClearAnchors();
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

            BtnExpNew.Activator.OnActivated += _ => Experience(ExperienceSubMenu.New);
            BtnExpLoad.Activator.OnActivated += _ => Experience(ExperienceSubMenu.Load);
            BtnExpDuplicate.Activator.OnActivated += _ => Experience(ExperienceSubMenu.Duplicate);

            // options
            {
                SltTextureQuality.Selection = SltTextureQuality.Options.FirstOrDefault(op => int.Parse(op.Value) == QualitySettings.masterTextureLimit);
                SltTextureQuality.OnValueChanged += _ => QualitySettings.masterTextureLimit = int.Parse(SltTextureQuality.Selection.Value);

                SltAnisotropic.Selection = SltAnisotropic.Options.FirstOrDefault(op => To<AnisotropicFiltering>(op.Value) == QualitySettings.anisotropicFiltering);
                SltAnisotropic.OnValueChanged += _ => QualitySettings.anisotropicFiltering = To<AnisotropicFiltering>(SltAnisotropic.Selection.Value);

                SltAntiAliasing.Selection = SltAntiAliasing.Options.FirstOrDefault(op => int.Parse(op.Value) == QualitySettings.antiAliasing);
                SltAntiAliasing.OnValueChanged += _ => QualitySettings.antiAliasing = int.Parse(SltAntiAliasing.Selection.Value);

                TglSoftParticles.Value = QualitySettings.softParticles;
                TglSoftParticles.OnValueChanged += _ => QualitySettings.softParticles = TglSoftParticles.Value;

                TglRealtimeReflectionProbes.Value = QualitySettings.realtimeReflectionProbes;
                TglRealtimeReflectionProbes.OnValueChanged += _ => QualitySettings.realtimeReflectionProbes = TglRealtimeReflectionProbes.Value;

                TglBillboards.Value = QualitySettings.billboardsFaceCameraPosition;
                TglBillboards.OnValueChanged += _ => QualitySettings.billboardsFaceCameraPosition = TglBillboards.Value;

                SltShadows.Selection = SltShadows.Options.FirstOrDefault(op => To<ShadowQuality>(op.Value) == QualitySettings.shadows);
                SltShadows.OnValueChanged += _ => QualitySettings.shadows = To<ShadowQuality>(SltShadows.Selection.Value);

                SltShadowmask.Selection = SltShadowmask.Options.FirstOrDefault(op => To<ShadowmaskMode>(op.Value) == QualitySettings.shadowmaskMode);
                SltShadowmask.OnValueChanged += _ => QualitySettings.shadowmaskMode = To<ShadowmaskMode>(SltShadows.Selection.Value);

                SltShadowResolution.Selection = SltShadowResolution.Options.FirstOrDefault(op => To<ShadowResolution>(op.Value) == QualitySettings.shadowResolution);
                SltShadowResolution.OnValueChanged += _ => QualitySettings.shadowResolution = To<ShadowResolution>(SltShadowResolution.Selection.Value);

                SltShadowProjection.Selection = SltShadowProjection.Options.FirstOrDefault(op => To<ShadowProjection>(op.Value) == QualitySettings.shadowProjection);
                SltShadowProjection.OnValueChanged += _ => QualitySettings.shadowProjection = To<ShadowProjection>(SltShadowProjection.Selection.Value);

                SltBlendWeights.Selection = SltBlendWeights.Options.FirstOrDefault(op => To<BlendWeights>(op.Value) == QualitySettings.blendWeights);
                SltBlendWeights.OnValueChanged += _ => QualitySettings.blendWeights = To<BlendWeights>(SltBlendWeights.Selection.Value);
            }
        }

        private T To<T>(string value)
        {
            try
            {
                return (T) Enum.Parse(typeof(T), value);
            }
            catch
            {
                return default(T);
            }
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
        /// <param name="type">The element type.</param>
        private void Experience(ExperienceSubMenu type)
        {
            if (null != OnExperience)
            {
                OnExperience(type);
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