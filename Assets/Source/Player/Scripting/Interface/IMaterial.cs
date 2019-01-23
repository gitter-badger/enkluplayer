namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// A Material.
    /// </summary>
    public interface IMaterial
    {
        /// <summary>
        /// Get a float by name.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        float GetFloat(string param);
        
        /// <summary>
        /// Set a float by name.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        void SetFloat(string param, float value);
        
        /// <summary>
        /// Get an int by name.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        int GetInt(string param);
        
        /// <summary>
        /// Set an int by name.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        void SetInt(string param, int value);

        /// <summary>
        /// Get a Vec3 by name.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        Vec3 GetVec3(string param);
        
        /// <summary>
        /// Set a Vec3 by name.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        void SetVec3(string param, Vec3 value);

        /// <summary>
        /// Get a Col4 by name.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        Col4 GetCol4(string param);
        
        /// <summary>
        /// Set a Col4 by name.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        void SetCol4(string param, Col4 value);

        /// <summary>
        /// Cleanup.
        /// </summary>
        void Teardown();
    }
}