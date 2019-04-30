using System;
using System.Collections;
using System.Linq;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using Enklu.Orchid;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// API for interacting with experiences.
    /// </summary>
    public class ExperienceJsApi
    {
        /// <summary>
        /// Simple POCO for js api.
        /// </summary>
        public class ExperienceJs
        {
            public string id;
            public string name;
            public string description;
            public bool isPublic;
            public string createdAt;
            public string updatedAt;
        }

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IMessageRouter _messages;
        private readonly IBootstrapper _bootstrapper;
        private readonly ApiController _api;
        private readonly ApplicationConfig _config;

        /// <summary>
        /// True iff there is a timer to load another experience.
        /// </summary>
        private bool _isPlayingTimed = false;

        /// <summary>
        /// All experiences.
        /// </summary>
        private ExperienceJs[] _all = new ExperienceJs[0];

        /// <summary>
        /// Constructor.
        /// </summary>
        public ExperienceJsApi(
            IMessageRouter messages,
            IBootstrapper bootstrapper,
            ApiController api,
            ApplicationConfig config)
        {
            _messages = messages;
            _bootstrapper = bootstrapper;
            _api = api;
            _config = config;
        }

        /// <summary>
        /// Refreshes list of experiences.
        /// </summary>
        public void refresh(IJsCallback callback)
        {
            _api
                .Apps
                .GetMyApps()
                .OnSuccess(response =>
                {
                    if (response.Payload.Success)
                    {
                        _all = response.Payload.Body
                            .Select(exp => new ExperienceJs
                            {
                                id = exp.Id,
                                name = exp.Name,
                                description = exp.Description,
                                isPublic = exp.Ispublic,
                                createdAt = exp.CreatedAt,
                                updatedAt = exp.UpdatedAt
                            })
                            .ToArray();

                        callback.Invoke();
                    }
                    else
                    {
                        callback.Invoke(response.Payload.Error);
                    }
                })
                .OnFailure(ex => callback.Invoke(ex.Message)); 
        }

        /// <summary>
        /// Retrieves all experiences.
        /// </summary>
        /// <returns></returns>
        public ExperienceJs[] all()
        {
            return _all;
        }

        /// <summary>
        /// Retrieves experience information by name.
        /// </summary>
        /// <param name="name">The name of the experience.</param>
        /// <returns></returns>
        public ExperienceJs byName(string name)
        {
            for (int i = 0, len = _all.Length; i < len; i++)
            {
                var exp = _all[i];
                if (exp.name == name)
                {
                    return exp;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves experience information by id.
        /// </summary>
        /// <param name="id">The id of the experience.</param>
        /// <returns></returns>
        public ExperienceJs byId(string id)
        {
            for (int i = 0, len = _all.Length; i < len; i++)
            {
                var exp = _all[i];
                if (exp.id == id)
                {
                    return exp;
                }
            }

            return null;
        }

        /// <summary>
        /// Plays an experience.
        /// </summary>
        /// <param name="id">The id of the experience.</param>
        public void play(string id)
        {
            if (null == byId(id))
            {
                Log.Warning(this, "Unknown experience id '{0}'.", id);
                return;
            }

            Log.Info(this, "Playing '{0}'.", id);

            _config.Play.Edit = false;
            _config.Play.AppId = id;

            _messages.Publish(MessageTypes.LOAD_APP, new LoadAppEvent { DoNotPersist = true });
        }

        /// <summary>
        /// Plays an experience for a specified number of seconds, at which
        /// point, the current experience is reloaded.
        /// </summary>
        /// <param name="id">The id of the experience.</param>
        /// <param name="seconds">The number of seconds to wait before returning.</param>
        public void playTimed(string id, float seconds)
        {
            if (_isPlayingTimed)
            {
                Log.Warning(this, "Could not play timed, as there is another experience already queued.");
                return;
            }

            if (null == byId(id))
            {
                Log.Warning(this, "Unknown experience id '{0}'.", id);
                return;
            }

            _isPlayingTimed = true;

            var currentId = _config.Play.AppId;
            var currentEdit = _config.Play.Edit;
            
            // play first
            play(id);

            // then add a timer for reloading
            _bootstrapper.BootstrapCoroutine(Wait(seconds, () =>
            {
                Log.Info(this, "Time is up! Playing '{0}'.", currentId);

                _isPlayingTimed = false;

                if (currentEdit)
                {
                    edit(currentId);
                }
                else
                {
                    play(currentId);
                }
            }));
        }
        
        /// <summary>
        /// Plays the experience in edit mode.
        /// </summary>
        /// <param name="id">The id of the experience.</param>
        public void edit(string id)
        {
            if (null == byId(id))
            {
                Log.Warning(this, "Unknown experience id '{0}'.", id);
                return;
            }

            _config.Play.Edit = true;
            _config.Play.AppId = id;

            _messages.Publish(MessageTypes.LOAD_APP, new LoadAppEvent { DoNotPersist = true });
        }

        /// <summary>
        /// Waits for a timeout before calling a function.
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator Wait(float seconds, Action callback)
        {
            yield return new WaitForSecondsRealtime(seconds);

            callback();
        }
    }
}