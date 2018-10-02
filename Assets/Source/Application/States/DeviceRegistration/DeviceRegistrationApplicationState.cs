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
using Void = CreateAR.Commons.Unity.Async.Void;

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
        private readonly UserPreferenceService _preferences;
        private readonly ApiController _api;
        private readonly DeviceResourceUpdateService _deviceUpdateService;
        
        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Authoratative server registrations for this device.
        /// </summary>
        private DeviceRegistration[] _serverRegistrations;

        /// <summary>
        /// Organizations this user belongs to.
        /// </summary>
        private Trellis.Messages.GetMyOrganizations.Body[] _orgs;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DeviceRegistrationApplicationState(
            IUIManager ui,
            IMessageRouter messages,
            UserPreferenceService preferences,
            ApiController api,
            DeviceResourceUpdateService deviceUpdateService)
        {
            _ui = ui;
            _messages = messages;
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

            // get registrations from server
            // get orgs
            Async
                .All(
                    GetServerRegistrations(),
                    GetOrgs())
                .OnSuccess(_ =>
                {
                    // ids of orgs we are not registered with
                    var missingRegistions = new List<string>();

                    // compare organizations list against authoritative list
                    for (int i = 0, len = _orgs.Length; i < len; i++)
                    {
                        var org = _orgs[i];
                        var registration = _serverRegistrations.FirstOrDefault(reg => reg.OrgId == org.Id);

                        // we are not registered with this organization
                        if (null == registration)
                        {
                            missingRegistions.Add(org.Id);
                        }
                    }

                    // if there are some missing, then request we add
                    if (missingRegistions.Count > 0)
                    {
                        Register(missingRegistions)
                            .OnSuccess(registrations =>
                            {
                                registrations = _serverRegistrations.Add(registrations);

                                _deviceUpdateService.Initialize(registrations);

                                UpdateUserPrefs(registrations);

                                @continue();
                            })
                            .OnFailure(ex => @continue());
                    }
                    // otherwise we are already registered with each org
                    else
                    {
                        UpdateUserPrefs(_serverRegistrations);

                        _deviceUpdateService.Initialize(_serverRegistrations);

                        @continue();
                    }
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
        /// Saves the updates list of device registrations.
        /// </summary>
        /// <param name="registrations">The registrations.</param>
        private void UpdateUserPrefs(DeviceRegistration[] registrations)
        {
            // update local data in case device changed
            _preferences
                .ForCurrentUser()
                .OnSuccess(prefs => prefs.Queue((prev, next) =>
                {
                    prev.DeviceRegistrations = registrations;

                    next(prev);
                }))
                .OnFailure(err => Log.Error(this, "Could not get user prefs : {0}", err));
        }

        /// <summary>
        /// Retrieves all server registrations for this device.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> GetServerRegistrations()
        {
            var token = new AsyncToken<Void>();

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
                            .OnSuccess(multiArray =>
                            {
                                _serverRegistrations = multiArray
                                    .SelectMany(arr => arr)
                                    .ToArray();

                                token.Succeed(Void.Instance);
                            })
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

        /// <summary>
        /// Retrieves registrations for a specific org.
        /// </summary>
        /// <param name="orgId">The org id.</param>
        /// <returns></returns>
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
                            .Where(body => body.Token == SystemInfo.deviceUniqueIdentifier)
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

        /// <summary>
        /// Retrieves all orgs this user belongs to.
        /// </summary>
        /// <returns></returns>
        private IAsyncToken<Void> GetOrgs()
        {
            var token = new AsyncToken<Void>();

            _api
                .Organizations
                .GetMyOrganizations()
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        _orgs = response.Payload.Body;

                        token.Succeed(Void.Instance);
                    }
                    else
                    {
                        token.Fail(new Exception(response.Payload.Error));
                    }
                })
                .OnFailure(token.Fail);

            return token;
        }

        /// <summary>
        /// Registers the device with a set of orgs.
        /// </summary>
        /// <param name="orgIds">The set of orgs this user belongs to.</param>
        /// <returns></returns>
        private IAsyncToken<DeviceRegistration[]> Register(IList<string> orgIds)
        {
            var token = new AsyncToken<DeviceRegistration[]>();

            // open confirmation UI
            int confirmViewId;
            _ui
                .Open<ConfirmDeviceRegistrationUIView>(new UIReference
                {
                    UIDataId = "Device.Registration.Confirm"
                }, out confirmViewId)
                .OnSuccess(el =>
                {
                    el.Populate(orgIds);
                    el.OnConfirm += () =>
                    {
                        // open load UI
                        _ui.Close(confirmViewId);
                        _ui.Open<ICommonLoadingView>(new UIReference { UIDataId = UIDataIds.LOADING });

                        // create device resources
                        var tokens = new List<IAsyncToken<HttpResponse<Response>>>();
                        foreach (var id in orgIds)
                        {
                            tokens
                                .Add(_api
                                .Devices
                                .CreateOrganizationDevice(id, new Request
                                {
                                    Name = SystemInfo.deviceUniqueIdentifier,
                                    Description = "My new device.",
                                    Token = SystemInfo.deviceUniqueIdentifier
                                }));
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
                                
                                token.Succeed(responseRegistrations);
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
                                        err.OnOk += () => token.Fail(exception);
                                    });
                            });
                    };

                    el.OnCancel += () => token.Fail(new Exception("User canceled."));
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not open Device.Registration.Confirm : {0}", exception);

                    token.Fail(exception);
                });

            return token;
        }
    }
}