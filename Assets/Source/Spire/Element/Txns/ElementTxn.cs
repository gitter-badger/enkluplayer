using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public class ElementResponse
    {
        public bool Success;
        public string Error;
        public readonly List<Element> Elements = new List<Element>();
    }

    /// <summary>
    /// Container for a list of <c>ElementActionData</c>. Entire transaction is
    /// applied atomically, i.e. all or nothing.
    /// </summary>
    public class ElementTxn
    {
        /// <summary>
        /// Used to generate session-unique id.
        /// </summary>
        private static uint IDS = 1;

        /// <summary>
        /// Serializer.
        /// </summary>
        private static readonly JsonSerializer _serializer = new JsonSerializer();

        /// <summary>
        /// Unique id.
        /// </summary>
        public readonly uint Id;

        /// <summary>
        /// Id of the scene.
        /// </summary>
        public readonly string SceneId;

        /// <summary>
        /// Actions.
        /// </summary>
        public readonly List<ElementActionData> Actions = new List<ElementActionData>();
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementTxn(string sceneId)
        {
            SceneId = sceneId;
            Id = IDS++;
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sceneId">Id of the scene.</param>
        /// <param name="actions">Actions.</param>
        public ElementTxn(string sceneId, ElementActionData[] actions)
            : this(sceneId)
        {
            Actions.AddRange(actions);
        }

        /// <summary>
        /// Creates a create action and returns self.
        /// </summary>
        /// <param name="parentId">Parent id.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public ElementTxn Create(string parentId, ElementData data)
        {
            byte[] bytes;
            _serializer.Serialize(data, out bytes);

            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.CREATE,
                ParentId = parentId,
                ElementId = data.Id,
                Element = data
            });

            return this;
        }
        
        /// <summary>
        /// Creates a delete element action and returns self.
        /// </summary>
        /// <param name="elementId">The id of the element.</param>
        /// <returns></returns>
        public ElementTxn Delete(string elementId)
        {
            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.DELETE,
                ElementId = elementId
            });

            return this;
        }

        /// <summary>
        /// Creates an update action and returns self.
        /// </summary>
        /// <param name="elementId">Id of the element to update.</param>
        /// <param name="key">The schema key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ElementTxn Update(
            string elementId,
            string key,
            int value)
        {
            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.UPDATE,
                ElementId = elementId,
                SchemaType = ElementActionSchemaTypes.INT,
                Key = key,
                Value = value.ToString()
            });

            return this;
        }

        /// <summary>
        /// Creates an update action and returns self.
        /// </summary>
        /// <param name="elementId">Id of the element to update.</param>
        /// <param name="key">The schema key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ElementTxn Update(
            string elementId,
            string key,
            float value)
        {
            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.UPDATE,
                ElementId = elementId,
                SchemaType = ElementActionSchemaTypes.FLOAT,
                Key = key,
                Value = value.ToString(CultureInfo.InvariantCulture)
            });

            return this;
        }

        /// <summary>
        /// Creates an update action and returns self.
        /// </summary>
        /// <param name="elementId">Id of the element to update.</param>
        /// <param name="key">The schema key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ElementTxn Update(
            string elementId,
            string key,
            string value)
        {
            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.UPDATE,
                ElementId = elementId,
                SchemaType = ElementActionSchemaTypes.STRING,
                Key = key,
                Value = value
            });

            return this;
        }

        /// <summary>
        /// Creates an update action and returns self.
        /// </summary>
        /// <param name="elementId">Id of the element to update.</param>
        /// <param name="key">The schema key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ElementTxn Update(
            string elementId,
            string key,
            bool value)
        {
            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.UPDATE,
                ElementId = elementId,
                SchemaType = ElementActionSchemaTypes.BOOL,
                Key = key,
                Value = value.ToString()
            });

            return this;
        }

        /// <summary>
        /// Creates an update action and returns self.
        /// </summary>
        /// <param name="elementId">Id of the element to update.</param>
        /// <param name="key">The schema key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ElementTxn Update(
            string elementId,
            string key,
            Vec3 value)
        {
            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.UPDATE,
                ElementId = elementId,
                SchemaType = ElementActionSchemaTypes.VEC3,
                Key = key,
                Value = string.Format("{0},{1},{2}",
                    value.x,
                    value.y,
                    value.z)
            });

            return this;
        }

        /// <summary>
        /// Creates an update action and returns self.
        /// </summary>
        /// <param name="elementId">Id of the element to update.</param>
        /// <param name="key">The schema key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public ElementTxn Update(
            string elementId,
            string key,
            Col4 value)
        {
            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.UPDATE,
                ElementId = elementId,
                SchemaType = ElementActionSchemaTypes.COL4,
                Key = key,
                Value = string.Format("{0},{1},{2},{3}",
                    value.r,
                    value.g,
                    value.b,
                    value.a)
            });

            return this;
        }
    }
}