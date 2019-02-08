using System;
using System.Collections.Generic;
using System.Text;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using ElementData = CreateAR.EnkluPlayer.IUX.ElementData;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Response from <c>IElementTxnManager::Request</c>.
    /// </summary>
    public class ElementResponse
    {
        /// <summary>
        /// List of affected elements.
        /// </summary>
        public readonly List<Element> Elements = new List<Element>();
    }

    /// <summary>
    /// Container for a list of <c>ElementActionData</c>. Entire transaction is
    /// applied atomically, i.e. all or nothing.
    /// </summary>
    public class ElementTxn
    {
        /// <summary>
        /// PRNG.
        /// </summary>
        private static readonly Random _Random = new Random();
        
        /// <summary>
        /// Unique id.
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// Id of the scene.
        /// </summary>
        public readonly string SceneId;

        /// <summary>
        /// True iff rollback should be tracked.
        /// </summary>
        public bool AllowRollback = true;

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
            Id = GenerateTxnId();
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
        /// Useful tostring.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var action in Actions)
            {
                builder.AppendFormat("\t\t{0}\n", action);
            }

            return string.Format(
                "[ElementTxn\n\tScene={0},\n\tActions=\n{1}]",
                SceneId,
                builder);
        }

        /// <summary>
        /// Creates a create action and returns self.
        /// </summary>
        /// <param name="parentId">Parent id.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public ElementTxn Create(string parentId, ElementData data)
        {
            ValidateElementIds(data);

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
        /// Creates a move element action and returns self.
        /// </summary>
        /// <param name="elementId">The id of the element.</param>
        /// <param name="parentId">The id of the new parent.</param>
        /// <param name="newPosition">The local position of the element after the move.</param>
        /// <param name="newRotation">The local rotation of the element after the move.</param>
        /// <param name="newScale">The local scale of the element after the move.</param>
        /// <returns></returns>
        public ElementTxn Move(
            string elementId,
            string parentId,
            Vec3 newPosition,
            Vec3 newRotation,
            Vec3 newScale)
        {
            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.MOVE,
                ElementId = elementId,
                ParentId = parentId,
                Value = newPosition
            });

            Update(elementId, "rotation", newRotation);
            Update(elementId, "scale", newScale);

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
            float value)
        {
            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.UPDATE,
                ElementId = elementId,
                SchemaType = ElementActionSchemaTypes.FLOAT,
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
            Vec3 value)
        {
            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.UPDATE,
                ElementId = elementId,
                SchemaType = ElementActionSchemaTypes.VEC3,
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
            Col4 value)
        {
            Actions.Add(new ElementActionData
            {
                Type = ElementActionTypes.UPDATE,
                ElementId = elementId,
                SchemaType = ElementActionSchemaTypes.COL4,
                Key = key,
                Value = value
            });

            return this;
        }

        /// <summary>
        /// Ensures all elements have a valid Id. Creates one if not.
        /// </summary>
        /// <param name="data">The data.</param>
        private void ValidateElementIds(ElementData data)
        {
            if (string.IsNullOrEmpty(data.Id))
            {
                data.Id = Guid.NewGuid().ToString();
            }

            for (var i = 0; i < data.Children.Length; i++)
            {
                ValidateElementIds(data.Children[i]);
            }
        }

        /// <summary>
        /// Probably-unique id.
        /// </summary>
        /// <returns></returns>
        private static int GenerateTxnId()
        {
            return _Random.Next();
        }
    }
}