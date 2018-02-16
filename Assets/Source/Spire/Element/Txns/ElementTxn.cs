using System.Collections.Generic;
using System.Globalization;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Container for a list of <c>ElementActionData</c>. Entire transaction is
    /// applied atomically, i.e. all or nothing.
    /// </summary>
    public class ElementTxn
    {
        /// <summary>
        /// Actions.
        /// </summary>
        public readonly List<ElementActionData> Actions = new List<ElementActionData>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementTxn()
        {
            //
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="actions">Actions.</param>
        public ElementTxn(ElementActionData[] actions)
        {
            Actions.AddRange(actions);
        }

        /// <summary>
        /// Creates a create action and returns self.
        /// </summary>
        /// <param name="parentId">Parent id.</param>
        /// <param name="elementType">Type of element.</param>
        /// <returns></returns>
        public ElementTxn Create(string parentId, int elementType)
        {
            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.CREATE,
                ParentId = parentId,
                ElementType = elementType
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
                SchemaType = ElementActionSchemaTypes.VEC3,
                Key = key,
                Value = string.Format("{{ \"x\": {0}, \"y\": {1}, \"z\": {2} }}",
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
                SchemaType = ElementActionSchemaTypes.VEC3,
                Key = key,
                Value = string.Format("{{ \"r\": {0}, \"g\": {1}, \"b\": {2}, \"a\": {3} }}",
                    value.r,
                    value.g,
                    value.b,
                    value.a)
            });

            return this;
        }
    }
}