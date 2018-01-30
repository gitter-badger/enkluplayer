using System;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
{
    public class DesignController
    {
        private PlayModeConfig _playConfig;

        private IUXEventHandler _events;
        private SplashMenuController _splash;
        
        public void Setup()
        {
            _playConfig = Object.FindObjectOfType<PlayModeConfig>();

            if (null == _playConfig)
            {
                throw new Exception("Could not find PlayModeConfig.");
            }

            _events = Object.Instantiate(_playConfig.Root);

            _splash = Object.Instantiate(_playConfig.SplashMenu, _events.transform);
            _splash.Initialize(_events);
        }

        public void Teardown()
        {
            Object.Destroy(_splash);
            Object.Destroy(_events);
        }
    }
}