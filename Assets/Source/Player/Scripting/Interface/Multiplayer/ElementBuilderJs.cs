using System;
using CreateAR.Commons.Unity.Logging;
using Enklu.Data;
using Enklu.Mycelium.Messages.Experience;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
{
    public class ElementBuilderJs
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IMultiplayerController _multiplayer;
        private readonly IElementJsCache _elements;

        /// <summary>
        /// Id of the current user.
        /// </summary>
        private readonly string _userId;

        /// <summary>
        /// Id of the parent.
        /// </summary>
        private readonly string _parentId;

        /// <summary>
        /// The data to use in the create request.
        /// </summary>
        private readonly ElementData _element = new ElementData
        {
            Id = Guid.NewGuid().ToString()
        };

        /// <summary>
        /// Owner, or null if none.
        /// </summary>
        private string _owner;

        /// <summary>
        /// Type of expiration.
        /// </summary>
        private ElementExpirationType _expiration = ElementExpirationType.Session;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementBuilderJs(
            IMultiplayerController multiplayer,
            IElementJsCache elements,
            string userId,
            string parentId)
        {
            _multiplayer = multiplayer;
            _elements = elements;
            _userId = userId;
            _parentId = parentId;
        }

        public void build(IJsCallback callback)
        {
            Log.Info(this, "Attempting to build element.");

            _multiplayer
                .Create(_parentId, _element, _owner, _expiration)
                .OnSuccess(el =>
                {
                    Log.Info(this, "Build element successfully.");

                    callback.Apply(this, null, _elements.Element(el));
                })
                .OnFailure(ex =>
                {
                    Log.Warning(this, "Could not build element: {0}", ex.Message);

                    callback.Apply(this, ex.Message);
                });
        }

        public ElementBuilderJs asset(ElementJs element)
        {
            _element.Schema.Strings["assetSrc"] = element.Element.Schema.GetOwn("assetSrc", "").Value;

            return this;
        }

        public ElementBuilderJs expiration(string expirationType)
        {
            _expiration = EnumExtensions.Parse(expirationType, ElementExpirationType.Session);

            return this;
        }

        public ElementBuilderJs ownership(string owner)
        {
            _owner = owner == MultiplayerJsInterface.OwnershipTypes.SELF
                ? _userId
                : owner;

            return this;
        }

        public ElementBuilderJs name(string name)
        {
            _element.Schema.Strings["name"] = name;

            return this;
        }

        public ElementBuilderJs setString(string name, string value)
        {
            if (name == "id")
            {
                return this;
            }

            _element.Schema.Strings[name] = value;

            return this;
        }

        public ElementBuilderJs setBool(string name, bool value)
        {
            _element.Schema.Bools[name] = value;

            return this;
        }

        public ElementBuilderJs setNumber(string name, float value)
        {
            _element.Schema.Floats[name] = value;

            return this;
        }

        public ElementBuilderJs setVec(string name, Vec3 value)
        {
            _element.Schema.Vectors[name] = value;

            return this;
        }

        public ElementBuilderJs setCol(string name, Col4 value)
        {
            _element.Schema.Colors[name] = value;

            return this;
        }
    }
}