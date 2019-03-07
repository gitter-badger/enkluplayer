using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Manages Material instances based on Element types.
    /// </summary>
    public interface IMaterialManager
    {
        /// <summary>
        /// Gets a shared Material instance based on primitive type and a material name.
        /// Returns null if there is no type/material registered.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="materialStr"></param>
        /// <returns></returns>
        Material Material(Element element, string materialStr);

        /// <summary>
        /// Registered a Material for a given Type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="material"></param>
        void RegisterMaterial(Type type, Material material);
    }
}