using System;
using RLD;
using UnityEngine;

namespace CreateAR.SpirePlayer
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
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void OnDeselected(ObjectDeselectEventArgs deselectEventArgs)
        {
            throw new NotImplementedException();
        }
    }
}