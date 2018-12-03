using System.Collections.Generic;
using CreateAR.EnkluPlayer.Scripting;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Test.Scripting
{
    public class TestAnimator : IAnimator
    {
        private Dictionary<string, bool> _bools = new Dictionary<string, bool>();
        private Dictionary<string, float> _floats = new Dictionary<string, float>();
        private Dictionary<string, int> _ints = new Dictionary<string, int>();
        private AnimatorControllerParameter[] _parameters;

        public TestAnimator(AnimatorControllerParameter[] parameters)
        {
            _parameters = parameters;
        }
        
        public AnimatorControllerParameter[] Parameters
        {
            get { return _parameters; }
        }
        
        public bool GetBool(string name)
        {
            return _bools[name];
        }

        public void SetBool(string name, bool value)
        {
            _bools[name] = value;
        }

        public int GetInt(string name)
        {
            return _ints[name];
        }

        public void SetInt(string name, int value)
        {
            _ints[name] = value;
        }

        public float GetFloat(string name)
        {
            return _floats[name];
        }

        public void SetFloat(string name, float value)
        {
            _floats[name] = value;
        }

        public string CurrentClipName(int layer = 0)
        {
            throw new System.NotImplementedException();
        }

        public bool IsClipPlaying(string name, int layer = 0)
        {
            throw new System.NotImplementedException();
        }
    }
}