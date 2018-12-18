using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// IAnimator implementation for Unity.
    /// </summary>
    public class UnityAnimator : IAnimator
    {
        /// <summary>
        /// Backing Unity Animator.
        /// </summary>
        private Animator _animator;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="animator"></param>
        public UnityAnimator(Animator animator)
        {
            _animator = animator;
        }

        /// <inheritdoc />
        public AnimatorControllerParameter[] Parameters
        {
            get { return _animator.parameters; }
        }

        /// <inheritdoc />
        public bool GetBool(string name)
        {
            return _animator.GetBool(name);
        }

        /// <inheritdoc />
        public void SetBool(string name, bool value)
        {
            _animator.SetBool(name, value);
        }

        /// <inheritdoc />
        public int GetInt(string name)
        {
            return _animator.GetInteger(name);
        }

        /// <inheritdoc />
        public void SetInt(string name, int value)
        {
            _animator.SetInteger(name, value);
        }

        /// <inheritdoc />
        public float GetFloat(string name)
        {
            return _animator.GetFloat(name);
        }

        /// <inheritdoc />
        public void SetFloat(string name, float value)
        {
            _animator.SetFloat(name, value);
        }

        /// <inheritdoc />
        public string CurrentClipName(int layer = 0)
        {
            return _animator.GetCurrentAnimatorClipInfo(layer)[0].clip.name;
        }

        /// <inheritdoc />
        public bool IsClipPlaying(string clipName, int layer = 0)
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