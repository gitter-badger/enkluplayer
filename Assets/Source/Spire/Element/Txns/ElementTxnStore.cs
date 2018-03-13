using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Applies actions, but can roll them back or forget about them later.
    /// </summary>
    public class ElementTxnStore : IElementTxnStore
    {
        /// <summary>
        /// Tracks a transaction.
        /// </summary>
        private class TxnRecord
        {
            /// <summary>
            /// List of records to we can rollback updates.
            /// </summary>
            public readonly List<ElementActionUpdateRecord> UpdateRecords = new List<ElementActionUpdateRecord>();

            /// <summary>
            /// The transaction.
            /// </summary>
            public ElementTxn Txn;

            /// <summary>
            /// True iff precommits are allowed for this transaction.
            /// </summary>
            public bool AllowsPreCommit;

            /// <summary>
            /// Returns id of transaction.
            /// </summary>
            public int Id
            {
                get { return Txn.Id; }
            }
        }

        /// <summary>
        /// Max number of transactions we're allowed to track.
        /// </summary>
        private const int MAX_TXNS = 1000;

        /// <summary>
        /// The strategy to affect elements with.
        /// </summary>
        private readonly IElementActionStrategy _strategy;

        /// <summary>
        /// List of transactions we're currently tracking.
        /// </summary>
        private readonly List<TxnRecord> _records = new List<TxnRecord>();

        /// <summary>
        /// Used over and over again.
        /// </summary>
        private readonly ElementActionUpdateRecord _scratchRecord = new ElementActionUpdateRecord();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementTxnStore(IElementActionStrategy strategy)
        {
            _strategy = strategy;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
                    case ElementActionTypes.MOVE:
                    {
                        if (!_strategy.ApplyMoveAction(action, out error))
                        {
                            Log.Error(this,
                                "ApplyAndCommit: Could not apply delete action : {0}.",
                                error);

                            // don't finish actions
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

        /// <inheritdoc />
        public void Commit(int id)
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

        /// <inheritdoc />
        public void Rollback(int id)
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

        /// <summary>
        /// Applies information from an action to an update record.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="record">The update record.</param>
        /// <returns></returns>
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
                    record.NextValue = (int) action.Value;
                    break;
                }
                case ElementActionSchemaTypes.FLOAT:
                {
                    record.NextValue = (float) action.Value;
                    break;
                }
                case ElementActionSchemaTypes.BOOL:
                {
                    record.NextValue = (bool) action.Value;
                    break;
                }
                case ElementActionSchemaTypes.VEC3:
                {
                    record.NextValue = (Vec3) action.Value;
                    break;
                }
                case ElementActionSchemaTypes.COL4:
                {
                    record.NextValue = (Col4) action.Value;

                    break;
                }
                default:
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Applies actions before commit.
        /// </summary>
        /// <param name="record">The record to apply.</param>
        /// <param name="error">The error, if any.</param>
        /// <returns></returns>
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

        /// <summary>
        /// True iff a transaction allows a precommit.
        /// </summary>
        /// <param name="txn">The transaction.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Sets up a record based on a transaction.
        /// </summary>
        /// <param name="txn">The transaction.</param>
        /// <returns></returns>
        private TxnRecord SetupRecord(ElementTxn txn)
        {
            // TODO: Pool.
            var record = new TxnRecord();
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
        
        /// <summary>
        /// Retrieves an existing transaction record by id.
        /// </summary>
        /// <param name="id">The is of the record.</param>
        /// <returns></returns>
        private TxnRecord RetrieveRecord(int id)
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

        /// <summary>
        /// Rolls back a transaction and removes it from tracking.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="index">The index into the record.</param>
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