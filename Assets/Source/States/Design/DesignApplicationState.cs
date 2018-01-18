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
        private readonly VineImporter _vine;
        private readonly IElementFactory _elements;

        private Element _menu;

        public DesignApplicationState(
            IMessageRouter messages,
            IVinePreProcessor preprocessor,
            IElementFactory elements)
        {
            _messages = messages;
            _vine = new VineImporter(preprocessor);
            _elements = elements;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            var designAsset = Resources.Load<TextAsset>("Vines/Design.Menu");
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
