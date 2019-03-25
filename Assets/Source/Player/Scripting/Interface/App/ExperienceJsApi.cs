using System;
using System.Linq;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;

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
        private readonly ApiController _api;
        private readonly ApplicationConfig _config;

        /// <summary>
        /// All experiences.
        /// </summary>
        private ExperienceJs[] _all = new ExperienceJs[0];

        /// <summary>
        /// Constructor.
        /// </summary>
        public ExperienceJsApi(
            IMessageRouter messages,
            ApiController api,
            ApplicationConfig config)
        {
            _messages = messages;
            _api = api;
            _config = config;
        }

        /// <summary>
        /// Refreshes list of experiences.
        /// </summary>
        public void refresh(Action<string> callback)
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

                        callback(null);
                    }
                    else
                    {
                        callback(response.Payload.Error);
                    }
                })
                .OnFailure(ex => callback(ex.Message)); 
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

            _config.Play.Edit = false;
            _config.Play.AppId = id;

            _messages.Publish(MessageTypes.PLAY);
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

            _messages.Publish(MessageTypes.PLAY);
        }
    }
}