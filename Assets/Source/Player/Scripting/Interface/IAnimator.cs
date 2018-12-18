using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Interface for an Animator.
    /// </summary>
    public interface IAnimator
    {
        /// <summary>
        /// Parameters for this animator.
        /// </summary>
        AnimatorControllerParameter[] Parameters { get; }

        /// <summary>
        /// Gets a bool parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool GetBool(string name);
        
        /// <summary>
        /// Sets a bool parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetBool(string name, bool value);

        /// <summary>
        /// Gets an int parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        int GetInt(string name);
        
        /// <summary>
        /// Sets an int parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetInt(string name, int value);

        /// <summary>
        /// Gets a float parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        float GetFloat(string name);
        
        /// <summary>
        /// Sets a float parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetFloat(string name, float value);

        /// <summary>
        /// Gets the current playing clip name.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        string CurrentClipName(int layer = 0);
        
        /// <summary>
        /// Returns true if clipName is currently playing.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        bool IsClipPlaying(string clipName, int layer = 0);
    }
}