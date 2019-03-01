using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Manages getting shared Unity Material instances for elements.
    /// </summary>
    public class MaterialManager : IMaterialManager
    {
        /// <summary>
        /// Material lookup.
        /// </summary>
        private readonly Dictionary<string, Material> _materials = new Dictionary<String,Material>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public MaterialManager(WidgetConfig config)
        {
            var textType = typeof(TextPrimitive);
            var activatorType = typeof(ActivatorPrimitive);
            
            // Register materials from WidgetConfig.
            RegisterMaterial(textType, config.TextOverlay);
            RegisterMaterial(textType, config.TextOccluded);
            RegisterMaterial(textType, config.TextHidden);
            RegisterMaterial(activatorType, config.ButtonOverlay);
            RegisterMaterial(activatorType, config.ButtonOccluded);
        }

        
        /// <inheritdoc />
        public Material Material(Element element, string materialStr)
        {
            var key = CreateKey(element.GetType(), materialStr);

            Material material;
            _materials.TryGetValue(key, out material);

            return material;
        }

        /// <inheritdoc />
        public void RegisterMaterial(Type type, Material material)
        {
            var matName = material.name;
            var key = CreateKey(type, matName);

            if (_materials.ContainsKey(key))
            {
                throw new Exception(string.Format("Material already registered for type ({0} : {1})", type, matName));
            }
            _materials[key] = material;
        }

        /// <summary>
        /// Creates a key for _materials based on type/name.
        /// </summary>
        /// <param name="type">Element type.</param>
        /// <param name="materialStr">Material name.</param>
        /// <returns></returns>
        private static string CreateKey(Type type, string materialStr)
        {
            return string.Format("{0}:{1}", type.FullName, materialStr);
        }
    }
}