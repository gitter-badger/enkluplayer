using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class DesignApplicationState : IState
    {
        private readonly IMessageRouter _messages;
        private readonly IElementFactory _elements;
        private readonly WidgetConfig _config;
        private readonly VineImporter _vine;

        private Element _menu;

        public DesignApplicationState(
            IMessageRouter messages,
            IVinePreProcessor preprocessor,
            IElementFactory elements,
            WidgetConfig config)
        {
            _messages = messages;
            _elements = elements;
            _config = config;
            _vine = new VineImporter(preprocessor);
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            var designAsset = _config.DesignMenu;
            if (!designAsset)
            {
                Log.Error(this, "Could not find Design Menu Vine!");

                _messages.Publish(MessageTypes.DEFAULT_STATE);
                return;
            }

            _menu = _elements.Element(_vine.Parse(designAsset.text));
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            _menu.Destroy();
            _menu = null;
        }
    }
}
