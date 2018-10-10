using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    public class MaterialJsApi
    {
        private readonly Renderer _renderer;

        public MaterialJsApi(Renderer renderer)
        {
            _renderer = renderer;
        }

        public void setFloat(string param, float value)
        {
            _renderer.sharedMaterial.SetFloat(param, value);
        }

        public void setInt(string param, int value)
        {
            _renderer.sharedMaterial.SetInt(param, value);
        }

        public void setVector(string param, Vec3 value)
        {
            _renderer.sharedMaterial.SetVector(param, value.ToVector());
        }

        public void setShaderPass(string pass, bool enabled)
        {
            _renderer.sharedMaterial.SetShaderPassEnabled(pass, enabled);
        }
    }
}