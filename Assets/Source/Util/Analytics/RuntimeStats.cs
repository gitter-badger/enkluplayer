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
                public WorldAnchorWidget.WorldAnchorStatus Status;
                public float TimeUnlocated;
            }

            public State[] States;
        }

        public class ExperienceInfo
        {
            public string ExperienceId; //
            public string AssetState; //
            public string ScriptState; //
        }
        
        public readonly DeviceInfo Device = new DeviceInfo();
        public readonly CameraInfo Camera = new CameraInfo();
        public readonly AnchorsInfo Anchors = new AnchorsInfo();
        public readonly ExperienceInfo Experience = new ExperienceInfo();
    }
}