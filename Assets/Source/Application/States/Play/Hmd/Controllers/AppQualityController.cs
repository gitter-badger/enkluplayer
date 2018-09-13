using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an object that applies and updates quality settings.
    /// </summary>
    public interface IAppQualityController
    {
        /// <summary>
        /// Sets up the player according to a specific element root
        /// configuration. Watches the root for updates.
        /// </summary>
        /// <param name="root">The element root.</param>
        void Setup(Element root);

        /// <summary>
        /// Stops watching for updates.
        /// </summary>
        void Teardown();
    }

    public class AppQualityController : IAppQualityController
    {
        private readonly ApplicationConfig _config;

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

        private int _defaultTestureLimit;
        private AnisotropicFiltering _defaultAniso;
        private BlendWeights _defaultBlendWeights;
        private ShadowProjection _defaultShadowProjection;
        private ShadowmaskMode _defaultShadowMask;
        private ShadowQuality _defaultShadowQuality;
        private bool _defaultBillboards;
        private bool _defaultRealtimeReflectionProbes;
        private bool _defaultSoftParticles;
        private int _defaultAa;
        private ShadowResolution _defaultShadowResolution;

        public AppQualityController(ApplicationConfig config)
        {
            _config = config;

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

        public void Setup(Element root)
        {
            var platform = _config.ParsedPlatform.ToString();

            _textureLimitProp = root.Schema.GetOwn(string.Format("{0}.quality.textureQuality", platform), _defaultTestureLimit);
            _textureLimitProp.OnChanged += TextureLimit_OnChanged;
            QualitySettings.masterTextureLimit = _textureLimitProp.Value;

            _anisoProp = root.Schema.GetOwn(string.Format("{0}.quality.aniso", platform), _defaultAniso.ToString());
            _anisoProp.OnChanged += Aniso_OnChanged;
            QualitySettings.anisotropicFiltering = _anisoProp.Value.ToEnum<AnisotropicFiltering>();

            _aaProp = root.Schema.GetOwn(string.Format("{0}.quality.aa", platform), _defaultAa);
            _aaProp.OnChanged += Aa_OnChanged;
            QualitySettings.antiAliasing = _aaProp.Value;

            _softParticlesProp = root.Schema.GetOwn(string.Format("{0}.quality.softParticles", platform), _defaultSoftParticles);
            _softParticlesProp.OnChanged += SoftParticles_OnChanged;
            QualitySettings.softParticles = _softParticlesProp.Value;

            _realtimeReflectionProbesProp = root.Schema.GetOwn(string.Format("{0}.quality.realtimeReflectionProbes", platform), _defaultRealtimeReflectionProbes);
            _realtimeReflectionProbesProp.OnChanged += RealtimeReflectionProbes_OnChanged;
            QualitySettings.realtimeReflectionProbes = _realtimeReflectionProbesProp.Value;

            _billboardsProp = root.Schema.GetOwn(string.Format("{0}.quality.billboards", platform), _defaultBillboards);
            _billboardsProp.OnChanged += Billboards_OnChanged;
            QualitySettings.billboardsFaceCameraPosition = _billboardsProp.Value;

            _shadowQualityProp = root.Schema.GetOwn(string.Format("{0}.quality.shadowQuality", platform), _defaultShadowQuality.ToString());
            _shadowQualityProp.OnChanged += Shadows_OnChanged;
            QualitySettings.shadows = _shadowQualityProp.Value.ToEnum<ShadowQuality>();

            _shadowMaskProp = root.Schema.GetOwn(string.Format("{0}.quality.shadowMask", platform), _defaultShadowMask.ToString());
            _shadowMaskProp.OnChanged += ShadowMask_OnChanged;
            QualitySettings.shadowmaskMode = _shadowMaskProp.Value.ToEnum<ShadowmaskMode>();

            _shadowResolutionProp = root.Schema.GetOwn(string.Format("{0}.quality.shadowResolution", platform), _defaultShadowResolution.ToString());
            _shadowResolutionProp.OnChanged += ShadowResolution_OnChanged;
            QualitySettings.shadowResolution = _shadowResolutionProp.Value.ToEnum<ShadowResolution>();

            _shadowProjectionProp = root.Schema.GetOwn(string.Format("{0}.quality.shadowProjection", platform), _defaultShadowProjection.ToString());
            _shadowProjectionProp.OnChanged += ShadowProjection_OnChanged;
            QualitySettings.shadowProjection = _shadowProjectionProp.Value.ToEnum<ShadowProjection>();

            _blendWeightsProp = root.Schema.GetOwn(string.Format("{0}.quality.blendWeights", platform), _defaultBlendWeights.ToString());
            _blendWeightsProp.OnChanged += BlendWeights_OnChanged;
            QualitySettings.blendWeights = _blendWeightsProp.Value.ToEnum<BlendWeights>();
        }

        public void Teardown()
        {
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

        private void BlendWeights_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            QualitySettings.blendWeights = _blendWeightsProp.Value.ToEnum<BlendWeights>();
        }

        private void ShadowProjection_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            QualitySettings.shadowProjection = _shadowProjectionProp.Value.ToEnum<ShadowProjection>();
        }

        private void ShadowResolution_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            QualitySettings.shadowResolution = _shadowResolutionProp.Value.ToEnum<ShadowResolution>();
        }

        private void ShadowMask_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            QualitySettings.shadowmaskMode = _shadowMaskProp.Value.ToEnum<ShadowmaskMode>();
        }

        private void Shadows_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            QualitySettings.shadows = _shadowQualityProp.Value.ToEnum<ShadowQuality>();
        }

        private void Billboards_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            QualitySettings.billboardsFaceCameraPosition = _billboardsProp.Value;
        }

        private void RealtimeReflectionProbes_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            QualitySettings.realtimeReflectionProbes = _realtimeReflectionProbesProp.Value;
        }

        private void SoftParticles_OnChanged(
            ElementSchemaProp<bool> prop,
            bool prev,
            bool next)
        {
            QualitySettings.softParticles = _softParticlesProp.Value;
        }

        private void Aa_OnChanged(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            QualitySettings.antiAliasing = _aaProp.Value;
        }

        private void Aniso_OnChanged(
            ElementSchemaProp<string> prop,
            string prev,
            string next)
        {
            QualitySettings.anisotropicFiltering = _anisoProp.Value.ToEnum<AnisotropicFiltering>();
        }

        private void TextureLimit_OnChanged(
            ElementSchemaProp<int> prop,
            int prev,
            int next)
        {
            QualitySettings.masterTextureLimit = _textureLimitProp.Value;
        }
    }
}