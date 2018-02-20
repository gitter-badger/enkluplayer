using System.Text;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    public class SceneUpdateService : ApplicationService
    {
        private readonly JsonSerializer _serializer = new JsonSerializer();

        private readonly IElementTxnManager _txns;

        public SceneUpdateService(
            MessageTypeBinder binder,
            IMessageRouter messages,
            IElementTxnManager txns)
            : base(binder, messages)
        {
            _txns = txns;
        }

        public override void Start()
        {
            base.Start();

            Subscribe<SceneUpdateEvent>(
                MessageTypes.SCENE_UPDATE,
                message =>
                {
                    Log.Info(this, "Received scene update event.");

                    // all values come in as strings
                    PrepActions(message);

                    // APPLY
                    _txns.Apply(new ElementTxn(message.Scene, message.Actions));
                });
        }

        private void PrepActions(SceneUpdateEvent message)
        {
            foreach (var action in message.Actions)
            {
                var valueString = action.Value.ToString();

                switch (action.SchemaType)
                {
                    case ElementActionSchemaTypes.INT:
                    {
                        action.Value = int.Parse(valueString);
                        break;
                    }
                    case ElementActionSchemaTypes.FLOAT:
                    {
                        action.Value = float.Parse(valueString);
                        break;
                    }
                    case ElementActionSchemaTypes.BOOL:
                    {
                        action.Value = bool.Parse(valueString);
                        break;
                    }
                    case ElementActionSchemaTypes.VEC3:
                    {
                        action.Value = Parse<Vec3>(valueString);
                        break;
                    }
                    case ElementActionSchemaTypes.COL4:
                    {
                        action.Value = Parse<Col4>(valueString);
                        break;
                    }
                }
            }
        }

        private T Parse<T>(string value)
        {
            object instance;
            var bytes = Encoding.UTF8.GetBytes(value);
            _serializer.Deserialize(typeof(T), ref bytes, out instance);

            return (T) instance;
        }
    }
}