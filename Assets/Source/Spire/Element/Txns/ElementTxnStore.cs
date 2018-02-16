﻿using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    public class ElementActionUpdateRecord
    {
        public Element Element;
        public string SchemaType;
        
        public string Key;
        
        public object PrevValue;
        public object NextValue;
    }

    /// <summary>
    /// Applies actions, but can roll them back or forget about them later.
    /// </summary>
    public class ElementTxnStore : IElementTxnStore
    {
        private class TxnRecord
        {
            public readonly List<ElementActionUpdateRecord> UpdateRecords = new List<ElementActionUpdateRecord>();

            public ElementTxn Txn;

            public uint Id;

            public bool AllowsPreCommit;
        }

        private const int MAX_TXNS = 1000;

        private readonly ElementActionStrategy _strategy;
        private readonly List<TxnRecord> _records = new List<TxnRecord>();
        private readonly ElementActionUpdateRecord _scratchRecord = new ElementActionUpdateRecord();

        public ElementTxnStore(ElementActionStrategy strategy)
        {
            _strategy = strategy;
        }

        public bool Request(ElementTxn txn, out string error)
        {
            var record = SetupRecord(txn);

            // this txn supports precommiting + potentially rolling back later
            if (record.AllowsPreCommit)
            {
                return PreCommit(record, out error);
            }
            
            error = string.Empty;
            return true;
        }
        
        public void Apply(ElementTxn txn)
        {
            for (int i = 0, len = txn.Actions.Count; i < len; i++)
            {
                var action = txn.Actions[i];

                string error;
                switch (action.Type)
                {
                    case ElementActionTypes.CREATE:
                    {
                        if (!_strategy.ApplyCreateAction(action, out error))
                        {
                            Log.Error(this,
                                "ApplyAndCommit: Could not apply create action : {1}.",
                                error);

                            // don't finish actions
                            return;
                        }

                        break;
                    }
                    case ElementActionTypes.DELETE:
                    {
                        if (!_strategy.ApplyDeleteAction(action, out error))
                        {
                            Log.Error(this,
                                "ApplyAndCommit: Could not apply delete action : {0}.",
                                error);

                            // don't finish actions
                            return;
                        }

                        break;
                    }
                    case ElementActionTypes.UPDATE:
                    {
                        if (ApplyActionToUpdateRecord(action, _scratchRecord))
                        {
                            if (!_strategy.ApplyUpdateAction(
                                _scratchRecord,
                                out error))
                            {
                                // don't finish
                                Log.Error(this,
                                    "Apply: Could not apply update action: {0}.",
                                    error);

                                return;
                            }
                        }
                        else
                        {
                            Log.Error(this, "Apply: Could not apply update action: ApplyActionToUpdate failed.");

                            // don't finish
                            return;
                        }
                        
                        break;
                    }
                    default:
                    {
                        Log.Error(this,
                            "Invalid action type '{0}'.",
                            action.Type);
                        return;
                    }
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
                    // was not precommitted, need to Apply() now
                    if (!record.AllowsPreCommit)
                    {
                        Apply(record.Txn);
                    }

                    _records.RemoveAt(i);

                    return;
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
                Rollback(record, record.UpdateRecords.Count - 1);
            }
            else
            {
                Log.Warning(this,
                    "Could not rollback {0}. No TxnRecord found.",
                    id);
            }
        }

        private bool ApplyActionToUpdateRecord(
            ElementActionData action,
            ElementActionUpdateRecord record)
        {
            var element = _strategy.Element;
            if (element.Id != action.ElementId)
            {
                element = element.FindOne<Element>(".." + action.ElementId);

                if (null == element)
                {
                    return false;
                }
            }

            record.Element = element;
            record.SchemaType = action.SchemaType;
            record.Key = action.Key;

            switch (record.SchemaType)
            {
                case ElementActionSchemaTypes.STRING:
                {
                    record.NextValue = action.Value;
                    break;
                }
                case ElementActionSchemaTypes.INT:
                {
                    int val;
                    if (int.TryParse(action.Value, out val))
                    {
                        record.NextValue = val;

                        break;
                    }

                    return false;
                }
                case ElementActionSchemaTypes.FLOAT:
                {
                    float val;
                    if (float.TryParse(action.Value, out val))
                    {
                        record.NextValue = val;

                        break;
                    }

                    return false;
                }
                case ElementActionSchemaTypes.BOOL:
                {
                    bool val;
                    if (bool.TryParse(action.Value, out val))
                    {
                        record.NextValue = val;

                        break;
                    }

                    return false;
                }
                case ElementActionSchemaTypes.VEC3:
                {
                    var vals = action.Value.Split(',');
                    if (3 != vals.Length)
                    {
                        return false;
                    }

                    float x, y, z;
                    if (!float.TryParse(vals[0], out x)
                        || !float.TryParse(vals[1], out y)
                        || !float.TryParse(vals[2], out z))
                    {
                        return false;
                    }

                    record.NextValue = new Vec3(x, y, z);
                    break;
                }
                case ElementActionSchemaTypes.COL4:
                {
                    var vals = action.Value.Split(',');
                    if (4 != vals.Length)
                    {
                        return false;
                    }

                    float r, g, b, a;
                    if (!float.TryParse(vals[0], out r)
                        || !float.TryParse(vals[1], out g)
                        || !float.TryParse(vals[2], out b)
                        || !float.TryParse(vals[3], out a))
                    {
                        return false;
                    }

                    record.NextValue = new Col4(r, g, b, a);

                    break;
                }
                default:
                {
                    return false;
                }
            }

            return true;
        }

        private bool PreCommit(TxnRecord record, out string error)
        {
            var precommits = record.UpdateRecords;
            for (int i = 0, len = precommits.Count; i < len; i++)
            {
                var precommit = precommits[i];

                string strategyError;
                if (!_strategy.ApplyUpdateAction(
                    precommit,
                    out strategyError))
                {
                    error = strategyError;

                    // rollback, starting at the previous action
                    Rollback(record, i - 1);
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }

        private bool AllowsPreCommit(ElementTxn txn)
        {
            for (int i = 0, len = txn.Actions.Count; i < len; i++)
            {
                if (txn.Actions[i].Type != ElementActionTypes.UPDATE)
                {
                    return false;
                }
            }

            return true;
        }

        private TxnRecord SetupRecord(ElementTxn txn)
        {
            // TODO: Pool.
            var record = new TxnRecord();
            record.Id = txn.Id;
            record.Txn = txn;
            record.AllowsPreCommit = AllowsPreCommit(txn);

            if (record.AllowsPreCommit)
            {
                for (var i = 0; i < txn.Actions.Count; i++)
                {
                    var action = txn.Actions[i];

                    // TODO: Pool.
                    var updateRec = new ElementActionUpdateRecord();

                    ApplyActionToUpdateRecord(action, updateRec);   

                    record.UpdateRecords.Add(updateRec);
                }
            }

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
            if (!_records.Remove(record))
            {
                Log.Error(this, "Cannot rollback untracked Txn!");
                return;
            }

            var precommits = record.UpdateRecords;
            
            // nothing to rollback
            if (0 == precommits.Count)
            {
                return;
            }

            // rollback in reverse order, starting at index
            while (index >= 0)
            {
                var precommit = precommits[index];
                precommit.NextValue = precommit.PrevValue;

                string error;
                if (!_strategy.ApplyUpdateAction(precommit, out error))
                {
                    Log.Warning(this,
                        "Rollback action failed : {0}.",
                        error);
                }

                index--;
            }
        }
    }
}