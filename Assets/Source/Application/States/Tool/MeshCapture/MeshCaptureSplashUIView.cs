﻿using System;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// UI for mesh capture.
    /// </summary>
    public class MeshCaptureSplashUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// True iff autosave is enabled.
        /// </summary>
        private bool _isAutoSaving = false;

        /// <summary>
        /// Last time we autosaved.
        /// </summary>
        private DateTime _lastAutoSave;

        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-back")]
        public ButtonWidget BtnBack { get; set; }

        [InjectElements("..btn-save")]
        public ButtonWidget BtnSave{ get; set; }

        [InjectElements("..btn-options")]
        public ButtonWidget BtnOptions { get; set; }

        [InjectElements("..tgl-autosave")]
        public ToggleWidget TglAutoSave { get; set; }

        /// <summary>
        /// Called when back is requested.
        /// </summary>
        public event Action OnBack;

        /// <summary>
        /// Called when save is requested.
        /// </summary>
        public event Action OnSave;
        
        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnBack.Activator.OnActivated += _ =>
            {
                if (null != OnBack)
                {
                    OnBack();
                }
            };

            BtnSave.Activator.OnActivated += _ =>
            {
                if (null != OnSave)
                {
                    OnSave();
                }
            };

            BtnOptions.Activator.OnActivated += _ =>
            {
                // 
            };

            TglAutoSave.OnValueChanged += _ =>
            {
                if (TglAutoSave.Value)
                {
                    BtnSave.Activator.InteractionEnabled = false;
                    BtnSave.Schema.Set("ready.color", "Disabled");

                    _lastAutoSave = DateTime.MinValue;
                    _isAutoSaving = true;
                }
                else
                {
                    BtnSave.Activator.InteractionEnabled = true;
                    BtnSave.Schema.Set("ready.color", "Ready");

                    _isAutoSaving = false;
                }
            };
        }

        /// <inheritdoc cref="MonoBehaviour" />
        private void Update()
        {
            if (_isAutoSaving)
            {
                var now = DateTime.Now;
                if (now.Subtract(_lastAutoSave).TotalSeconds > 5)
                {
                    _lastAutoSave = now;

                    if (null != OnSave)
                    {
                        OnSave();
                    }
                }
            }
        }
    }
}