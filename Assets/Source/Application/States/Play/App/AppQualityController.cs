using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Basic implementation of <c>IAppQualityController</c> that listens to
    /// prop changes on the root element.
    /// </summary>
    public class AppQualityController : IAppQualityController
    {
        /// <summary>
        /// Prop names.
        /// </summary>
        public const string PROP_TEMPLATE_TEXTUREQUALITY = "{0}.quality.textureQuality";
        public const string PROP_TEMPLATE_ANISO = "{0}.quality.aniso";
        public const string PROP_TEMPLATE_AA = "{0}.quality.aa";
        public const string PROP_TEMPLATE_SOFTPARTICLES = "{0}.quality.softParticles";
        public const string PROP_TEMPLATE_REALTIMEREFLECTIONPROBES = "{0}.quality.realtimeReflectionProbes";
        public const string PROP_TEMPLATE_BILLBOARDS = "{0}.quality.billboards";
        public const string PROP_TEMPLATE_SHADOWQUALITY = "{0}.quality.shadowQuality";
        public const string PROP_TEMPLATE_SHADOWMASK = "{0}.quality.shadowMask";
        public const string PROP_TEMPLATE_SHADOWRESOLUTION = "{0}.quality.shadowResolution";
        public const string PROP_TEMPLATE_SHADOWPROJECTION = "{0}.quality.shadowProjection";
        public const string PROP_TEMPLATE_BLENDWEIGHTS = "{0}.quality.blendWeights";
        
        /// <summary>
        /// Props pulled from root element.
        /// </summary>
        private ElementSchemaProp<int> _textureLimitProp;
        private ElementSchemaProp<string> _anisoProp;
        private ElementSchemaProp<int> _aaProp;
        private ElementSchemaProp<bool> _softParticlesProp;
        private ElementSchemaProp<bool> _realtimeReflectionProbesProp;
        private ElementSchemaProp<bool> _billboardsProp;
        private ElementSchemaProp<string> _shadowQualityProp;
        private ElementSchemaProp<string> _shadowMaskProp;
        private ElementSchemaProp<string> _shadowResolutionProp;
        private ElementSchemaProp<string> _shadowProjectionProp;
        private ElementSchemaProp<string> _blendWeightsProp;

        /// <summary>
        /// Default values.
        /// </summary>
        private readonly int _defaultTestureLimit;
        private readonly AnisotropicFiltering _defaultAniso;
        private readonly BlendWeights _defaultBlendWeights;
        private readonly ShadowProjection _defaultShadowProjection;
        private readonly ShadowmaskMode _defaultShadowMask;
        private readonly ShadowQuality _defaultShadowQuality;
        private readonly bool _defaultBillboards;
        private readonly bool _defaultRealtimeReflectionProbes;
        private readonly bool _defaultSoftParticles;
        private readonly int _defaultAa;
        private readonly ShadowResolution _defaultShadowResolution;

        /// <summary>
        /// True iff Setup has been called but Teardown has not yet.
        /// </summary>
        private bool _isSetup;

        /// <summary>
        /// Constructor.
        /// </summary>
        public AppQualityController()
        {
            // save defaults
            _defaultTestureLimit = QualitySettings.masterTextureLimit;
            _defaultAniso = QualitySettings.anisotropicFiltering;
            _defaultAa = QualitySettings.antiAliasing;
            _defaultSoftParticles = QualitySettings.softParticles;
            _defaultRealtimeReflectionProbes = QualitySettings.realtimeReflectionProbes;
            _defaultBillboards = QualitySettings.billboardsFaceCameraPosition;
            _defaultShadowQuality = QualitySettings.shadows;
            _defaultShadowMask = QualitySettings.shadowmaskMode;
            _defaultShadowResolution = QualitySettings.shadowResolution;
            _defaultShadowProjection = QualitySettings.shadowProjection;
            _defaultBlendWeights = QualitySettings.blendWeights;
        }

        /// <inheritdoc />
        public void Setup(Element root)
        {
            var platform = UnityEngine.Application.platform.ToString();

            _textureLimitProp = root.Schema.GetOwn(string.Format(PROP_TEMPLATE_TEXTUREQUALITY, platform), _defaultTestureLimit);
            _textureLimitProp.OnChanged += TextureLimit_OnChanged;
            QualitySettings.masterTextureLimit = _textureLimitProp.Value;

            _anisoProp = root.Schema.GetOwn(string.Format(PROP_TEMPLATE_ANISO, platform), _defaultAniso.ToString());
            _anisoProp.OnChanged += Aniso_OnChanged;
            QualitySettings.anisotropicFiltering = _anisoProp.Value.ToEnum<AnisotropicFiltering>();

            _aaProp = root.Schema.GetOwn(string.Format(PROP_TEMPLATE_AA, platform), _defaultAa);
            _aaProp.OnChanged += Aa_OnChanged;
            QualitySettings.antiAliasing = _aaProp.Value;

            _softParticlesProp = root.Schema.GetOwn(string.Format(PROP_TEMPLATE_SOFTPARTICLES, platform), _defaultSoftParticles);
            _softParticlesProp.OnChanged += SoftParticles_OnChanged;
            QualitySettings.softParticles = _softParticlesProp.Value;

            _realtimeReflectionProbesProp = root.Schema.GetOwn(string.Format(PROP_TEMPLATE_REALTIMEREFLECTIONPROBES, platform), _defaultRealtimeReflectionProbes);
            _realtimeReflectionProbesProp.OnChanged += RealtimeReflectionProbes_OnChanged;
            QualitySettings.realtimeReflectionProbes = _realtimeReflectionProbesProp.Value;

            _billboardsProp = root.Schema.GetOwn(string.Format(PROP_TEMPLATE_BILLBOARDS, platform), _defaultBillboards);
            _billboardsProp.OnChanged += Billboards_OnChanged;
            QualitySettings.billboardsFaceCameraPosition = _billboardsProp.Value;

            _shadowQualityProp = root.Schema.GetOwn(string.Format(PROP_TEMPLATE_SHADOWQUALITY, platform), _defaultShadowQuality.ToString());
            _shadowQualityProp.OnChanged += Shadows_OnChanged;
            QualitySettings.shadows = _shadowQualityProp.Value.ToEnum<ShadowQuality>();

            _shadowMaskProp = root.Schema.GetOwn(string.Format(PROP_TEMPLATE_SHADOWMASK, platform), _defaultShadowMask.ToString());
            _shadowMaskProp.OnChanged += ShadowMask_OnChanged;
            QualitySettings.shadowmaskMode = _shadowMaskProp.Value.ToEnum<ShadowmaskMode>();

            _shadowResolutionProp = root.Schema.GetOwn(string.Format(PROP_TEMPLATE_SHADOWRESOLUTION, platform), _defaultShadowResolution.ToString());
            _shadowResolutionProp.OnChanged += ShadowResolution_OnChanged;
            QualitySettings.shadowResolution = _shadowResolutionProp.Value.ToEnum<ShadowResolution>();

            _shadowProjectionProp = root.Schema.GetOwn(string.Format(PROP_TEMPLATE_SHADOWPROJECTION, platform), _defaultShadowProjection.ToString());
            _shadowProjectionProp.OnChanged += ShadowProjection_OnChanged;
            QualitySettings.shadowProjection = _shadowProjectionProp.Value.ToEnum<ShadowProjection>();

            _blendWeightsProp = root.Schema.GetOwn(string.Format(PROP_TEMPLATE_BLENDWEIGHTS, platform), _defaultBlendWeights.ToString());
            _blendWeightsProp.OnChanged += BlendWeights_OnChanged;
            QualitySettings.blendWeights = _blendWeightsProp.Value.ToEnum<BlendWeights>();

            _isSetup = true;
        }

        /// <inheritdoc />
        public void Teardown()
        {
            // in the case that props have not yet been retrieved, return early
            if (!_isSetup)
            {
                return;
            }

            _textureLimitProp.OnChanged -= TextureLimit_OnChanged;
            _anisoProp.OnChanged -= Aniso_OnChanged;
            _aaProp.OnChanged -= Aa_OnChanged;
            _softParticlesProp.OnChanged -= SoftParticles_OnChanged;
            _realtimeReflectionProbesProp.OnChanged -= RealtimeReflectionProbes_OnChanged;
            _billboardsProp.OnChanged -= Billboards_OnChanged;
            _shadowQualityProp.OnChanged -= Shadows_OnChanged;
            _shadowMaskProp.OnChanged -= ShadowMask_OnChanged;
            _shadowResolutionProp.OnChanged -= ShadowResolution_OnChanged;
            _shadowProjectionProp.OnChanged -= ShadowProjection_OnChanged;
            _blendWeightsProp.OnChanged -= BlendWeights_OnChanged;
        }

        /// <summary>
        /// Called whe prop changes.
        /// </summary>
        /// <param name="prop">Prop that changed.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void BlendWeights_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            QualitySettings.blendWeights = _blendWeightsProp.Value.ToEnum<BlendWeights>();
        }

        /// <summary>
        /// Called whe prop changes.
        /// </summary>
        /// <param name="prop">Prop that changed.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void ShadowProjection_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            QualitySettings.shadowProjection = _shadowProjectionProp.Value.ToEnum<ShadowProjection>();
        }

        /// <summary>
        /// Called whe prop changes.
        /// </summary>
        /// <param name="prop">Prop that changed.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void ShadowResolution_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            QualitySettings.shadowResolution = _shadowResolutionProp.Value.ToEnum<ShadowResolution>();
        }

        /// <summary>
        /// Called whe prop changes.
        /// </summary>
        /// <param name="prop">Prop that changed.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void ShadowMask_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            QualitySettings.shadowmaskMode = _shadowMaskProp.Value.ToEnum<ShadowmaskMode>();
        }

        /// <summary>
        /// Called whe prop changes.
        /// </summary>
        /// <param name="prop">Prop that changed.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Shadows_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            QualitySettings.shadows = _shadowQualityProp.Value.ToEnum<ShadowQuality>();
        }

        /// <summary>
        /// Called whe prop changes.
        /// </summary>
        /// <param name="prop">Prop that changed.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Billboards_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            QualitySettings.billboardsFaceCameraPosition = _billboardsProp.Value;
        }

        /// <summary>
        /// Called whe prop changes.
        /// </summary>
        /// <param name="prop">Prop that changed.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void RealtimeReflectionProbes_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            QualitySettings.realtimeReflectionProbes = _realtimeReflectionProbesProp.Value;
        }

        /// <summary>
        /// Called whe prop changes.
        /// </summary>
        /// <param name="prop">Prop that changed.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void SoftParticles_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            QualitySettings.softParticles = _softParticlesProp.Value;
        }

        /// <summary>
        /// Called whe prop changes.
        /// </summary>
        /// <param name="prop">Prop that changed.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Aa_OnChanged(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            QualitySettings.antiAliasing = _aaProp.Value;
        }

        /// <summary>
        /// Called whe prop changes.
        /// </summary>
        /// <param name="prop">Prop that changed.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Aniso_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            QualitySettings.anisotropicFiltering = _anisoProp.Value.ToEnum<AnisotropicFiltering>();
        }

        /// <summary>
        /// Called whe prop changes.
        /// </summary>
        /// <param name="prop">Prop that changed.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void TextureLimit_OnChanged(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            QualitySettings.masterTextureLimit = _textureLimitProp.Value;
        }
    }
}