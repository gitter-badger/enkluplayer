using System;
using CreateAR.Commons.Unity.Logging;
using RLD;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Put on things you don't want to be selecable.
    /// </summary>
    public class NonSelectableMonoBehaviour : MonoBehaviour, IRTObjectSelectionListener
    {
        /// <inheritdoc />
        public bool OnCanBeSelected(ObjectSelectEventArgs selectEventArgs)
        {
            return false;
        }

        /// <inheritdoc />
        public void OnSelected(ObjectSelectEventArgs selectEventArgs)
        {
            Log.Error(this, "Should not have been able to select {0}.", name);
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void OnDeselected(ObjectDeselectEventArgs deselectEventArgs)
        {
            Log.Error(this, "Should not have been able to deselect {0}.", name);
            throw new NotImplementedException();
        }
    }
}