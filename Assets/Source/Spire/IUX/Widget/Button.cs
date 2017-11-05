﻿
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Button
    /// </summary>
    public class Button : AimableWidget
    {
        /// <summary>
        /// Current theta as a result of intention's steadiness
        /// </summary>
        private float _steadinessThetaDegress;

        /// <summary>
        /// Current activation percentage
        /// </summary>
        private float _activation;

        /// <summary>
        /// Keyword recognizer for voice activation
        /// </summary>
        private KeywordRecognizer _keywordRecognizer;

        /// <summary>
        /// State management for the button
        /// </summary>
        private FiniteStateMachine _states;

        /// <summary>
        /// Display text
        /// </summary>
        public Caption Caption;

        /// <summary>
        /// Transform affected by the steadiness of intention
        /// </summary>
        public Transform SteadinessTransform;

        /// <summary>
        /// The rotation in degrees of the steadiness transform
        /// </summary>
        public float SteadinessRotation;

        /// <summary>
        /// Fills with activation percentage
        /// </summary>
        public Image ActivationImage;

        /// <summary>
        /// Activates when the button is focused
        /// </summary>
        public AudioSource FocusedAudioSource;

        /// <summary>
        /// Fill Rate Multiplier
        /// </summary>
        public AnimationCurve FillDecay = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        /// <summary>
        /// Activation percentage
        /// </summary>
        public float Activation
        {
            get { return _activation; }
            set { _activation = value; }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public void SetSchema(ButtonSchema schema)
        {
            if (schema == null)
            {
                return;
            }

            base.SetSchema(schema);

            if (Caption != null)
            {
                Caption.SetSchema(schema.Caption);
            }

            var voiceActivator = schema.VoiceActivator;
            if (string.IsNullOrEmpty(voiceActivator))
            {
                if (schema.Caption != null
                && !string.IsNullOrEmpty(schema.Caption.Text))
                {
                    // default to the caption text if there is no voice activator specified
                    voiceActivator = schema.Caption.Text;
                }
            }

            if (!string.IsNullOrEmpty(voiceActivator))
            {
                StartKeywordRecognizer(voiceActivator);
            }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            var ready = new ButtonReadyState(this);
            ready.OnTransition += type => _states.Change(type);

            var activating = new ButtonActivatingState(this);
            activating.OnTransition += type => _states.Change(type);

            var activated = new ButtonActivatedState(this);
            activated.OnTransition += type => _states.Change(type);

            _states = new FiniteStateMachine(new IState[]
            {
                ready, activating, activated
            });

            _states.Change<ButtonReadyState>();
        }

        /// <summary>
        /// Frame based update
        /// </summary>
        public override void Update()
        {
            base.Update();

            var deltaTime = Time.deltaTime;

            UpdateColliders();

            _states.Update(deltaTime);

            UpdateActivation();

            if (FocusedAudioSource != null)
            {
                FocusedAudioSource.gameObject.SetActive(IsFocused);
            }

            if (float.IsInfinity(_activation)
             || float.IsNaN(_activation))
            {
                _activation = 0.0f;
                _states.Change<ButtonReadyState>();
            }

            UpdateSteadiness(deltaTime);
        }

        /// <summary>
        /// Destroy necessary items here
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            if (_keywordRecognizer != null)
            {
                _keywordRecognizer.Stop();
                _keywordRecognizer.Dispose();
            }
        }

        /// <summary>
        /// Updates the steadiness feedback
        /// </summary>
        /// <param name="deltaTime"></param>
        private void UpdateSteadiness(float deltaTime)
        {
            const float STEADINESS_LERP_RATE_MAGIC_NUMBER = 8.0f;

            if (SteadinessTransform != null)
            {
                var targetRotation = IsFocused ? SteadinessRotation * Intention.Steadiness : 0.0f;
                _steadinessThetaDegress = Mathf.Lerp(_steadinessThetaDegress, targetRotation, deltaTime * STEADINESS_LERP_RATE_MAGIC_NUMBER);
                var focusTween = ShowIfFocusedWidget != null ? ShowIfFocusedWidget.Tween : 1.0f;
                SteadinessTransform.localRotation = Quaternion.Euler(0, 0, _steadinessThetaDegress);
                SteadinessTransform.localScale = Vector3.one * focusTween;
            }
        }

        /// <summary>
        /// Updates the colliders
        /// </summary>
        private void UpdateColliders()
        {
            if (FocusCollider == null)
            {
                return;
            }

            FocusCollider.enabled
                = IsInteractable
                  && IsVisible
                  && Tween > 0.99f;

            if (BufferCollider != null)
            {
                BufferCollider.enabled
                    = FocusCollider.enabled;
            }
        }
        
        /// <summary>
        /// Updates visual activation feedback.
        /// </summary>
        private void UpdateActivation()
        {
            if (ActivationImage != null)
            {
                ActivationImage.fillAmount = _activation;
            }
        }
        
        /// <summary>
        /// Starts a KeywordRecognizer session to listen to key word or phrase
        /// </summary>
        /// <param name="str">Keyword or phrase to listen for</param>
        private void StartKeywordRecognizer(string str)
        {
            if (_keywordRecognizer != null)
            {
                _keywordRecognizer.Stop();
                _keywordRecognizer.Dispose();
            }

            _keywordRecognizer = new KeywordRecognizer(new[] { str });
            _keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            _keywordRecognizer.Start();
        }

        /// <summary>
        /// Invoked when a recognizable word or phrase is spoken
        /// </summary>
        /// <param name="args"></param>
        private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            if (IsInteractable 
            && _states.StateType != typeof(ButtonActivatingState))
            {
                _keywordRecognizer.Stop();
                _keywordRecognizer.Dispose();
                _states.Change<ButtonActivatedState>();
            }
        }
    }
}
