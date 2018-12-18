using CreateAR.EnkluPlayer.Scripting;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class TestAudioSource : IAudioSource
    {
        public float Volume { get; set; }
        public bool Loop { get; set; }
        public bool Mute { get; set; }
        public bool PlayOnAwake { get; set; }
        public float SpatialBlend { get; set; }
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }
        public float DopplerLevel { get; set; }
    }
}