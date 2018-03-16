using CreateAR.Commons.Unity.Async;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class ScreenGrabber : MonoBehaviour
    {
        private AsyncToken<Texture2D> _grabToken;
        private Texture2D _texture;
        
        public IAsyncToken<Texture2D> Grab()
        {
            _grabToken = new AsyncToken<Texture2D>();
            return _grabToken;
        }

        private void OnPostRender()
        {
            if (null != _grabToken)
            {
                if (null == _texture)
                {
                    _texture = new Texture2D(Screen.width, Screen.height);
                }
                
                _texture.ReadPixels(
                    new Rect(0, 0, Screen.width, Screen.height),
                    0, 0, false);
                _texture.Apply(false);
                
                _grabToken.Succeed(_texture);
                _grabToken = null;
            }
        }
    }
}