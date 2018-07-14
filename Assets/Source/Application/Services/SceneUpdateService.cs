using System.Text;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Handles scene updates.
    /// </summary>
    public class SceneUpdateService : ApplicationService
    {
        /// <summary>
        /// Serializer.
        /// </summary>
        private readonly JsonSerializer _serializer = new JsonSerializer();

        /// <summary>
        /// Txns.
        /// </summary>
        private readonly IElementTxnManager _txns;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SceneUpdateService(
            MessageTypeBinder binder,
            IMessageRouter messages,
            IElementTxnManager txns)
            : base(binder, messages)
        {
            _txns = txns;
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();

            Subscribe<SceneCreateEvent>(
                MessageTypes.SCENE_CREATE,
                message =>
                {
                    Log.Info(this, "Received scene create event.");

                    // TODO: THIS
                    //_txns.TrackScene(message.Scene);
                });

            Subscribe<SceneUpdateEvent>(
                MessageTypes.SCENE_UPDATE,
                message =>
                {
                    Log.Info(this, "Received scene update event: \n{0}", message);

                    // all values come in as strings
                    PrepActions(message);

                    Log.Info(this, "{0} actions prepped.", message.Actions.Length);

                    // APPLY
                    _txns.Apply(new ElementTxn(message.Scene, message.Actions));
                });

            Subscribe<SceneDeleteEvent>(
                MessageTypes.SCENE_DELETE,
                message =>
                {
                    Log.Info(this, "Received scene delete event.");

                    // TODO: THIS
                    //_txns.UntrackScene(message.Scene);
                });
        }

        /// <summary>
        /// Preps actions by parsing action values.
        /// </summary>
        /// <param name="message">The message.</param>
        private void PrepActions(SceneUpdateEvent message)
        {
            for (var i = message.Actions.Length - 1; i >= 0; i--)
            {
                var action = message.Actions[i];
                if (action.Type == ElementActionTypes.CREATE
                    || action.Type == ElementActionTypes.DELETE)
                {
                    continue;
                }

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
                    case ElementActionSchemaTypes.STRING:
                    {
                        action.Value = valueString;
                        break;
                    }
                    default:
                    {
                        Log.Error(this, "Removing invalid action: no SchemaType!");
                        message.Actions = message.Actions.Remove(action);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Parses into a T.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="value">The string value.</param>
        /// <returns></returns>
        private T Parse<T>(string value)
        {
            object instance;
            var bytes = Encoding.UTF8.GetBytes(value);
            _serializer.Deserialize(typeof(T), ref bytes, out instance);

            return (T) instance;
        }
    }
}