using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    public class RuntimeStats
    {
        public class DeviceInfo
        {
            public float Battery;

            public float ReservedMemory;
            public float AllocatedMemory;
            public float MonoMemory;
            public float GpuMemory;
            public float GraphicsDriverMemory;

            public float AvailableMemory; //
        }

        public class CameraInfo
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public string AnchorRelativeTo;
        }

        public class AnchorsInfo
        {
            public struct State
            {
                public string Id;
                public WorldAnchorWidget.WorldAnchorStatus Status;
                public float TimeUnlocated;
            }

            public State[] States = new State[0];
        }

        public class ExperienceInfo
        {
            public struct LoaderInfo
            {
                public int QueueLength;
                public string Errors;
            }
            
            public string ExperienceId; //
            public LoaderInfo AssetState = new LoaderInfo();
            public LoaderInfo ScriptState = new LoaderInfo();
        }
        
        public readonly DeviceInfo Device = new DeviceInfo();
        public readonly CameraInfo Camera = new CameraInfo();
        public readonly AnchorsInfo Anchors = new AnchorsInfo();
        public readonly ExperienceInfo Experience = new ExperienceInfo();
        
        public float Uptime;
    }
}