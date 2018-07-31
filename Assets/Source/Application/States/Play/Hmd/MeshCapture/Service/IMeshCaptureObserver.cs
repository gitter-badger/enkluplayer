using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Object that receives mesh updates.
    /// </summary>
    public interface IMeshCaptureObserver
    {
        /// <summary>
        /// Called when there is a mesh update.
        /// </summary>
        /// <param name="id">Id of the mesh.</param>
        /// <param name="filter">Filter.</param>
        void OnData(int id, MeshFilter filter);
    }
}