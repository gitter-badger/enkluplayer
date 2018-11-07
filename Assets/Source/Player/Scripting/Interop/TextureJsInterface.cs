using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using Jint.Native;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.EnkluPlayer.Scripting
{
    /*
     * TODO
     * - Set TextureWrapMode
     */
    
    [JsInterface("textures")]
    public class TextureJsInterface
    {
        private int _nextId = 1;
        
        private readonly Dictionary<int, Texture2D> _textureLookup = new Dictionary<int, Texture2D>();

        [DenyJsAccess]
        public Texture2D GetTexture(int id)
        {
            Texture2D tex;
            _textureLookup.TryGetValue(id, out tex);
            return tex;
        }

        public int create2D(int width, int height)
        {
            return create2D(width, height, "RGB24", false);
        }
        
        public int create2D(int width, int height, string format, bool mipMap)
        {
            TextureFormat texFormat;
            try
            {
                texFormat = (TextureFormat) Enum.Parse(typeof(TextureFormat), format);
            }
            catch (ArgumentException e)
            {
                Log.Error("Unknown texture format: {0}", format);
                return 0;
            }
            
            var id = _nextId++;
            
            _textureLookup.Add(id, new Texture2D(width, height, texFormat, mipMap){wrapMode = TextureWrapMode.Clamp});

            return id;
        }

        public void destroy(int texId)
        {
            Texture2D tex;
            if (!_textureLookup.TryGetValue(texId, out tex))
            {
                Log.Error(this, "Unknown ID");
                return;
            }
            
            Object.Destroy(tex);
        }

        public void setPixel(int texId, int x, int y, Col4 color)
        {
            Texture2D tex;
            if (!_textureLookup.TryGetValue(texId, out tex))
            {
                Log.Error(this, "Unknown ID");
                return;
            }
            
            tex.SetPixel(x, y, color.ToColor());
        }

        public void setPixels(int texId, Col4[] colors)
        {
            
            Texture2D tex;
            if (!_textureLookup.TryGetValue(texId, out tex))
            {
                Log.Error(this, "Unknown ID");
                return;
            }

            var unityColors = new Color[colors.Length];
            for (int i = 0, len = colors.Length; i < len; i++)
            {
                unityColors[i] = colors[i].ToColor();
            }
            
            tex.SetPixels(unityColors);
        }

        public void apply(int texId)
        {
            apply(texId, true, false);
        }

        public void apply(int texId, bool updateMipmaps, bool makeNoLongerReadable)
        {
            Texture2D tex;
            if (!_textureLookup.TryGetValue(texId, out tex))
            {
                Log.Error(this, "Unknown ID");
                return;
            }
            
            tex.Apply(updateMipmaps, makeNoLongerReadable);
        }
    }
}