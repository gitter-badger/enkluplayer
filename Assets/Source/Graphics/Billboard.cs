using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public enum BillboardType
    {
        Horizontal, // faces the camera without pitching forward or backward
        Absolute,   // takes no rotation (effectively nulls parent's rotation)
        Camera      // faces the camera
    }

    /// <summary>
    /// Dynamically orients a gameobect,
    /// </summary>
    [ExecuteInEditMode]
    public class Billboard : MonoBehaviour
    {
        /// <summary>
        /// Defines the calculations used to orient the gameobject
        /// </summary>
        public BillboardType Type = BillboardType.Camera;

        /// <summary>
        /// Frame based update,
        /// </summary>
        public void Update()
        {
            if (Type == BillboardType.Absolute)
            {
                transform.rotation = Quaternion.identity;
                return;
            }

            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            var up 
                = Type == BillboardType.Horizontal
                ? Vector3.up
                : mainCamera.transform.up;
            
            var forward
                = transform.position
                - mainCamera.transform.position;
            forward.y = 0;
            forward.Normalize();
            transform.LookAt(
                transform.position + forward,
                up);

            var localEuler = transform.localEulerAngles;

            if (Type == BillboardType.Horizontal)
            {
                localEuler.z = 0;
            }

            transform.localEulerAngles = localEuler;
        }
    }
}
