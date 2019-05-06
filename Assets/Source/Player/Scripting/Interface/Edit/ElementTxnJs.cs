using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;
using Enklu.Orchid;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Js wrapper for ElementTxn.
    /// </summary>
    public class ElementTxnJs
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IElementManager _elements;

        /// <summary>
        /// The underlying txn.
        /// </summary>
        [DenyJsAccess]
        public ElementTxn Txn { get; private set; }

        /// <summary>
        /// Constructor.
        /// <summary>
        public ElementTxnJs(
            IElementManager elements,
            ElementTxn txn)
        {
            _elements = elements;

            Txn = txn;
        }

        public ElementTxnJs update(string elementId, string schemaType, string key, object value)
        {
            switch (schemaType)
            {
                case ElementActionSchemaTypes.STRING:
                {
                    Txn.Update(elementId, key, (string) value);
                    break;
                }
                case ElementActionSchemaTypes.INT:
                {
                    Txn.Update(elementId, key, (int) value);
                    break;
                }
                case ElementActionSchemaTypes.FLOAT:
                {
                    Txn.Update(elementId, key, (float) value);
                    break;
                }
                case ElementActionSchemaTypes.BOOL:
                {
                    Txn.Update(elementId, key, (bool) value);
                    break;
                }
                case ElementActionSchemaTypes.COL4:
                {
                    Txn.Update(elementId, key, (Col4) value);
                    break;
                }
                case ElementActionSchemaTypes.VEC3:
                {
                    Txn.Update(elementId, key, (Vec3) value);
                    break;
                }
            }

            return this;
        }

        public ElementTxnJs duplicate(string parentId, string templateElementId, string elementId)
        {
            var parent = _elements.ById(parentId);
            if (null == parent)
            {
                Log.Warning(this, "Could not find parent.");
                return this;
            }

            var template = _elements.ById(templateElementId);
            if (null == template)
            {
                Log.Warning(this, "Could not find template element.");
                return this;
            }

            var data = template.ToElementData();
            data.Id = elementId;
            data.Schema.Strings["id"] = elementId;
            Txn.Create(parentId, data);

            return this;
        }

        public ElementTxnJs create(string parentId, int type, string elementId)
        {
            Txn.Create(parentId, new ElementData
            {
                Id = elementId,
                Type = type
            });

            return this;
        }

        public ElementTxnJs delete(string elementId)
        {
            Txn.Delete(elementId);

            return this;
        }
    }
}