﻿using CreateAR.EnkluPlayer.IUX;
using Enklu.Data;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Describes an object that applies actions to elements.
    /// </summary>
    public interface IElementActionStrategy
    {
        /// <summary>
        /// Root element.
        /// </summary>
        Element Element { get; }

        /// <summary>
        /// Applies a create action.
        /// </summary>
        /// <param name="action">The action to apply.</param>
        /// <param name="error">The error, if any.</param>
        bool ApplyCreateAction(
            ElementActionData action,
            out string error);

        /// <summary>
        /// Applies a delete action.
        /// </summary>
        /// <param name="action">The action to apply.</param>
        /// <param name="error">The error, if any.</param>
        bool ApplyDeleteAction(
            ElementActionData action,
            out string error);

        /// <summary>
        /// Directly applies an update without storing previous state.
        /// </summary>
        /// <param name="action">The action to apply.</param>
        /// <param name="error">The error, if any.</param>
        bool ApplyUpdateAction(
            ElementActionData action,
            out string error);

        /// <summary>
        /// Applies an update record.
        /// </summary>
        /// <param name="record">Record that contains new state and allows storing prev state.</param>
        /// <param name="error">The error, if any.</param>
        bool ApplyUpdateAction(
            ElementActionUpdateRecord record,
            out string error);

        /// <summary>
        /// Applies a move action.
        /// </summary>
        /// <param name="action">The action to apply.</param>
        /// <param name="error">The error, if any.</param>
        bool ApplyMoveAction(
            ElementActionData action,
            out string error);
    }
}