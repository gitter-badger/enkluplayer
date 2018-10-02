using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.CreateOrganizationDevice;
using UnityEngine;

namespace CreateAR.EnkluPlayer
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
        private readonly DeviceResourceUpdateService _deviceUpdateService;
        
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
            ApiController api,
            DeviceResourceUpdateService deviceUpdateService)
        {
            _ui = ui;
            _messages = messages;
            _config = config;
            _preferences = preferences;
            _api = api;
            _deviceUpdateService = deviceUpdateService;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            _frame = _ui.CreateFrame();

            // show loading UI
            int id;
            _ui
                .Open<ICommonLoadingView>(new UIReference
                {
                    UIDataId = UIDataIds.LOADING
                }, out id);

            // continues to next state
            Action @continue = () => _messages.Publish(MessageTypes.DEVICE_REGISTRATION_COMPLETE);

            // check user prefs to see if we need to register
            _preferences
                .ForUser(_config.Network.Credentials.UserId)
                .OnSuccess(data =>
                {
                    var registrations = data.Data.DeviceRegistrations;
                    ValidateRegistrations(registrations, valid =>
                    {
                        if (valid.Length > 0)
                        {
                            // resave
                            data.Queue((p, n) =>
                            {
                                p.DeviceRegistrations = valid;

                                n(p);
                            });

                            Log.Info(this, "Device is already registered.");
                            
                            // start update service
                            _deviceUpdateService.Initialize(valid);

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
                                                    var tokens = new List<IAsyncToken<HttpResponse<Response>>>();
                                                    foreach (var org in organizations)
                                                    {
                                                        var token = _api
                                                            .Devices
                                                            .CreateOrganizationDevice(org.Id, new Request
                                                            {
                                                                Name = "HoloLens",
                                                                Description = "My new device.",
                                                                Token = SystemInfo.deviceUniqueIdentifier
                                                            });
                                                        tokens.Add(token);
                                                    }

                                                    // wait till they are all done
                                                    Async
                                                        .All(tokens.ToArray())
                                                        .OnSuccess(responses =>
                                                        {
                                                            var responseRegistrations = responses
                                                                .Select(res => new DeviceRegistration
                                                                {
                                                                    DeviceId = res.Payload.Body.Id,
                                                                    OrgId = res.Payload.Body.Org
                                                                })
                                                                .ToArray();

                                                            data.Queue((before, next) =>
                                                            {
                                                                // save them
                                                                before.DeviceRegistrations = responseRegistrations;

                                                                next(before);
                                                            });

                                                            // start device update service
                                                            _deviceUpdateService.Initialize(responseRegistrations);

                                                            @continue();
                                                        })
                                                        .OnFailure(exception =>
                                                        {
                                                            Log.Error(this, "Could not create Device resources : {0}", exception);

                                                            _ui
                                                                .Open<ICommonErrorView>(new UIReference
                                                                {
                                                                    UIDataId = UIDataIds.ERROR
                                                                })
                                                                .OnSuccess(err =>
                                                                {
                                                                    err.Message = "Could not register device. Please try again later.";
                                                                    err.Action = "Okay";
                                                                    err.OnOk += @continue;
                                                                });
                                                        });
                                                };

                                                el.OnCancel += () =>
                                                {
                                                    data.Queue((before, next) =>
                                                    {
                                                        before.IgnoreDeviceRegistration = true;
                                                        next(before);
                                                    });

                                                    @continue();
                                                };
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
                    });
                    
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

        /// <summary>
        /// Validates registrations and returns the validated list.
        /// </summary>
        /// <param name="registrations">Saved registrations.</param>
        /// <param name="callback">Callback.</param>
        private void ValidateRegistrations(
            DeviceRegistration[] registrations,
            Action<DeviceRegistration[]> callback)
        {
            GetMyRegistrations()
                .OnSuccess(callback)
                .OnFailure(ex =>
                {
                    Log.Error(this, "Could not get device registrations : {0}", ex);

                    callback(registrations);
                });
        }

        private IAsyncToken<DeviceRegistration[]> GetMyRegistrations()
        {
            var token = new AsyncToken<DeviceRegistration[]>();

            // get list of organizations
            _api
                .Organizations
                .GetMyOrganizations()
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        var tokens = response.Payload.Body
                            .Select(org => GetOrgRegistrations(org.Id))
                            .ToArray();
                        Async
                            .All(tokens)
                            .OnSuccess(multiArray => token.Succeed(multiArray
                                .SelectMany(arr => arr)
                                .ToArray()))
                            .OnFailure(token.Fail);
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }

        private IAsyncToken<DeviceRegistration[]> GetOrgRegistrations(string orgId)
        {
            var token = new AsyncToken<DeviceRegistration[]>();

            _api
                .Devices
                .GetOrganizationDevices(orgId)
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        token.Succeed(response
                            .Payload
                            .Body
                            .Select(body =>
                                new DeviceRegistration
                                {
                                    OrgId = orgId,
                                    DeviceId = body.Id
                                })
                            .ToArray());
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }
    }
}