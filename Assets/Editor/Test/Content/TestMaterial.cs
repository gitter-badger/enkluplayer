using System.Collections;
using System.Collections.Generic;
using CreateAR.EnkluPlayer.Scripting;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class TestMaterial : IMaterial
    {
        private Dictionary<string, float> _floats = new Dictionary<string, float>();
        private Dictionary<string, int> _ints = new Dictionary<string, int>();
        private Dictionary<string, Vec3> _v3s = new Dictionary<string, Vec3>();
        private Dictionary<string, Col4> _c4s = new Dictionary<string, Col4>();
        
        public float GetFloat(string param)
        {
            return _floats[param];
        }
        
        public void SetFloat(string param, float value)
        {
            _floats[param] = value;
        }

        public int GetInt(string param)
        {
            return _ints[param];
        }
        
        public void SetInt(string param, int value)
        {
            _ints[param] = value;
        }

        public Vec3 GetVec3(string param)
        {
            return _v3s[param];
        }

        public void SetVec3(string param, Vec3 value)
        {
            _v3s[param] = value;
        }
        
        public Col4 GetCol4(string param)
        {
            return _c4s[param];
        }

        public void SetCol4(string param, Col4 value)
        {
            _c4s[param] = value;
        }
    }
}