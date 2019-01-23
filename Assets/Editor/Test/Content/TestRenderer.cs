using CreateAR.EnkluPlayer.Scripting;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class TestRenderer : IRenderer
    {
        public IMaterial SharedMaterial { get; set; }
        public IMaterial Material { get; set; }

        public TestRenderer(TestMaterial material)
        {
            SharedMaterial = material;
        }
    }
}