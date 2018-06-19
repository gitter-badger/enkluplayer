using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.CreateOrganizationDevice;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Determines if device needs to be registered.
    /// </summary>
    public class DeviceRegistrationApplicationState : IState
    {
        /// <summary>
        /// UI manager.
        /// </summary>
        private readonly IUIManager _ui;
        private readonly IMessageRouter _messages;
        private readonly ApplicationConfig _config;
        private readonly UserPreferenceService _preferences;
        private readonly ApiController _api;

        /// <summary>
        /// Loading view id.
        /// </summary>
        private int _loadingId;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DeviceRegistrationApplicationState(
            IUIManager ui,
            IMessageRouter messages,
            ApplicationConfig config,
            UserPreferenceService preferences,
            ApiController api)
        {
            _ui = ui;
            _messages = messages;
            _config = config;
            _preferences = preferences;
            _api = api;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _frame = _ui.CreateFrame();

            // show loading UI
            _ui
                .Open<ICommonLoadingView>(new UIReference
                {
                    UIDataId = UIDataIds.LOADING
                }, out _loadingId);

            // continues to next state
            Action @continue = () => _messages.Publish(MessageTypes.DEVICE_REGISTRATION_COMPLETE);

            // check user prefs to see if we need to register
            _preferences
                .ForUser(_config.Network.Credentials.UserId)
                .OnSuccess(data =>
                {
                    var orgs = data.Data.Orgs;
                    if (orgs.Length > 0)
                    {
                        Log.Info(this, "Device is already registered.");

                        @continue();
                    }
                    else if (data.Data.IgnoreDeviceRegistration)
                    {
                        Log.Info(this, "User has previously ignored registration.");

                        @continue();
                    }
                    else
                    {
                        // get list of organizations
                        _api
                            .Organizations
                            .GetMyOrganizations()
                            .OnSuccess(response =>
                            {
                                // open confirmation UI
                                var organizations = response.Payload.Body;
                                if (organizations.Length > 0)
                                {
                                    int confirmViewId;
                                    _ui
                                        .Open<ConfirmDeviceRegistrationUIView>(new UIReference
                                        {
                                            UIDataId = "Device.Registration.Confirm"
                                        }, out confirmViewId)
                                        .OnSuccess(el =>
                                        {
                                            el.Populate(organizations);
                                            el.OnConfirm += () =>
                                            {
                                                // open load UI
                                                _ui.Close(confirmViewId);
                                                _ui.Open<ICommonLoadingView>(new UIReference{ UIDataId = UIDataIds.LOADING });

                                                // create device resources
                                                var responses = new List<IAsyncToken<HttpResponse<Response>>>();
                                                foreach (var org in organizations)
                                                {
                                                    var token = _api
                                                        .Devices
                                                        .CreateOrganizationDevice(org.Id, new Request
                                                        {
                                                            Name = "HoloLens",
                                                            Description = string.Format("{0}'s device.", _config.Network.Credentials.Email),
                                                            Token = SystemInfo.deviceUniqueIdentifier
                                                        });
                                                    responses.Add(token);
                                                }

                                                // wait till they are all done
                                                Async
                                                    .All(responses.ToArray())
                                                    .OnSuccess(_ => @continue())
                                                    .OnFailure(exception =>
                                                    {
                                                        Log.Error(this, "Could not create Device resources : {0}", exception);

                                                        _ui
                                                            .Open<ICommonErrorView>(new UIReference
                                                            {
                                                                UIDataId = UIDataIds.LOADING
                                                            })
                                                            .OnSuccess(err =>
                                                            {
                                                                err.Message = "Could not register device. Please try again later.";
                                                                err.Action = "Okay";
                                                                err.OnOk += @continue;
                                                            });
                                                    });
                                            };

                                            el.OnCancel += @continue;
                                        })
                                        .OnFailure(exception =>
                                        {
                                            Log.Error(this, "Could not open Device.Registration.Confirm : {0}", exception);

                                            @continue();
                                        });
                                }
                                else
                                {
                                    Log.Info(this, "Not a member of any organizations.");

                                    @continue();
                                }
                            })
                            .OnFailure(exception =>
                            {
                                Log.Error(this, "Could not get my organizations : {0}", exception);

                                @continue();
                            });
                    }
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load UserPreferenceData. Skipping device registration : {0}", exception);

                    @continue();
                });
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc />
        public void Exit()
        {
            _frame.Release();
        }
    }
}
