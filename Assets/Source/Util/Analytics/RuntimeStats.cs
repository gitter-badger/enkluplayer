using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// All of the stats that the runtime will update.
    /// Some fields update every frame or as needed.
    /// </summary>
    public class RuntimeStats
    {
        /// <summary>
        /// Device specific information.
        /// </summary>
        public class DeviceInfo
        {
            /// <summary>
            /// Current battery level.
            /// </summary>
            public float Battery;

            /// <summary>
            /// Reserved memory;
            /// </summary>
            public float ReservedMemory;
            
            /// <summary>
            /// Allocated memory.
            /// </summary>
            public float AllocatedMemory;
            
            /// <summary>
            /// Mono memory;
            /// </summary>
            public float MonoMemory;
            
            /// <summary>
            /// GPU memory.
            /// </summary>
            public float GpuMemory;
            
            /// <summary>
            /// Graphics driver memory;
            /// </summary>
            public float GraphicsDriverMemory;

            /// <summary>
            /// Available memory.
            /// </summary>
            public float AvailableMemory; // TODO: Grab from device portal
        }

        /// <summary>
        /// Camera related information.
        /// </summary>
        public class CameraInfo
        {
            /// <summary>
            /// Current position.
            /// </summary>
            public Vector3 Position;
            
            /// <summary>
            /// Current rotation.
            /// </summary>
            public Quaternion Rotation;
            
            /// <summary>
            /// Anchor ID position/rotation are relative to.
            /// </summary>
            public string AnchorRelativeTo;
        }

        /// <summary>
        /// Anchor related information.
        /// </summary>
        public class AnchorsInfo
        {
            /// <summary>
            /// The info we want to track per anchor.
            /// </summary>
            public struct State
            {
                /// <summary>
                /// The anchor's Id.
                /// </summary>
                public string Id;
                
                /// <summary>
                /// The anchor's status.
                /// </summary>
                public WorldAnchorStatus Status;
                
                /// <summary>
                /// The time this anchor has been unlocated.
                /// </summary>
                public float TimeUnlocated;
            }

            /// <summary>
            /// States for all of the anchors.
            /// </summary>
            public State[] States = new State[0];
        }

        /// <summary>
        /// Experience related information.
        /// </summary>
        public class ExperienceInfo
        {
            /// <summary>
            /// Loader specific information.
            /// </summary>
            public struct LoaderInfo
            {
                public int QueueLength;
                public string NextLoad;
                public string Errors;
            }
            
            /// <summary>
            /// The current experience id
            /// </summary>
            public string ExperienceId;
            
            /// <summary>
            /// AssetImporter state.
            /// </summary>
            public LoaderInfo AssetState = new LoaderInfo();
            
            /// <summary>
            /// ScriptImporter state.
            /// </summary>
            public LoaderInfo ScriptState = new LoaderInfo();
        }
        
        /// <summary>
        /// Device info.
        /// </summary>
        public readonly DeviceInfo Device = new DeviceInfo();
        
        /// <summary>
        /// CameraInfo info.
        /// </summary>
        public readonly CameraInfo Camera = new CameraInfo();
        
        /// <summary>
        /// AnchorsInfo.
        /// </summary>
        public readonly AnchorsInfo Anchors = new AnchorsInfo();
        
        /// <summary>
        /// ExperienceInfo.
        /// </summary>
        public readonly ExperienceInfo Experience = new ExperienceInfo();
        
        /// <summary>
        /// The Time.realTimeSinceStartup for the runtime.
        /// </summary>
        public float Uptime;
    }
}