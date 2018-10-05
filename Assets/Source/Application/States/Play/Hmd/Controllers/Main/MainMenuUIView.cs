using System;
using System.Linq;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
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

        [InjectElements("..btn-deviceregistration")]
        public ButtonWidget BtnRegistration { get; set; }

        [InjectElements("..slt-logging")]
        public SelectWidget SltLogging { get; set; }

        [InjectElements("..btn-exp-new")]
        public ButtonWidget BtnExpNew { get; set; }

        [InjectElements("..btn-exp-load")]
        public ButtonWidget BtnExpLoad { get; set; }

        [InjectElements("..btn-exp-duplicate")]
        public ButtonWidget BtnExpDuplicate { get; set; }

        [InjectElements("..txt-version")]
        public CaptionWidget TxtVersion { get; set; }

        [InjectElements("..txt-deviceName")]
        public CaptionWidget TxtDeviceName { get; set; }

        [InjectElements("..tgl-metrics")]
        public ToggleWidget TglMetrics { get; set; }

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

        [InjectElements("..btn-logout")]
        public ButtonWidget BtnLogout { get; set; }

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
        /// Called when the user wants to sync device registrations.
        /// </summary>
        public event Action OnDeviceRegistration;

        /// <summary>
        /// Called when _visible_ log level has been changed.
        /// </summary>
        public event Action<LogLevel> OnLogLevelChanged;

        /// <summary>
        /// Called when signout is requested.
        /// </summary>
        public event Action OnSignout;

        /// <summary>
        /// Toggles metrics hud.
        /// </summary>
        public event Action<bool> OnMetricsHud;

        /// <summary>
        /// Id of the scene.
        /// </summary>
        private string _sceneId;

        /// <summary>
        /// Root element of the scene.
        /// </summary>
        private Element _root;

        /// <summary>
        /// Manages element transactions.
        /// </summary>
        private IElementTxnManager _txns;
        
        /// <summary>
        /// Initializes the view.
        /// </summary>
        public void Initialize(
            string sceneId,
            Element root,
            IElementTxnManager txns,
            ApplicationConfig config,
            bool play)
        {
            _sceneId = sceneId;
            _root = root;
            _txns = txns;

            TxtVersion.Label = string.Format("v.{0}", config.Version);
            TxtDeviceName.Label = string.Format("Device: {0}", SystemInfo.deviceUniqueIdentifier);

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
            BtnRegistration.OnActivated += _ =>
            {
                if (null != OnDeviceRegistration)
                {
                    OnDeviceRegistration();
                }
            };
            SltLogging.OnValueChanged += _ =>
            {
                if (null != OnLogLevelChanged)
                {
                    OnLogLevelChanged(EnumExtensions.Parse(
                        SltLogging.Selection.Value,
                        LogLevel.Info));
                }
            };

            BtnLogout.OnActivated += _ =>
            {
                if (null != OnSignout)
                {
                    OnSignout();
                }
            };

            BtnExpNew.Activator.OnActivated += _ => Experience(ExperienceSubMenu.New);
            BtnExpLoad.Activator.OnActivated += _ => Experience(ExperienceSubMenu.Load);
            BtnExpDuplicate.Activator.OnActivated += _ => Experience(ExperienceSubMenu.Duplicate);

            TglMetrics.OnValueChanged += _ =>
            {
                if (null != OnMetricsHud)
                {
                    OnMetricsHud(TglMetrics.Value);
                }
            };

            // options
            {
                var platform = UnityEngine.Application.platform.ToString();

                SltTextureQuality.Selection = SltTextureQuality.Options.FirstOrDefault(op => int.Parse(op.Value) == QualitySettings.masterTextureLimit);
                SltTextureQuality.OnValueChanged += _ =>
                {
                    var value = int.Parse(SltTextureQuality.Selection.Value);
                    _txns
                        .Request(new ElementTxn(_sceneId).Update(
                            _root.Id,
                            string.Format(AppQualityController.PROP_TEMPLATE_TEXTUREQUALITY, platform),
                            value))
                        .OnFailure(ex => Log.Warning(this, "Could not change quality settings : {0}", ex));
                };

                SltAnisotropic.Selection = SltAnisotropic.Options.FirstOrDefault(op => To<AnisotropicFiltering>(op.Value) == QualitySettings.anisotropicFiltering);
                SltAnisotropic.OnValueChanged += _ =>
                {
                    var value = To<AnisotropicFiltering>(SltAnisotropic.Selection.Value).ToString();
                    _txns
                        .Request(new ElementTxn(_sceneId).Update(
                            _root.Id,
                            string.Format(AppQualityController.PROP_TEMPLATE_ANISO, platform),
                            value))
                        .OnFailure(ex => Log.Warning(this, "Could not change quality settings : {0}", ex));
                };

                SltAntiAliasing.Selection = SltAntiAliasing.Options.FirstOrDefault(op => int.Parse(op.Value) == QualitySettings.antiAliasing);
                SltAntiAliasing.OnValueChanged += _ =>
                {
                    var value = int.Parse(SltAntiAliasing.Selection.Value);
                    _txns
                        .Request(new ElementTxn(_sceneId).Update(
                            _root.Id,
                            string.Format(AppQualityController.PROP_TEMPLATE_AA, platform),
                            value))
                        .OnFailure(ex => Log.Warning(this, "Could not change quality settings : {0}", ex));
                    
                };

                TglSoftParticles.Value = QualitySettings.softParticles;
                TglSoftParticles.OnValueChanged += _ =>
                {
                    var value = TglSoftParticles.Value;
                    _txns
                        .Request(new ElementTxn(_sceneId).Update(
                            _root.Id,
                            string.Format(AppQualityController.PROP_TEMPLATE_SOFTPARTICLES, platform),
                            value))
                        .OnFailure(ex => Log.Warning(this, "Could not change quality settings : {0}", ex));
                };

                TglRealtimeReflectionProbes.Value = QualitySettings.realtimeReflectionProbes;
                TglRealtimeReflectionProbes.OnValueChanged += _ =>
                {
                    var value = TglRealtimeReflectionProbes.Value;
                    _txns
                        .Request(new ElementTxn(_sceneId).Update(
                            _root.Id,
                            string.Format(AppQualityController.PROP_TEMPLATE_REALTIMEREFLECTIONPROBES, platform),
                            value))
                        .OnFailure(ex => Log.Warning(this, "Could not change quality settings : {0}", ex));
                };

                TglBillboards.Value = QualitySettings.billboardsFaceCameraPosition;
                TglBillboards.OnValueChanged += _ =>
                {
                    var value = TglBillboards.Value;
                    _txns
                        .Request(new ElementTxn(_sceneId).Update(
                            _root.Id,
                            string.Format(AppQualityController.PROP_TEMPLATE_BILLBOARDS, platform),
                            value))
                        .OnFailure(ex => Log.Warning(this, "Could not change quality settings : {0}", ex));
                };

                SltShadows.Selection = SltShadows.Options.FirstOrDefault(op => To<ShadowQuality>(op.Value) == QualitySettings.shadows);
                SltShadows.OnValueChanged += _ =>
                {
                    var value = To<ShadowQuality>(SltShadows.Selection.Value).ToString();
                    _txns
                        .Request(new ElementTxn(_sceneId).Update(
                            _root.Id,
                            string.Format(AppQualityController.PROP_TEMPLATE_SHADOWQUALITY, platform),
                            value))
                        .OnFailure(ex => Log.Warning(this, "Could not change quality settings : {0}", ex));
                };

                SltShadowmask.Selection = SltShadowmask.Options.FirstOrDefault(op => To<ShadowmaskMode>(op.Value) == QualitySettings.shadowmaskMode);
                SltShadowmask.OnValueChanged += _ =>
                {
                    var value = To<ShadowmaskMode>(SltShadowmask.Selection.Value).ToString();
                    _txns
                        .Request(new ElementTxn(_sceneId).Update(
                            _root.Id,
                            string.Format(AppQualityController.PROP_TEMPLATE_SHADOWMASK, platform),
                            value))
                        .OnFailure(ex => Log.Warning(this, "Could not change quality settings : {0}", ex));
                };

                SltShadowResolution.Selection = SltShadowResolution.Options.FirstOrDefault(op => To<ShadowResolution>(op.Value) == QualitySettings.shadowResolution);
                SltShadowResolution.OnValueChanged += _ =>
                {
                    var value = To<ShadowResolution>(SltShadowResolution.Selection.Value).ToString();
                    _txns
                        .Request(new ElementTxn(_sceneId).Update(
                            _root.Id,
                            string.Format(AppQualityController.PROP_TEMPLATE_SHADOWRESOLUTION, platform),
                            value))
                        .OnFailure(ex => Log.Warning(this, "Could not change quality settings : {0}", ex));
                };

                SltShadowProjection.Selection = SltShadowProjection.Options.FirstOrDefault(op => To<ShadowProjection>(op.Value) == QualitySettings.shadowProjection);
                SltShadowProjection.OnValueChanged += _ =>
                {
                    var value = To<ShadowProjection>(SltShadowProjection.Selection.Value).ToString();
                    _txns
                        .Request(new ElementTxn(_sceneId).Update(
                            _root.Id,
                            string.Format(AppQualityController.PROP_TEMPLATE_SHADOWPROJECTION, platform),
                            value))
                        .OnFailure(ex => Log.Warning(this, "Could not change quality settings : {0}", ex));
                };

                SltBlendWeights.Selection = SltBlendWeights.Options.FirstOrDefault(op => To<BlendWeights>(op.Value) == QualitySettings.blendWeights);
                SltBlendWeights.OnValueChanged += _ =>
                {
                    var value = To<BlendWeights>(SltBlendWeights.Selection.Value).ToString();
                    _txns
                        .Request(new ElementTxn(_sceneId).Update(
                            _root.Id,
                            string.Format(AppQualityController.PROP_TEMPLATE_BLENDWEIGHTS, platform),
                            value))
                        .OnFailure(ex => Log.Warning(this, "Could not change quality settings : {0}", ex));
                };
            }
        }

        /// <summary>
        /// Converts a string to an enum.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The string value.</param>
        /// <returns></returns>
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