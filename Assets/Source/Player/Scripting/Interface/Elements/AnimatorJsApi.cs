using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Exposes a simple interface to Unity's Animator. JS Scripts can get/set
    /// parameters as needed to trigger different animation states. The current
    /// animation clip's name is available for querying, to check what
    /// animation is currently active.
    /// </summary>
    public class AnimatorJsApi
    {
        /// <summary>
        /// Underlying Unity Animator to wrap.
        /// </summary>
        private readonly Animator _animator;

        /// <summary>
        /// List of available parameter names.
        /// </summary>
        private readonly string[] _parameterNames;

        /// <summary>
        /// List of available parameter names.
        /// </summary>
        public string[] parameterNames
        {
            get { return _parameterNames; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="animator"></param>
        public AnimatorJsApi(Animator animator)
        {
            _animator = animator;

            _parameterNames = new string[_animator.parameterCount];
            for (int i = 0; i < _animator.parameters.Length; i++)
            {
                _parameterNames[i] = _animator.parameters[i].name;
            }
        }

        /// <summary>
        /// Gets the current value for a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool getBool(string name)
        {
            return _animator.GetBool(name);
        }

        /// <summary>
        /// Sets the value for a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void setBool(string name, bool value)
        {
            _animator.SetBool(name, value);
        }

        /// <summary>
        /// Gets the current value for a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int getInteger(string name)
        {
            return _animator.GetInteger(name);
        }

        /// <summary>
        /// Sets the value for a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void setInteger(string name, int value)
        {
            _animator.SetInteger(name, value);
        }

        /// <summary>
        /// Gets the current value for a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public float getFloat(string name)
        {
            return _animator.GetFloat(name);
        }

        /// <summary>
        /// Sets the value for a parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void setFloat(string name, float value)
        {
            _animator.SetFloat(name, value);
        }

        /// <summary>
        /// Returns the <see cref="AnimationClip"/> name for the playing animation with the highest weight.
        /// </summary>
        /// <param name="layer">Optional - layer to check on.</param>
        /// <returns></returns>
        public string getCurrentClipName(int layer = 0)
        {
            return _animator.GetCurrentAnimatorClipInfo(layer)[0].clip.name;
        }

        /// <summary>
        /// Returns true if the <c>clipName</c> is playing with any non-zero weight.
        /// </summary>
        /// <param name="clipName">Clip to test.</param>
        /// <param name="layer">Optional - layer to check on.</param>
        /// <returns></returns>
        public bool isClipPlaying(string clipName, int layer = 0)
        {
            var clips = _animator.GetCurrentAnimatorClipInfo(layer);

            for (var i = 0; i < clips.Length; i++)
            {
                if (clips[i].clip.name == clipName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}