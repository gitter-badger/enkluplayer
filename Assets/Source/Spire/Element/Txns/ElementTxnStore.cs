using System.Collections.Generic;
using System.Globalization;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Applies actions, but can roll them back or forget about them later.
    /// </summary>
    public class ElementTxnStore : IElementTxnStore
    {
        private class TxnRecord
        {
            public readonly uint Id;
            public readonly List<ElementActionData> UndoActions = new List<ElementActionData>();

            public TxnRecord(uint id)
            {
                Id = id;
            }
        }

        private const int MAX_TXNS = 1000;

        private readonly IElementActionStrategy _strategy;
        private readonly List<TxnRecord> _records = new List<TxnRecord>();

        public ElementTxnStore(IElementActionStrategy strategy)
        {
            _strategy = strategy;
        }

        public bool Apply(ElementTxn txn, out string error)
        {
            var record = CreateRecord(txn);

            var actions = txn.Actions;
            var undoActions = record.UndoActions;

            for (int i = 0, len = actions.Count; i < len; i++)
            {
                var action = actions[i];
                var undoAction = GenerateUndoAction(action);

                string strategyError;
                if (!_strategy.Apply(action, out strategyError))
                {
                    error = strategyError;

                    // rollback, starting at the previous action
                    Rollback(record, i - 1);

                    return false;
                }

                undoActions.Add(undoAction);
            }

            error = string.Empty;
            return true;
        }

        public void ApplyAndCommit(ElementTxn txn)
        {
            string error;
            for (int i = 0, len = txn.Actions.Count; i < len; i++)
            {
                var action = txn.Actions[i];
                if (!_strategy.Apply(action, out error))
                {
                    Log.Error(this,
                        "ApplyAndCommit: Could not apply action : {1}.",
                        error);

                    // don't finish actions
                    break;
                }
            }
        }

        public void Commit(uint id)
        {
            for (var i = 0; i < _records.Count; i++)
            {
                var record = _records[i];
                if (record.Id == id)
                {
                    _records.RemoveAt(i);

                    break;
                }
            }

            Log.Warning(this,
                "Could not commit {0}. No TxnRecord found.",
                id);
        }

        public void Rollback(uint id)
        {
            var record = RetrieveRecord(id);
            if (null != record)
            {
                Rollback(record, record.UndoActions.Count - 1);
            }
            else
            {
                Log.Warning(this,
                    "Could not rollback {0}. No TxnRecord found.",
                    id);
            }
        }

        private TxnRecord CreateRecord(ElementTxn txn)
        {
            // TODO: Pool.
            var record = new TxnRecord(txn.Id);

            _records.Add(record);

            while (_records.Count > MAX_TXNS)
            {
                _records.RemoveAt(0);
            }

            return record;
        }

        private TxnRecord RetrieveRecord(uint id)
        {
            for (var i = 0; i < _records.Count; i++)
            {
                var record = _records[i];
                if (record.Id == id)
                {
                    return record;
                }
            }

            return null;
        }

        private void Rollback(TxnRecord record, int index)
        {
            var actions = record.UndoActions;
            
            // nothing to rollback
            if (0 == actions.Count)
            {
                return;
            }

            // rollback in reverse order, starting at index
            while (index >= 0)
            {
                var action = actions[index];

                string error;
                if (!_strategy.Apply(action, out error))
                {
                    Log.Warning(this,
                        "Rollback action failed : {0}.",
                        error);
                }

                index--;
            }
        }

        private ElementActionData GenerateUndoAction(ElementActionData action)
        {
            switch (action.Type)
            {
                case ElementActionTypes.CREATE:
                {
                    return new ElementActionData
                    {
                        Type = ElementActionTypes.DELETE,
                        ElementId = action.ElementId
                    };
                }
                case ElementActionTypes.UPDATE:
                {
                    return new ElementActionData
                    {
                        Type = ElementActionTypes.UPDATE,
                        SchemaType = action.SchemaType,
                        Key = action.Key,
                        Value = GetValue(action.ElementId, action.SchemaType, action.Key)
                    };
                }
                default:
                {
                    return null;
                }
            }
        }

        private string GetValue(
            string elementId,
            string schemaType,
            string key)
        {
            Element element;

            var root = _strategy.Element;
            if (elementId == root.Id)
            {
                element = root;
            }
            else
            {
                element = root.FindOne<Element>(".." + elementId);
            }

            if (null == element)
            {
                Log.Warning(this,
                    "Could not get value for update rollback action : Element '{0}' does not exist.",
                    elementId);
                return string.Empty;
            }

            switch (schemaType)
            {
                case ElementActionSchemaTypes.STRING:
                {
                    return element.Schema.Get<string>(key).Value;
                }
                case ElementActionSchemaTypes.INT:
                {
                    return element.Schema.Get<int>(key).Value.ToString();
                }
                case ElementActionSchemaTypes.FLOAT:
                {
                    return element.Schema.Get<float>(key).Value.ToString(CultureInfo.InvariantCulture);
                }
                case ElementActionSchemaTypes.BOOL:
                {
                    return element.Schema.Get<bool>(key).Value.ToString();
                }
                case ElementActionSchemaTypes.VEC3:
                {
                    var val = element.Schema.Get<Vec3>(key).Value;

                    return string.Format(
                        "{{ \"x\": {0}, \"y\": {1}, \"z\": {2} }",
                        val.x,
                        val.y,
                        val.y);
                }
                case ElementActionSchemaTypes.COL4:
                {
                    var val = element.Schema.Get<Col4>(key).Value;

                    return string.Format(
                        "{{ \"r\": {0}, \"g\": {1}, \"b\": {2}, \"a\": {3} }}",
                        val.r,
                        val.g,
                        val.b,
                        val.a);
                }
                default:
                {
                    Log.Warning(this, "Unknown schemaType {0}.", schemaType);
                    return string.Empty;
                }
            }
        }
    }
}