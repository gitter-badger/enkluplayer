using System.Text;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using Enklu.Data;

namespace CreateAR.EnkluPlayer
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
                    message.Actions = PrepActions(message.Actions);

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

            Subscribe<BridgeHelperActionEvent>(
                MessageTypes.BRIDGE_HELPER_PREVIEW_ACTION,
                OnAction);
        }

        /// <summary>
        /// Called when actions are sent directly over bridge. This is usually for a preview.
        /// </summary>
        /// <param name="event">The event.</param>
        private void OnAction(BridgeHelperActionEvent @event)
        {
            var txn = new ElementTxn(@event.SceneId);
            txn.Actions.AddRange(PrepActions(@event.Actions));

            _txns.Apply(txn);
        }

        /// <summary>
        /// Preps actions by parsing action values.
        /// </summary>
        private ElementActionData[] PrepActions(ElementActionData[] actions)
        {
            for (var i = actions.Length - 1; i >= 0; i--)
            {
                var action = actions[i];
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
                        int value;
                        if (int.TryParse(valueString, out value))
                        {
                            action.Value = value;
                        }
                        break;
                    }
                    case ElementActionSchemaTypes.FLOAT:
                    {
                        float value;
                        if (float.TryParse(valueString, out value))
                        {
                            action.Value = value;
                        }
                        
                        break;
                    }
                    case ElementActionSchemaTypes.BOOL:
                    {
                        bool value;
                        if (bool.TryParse(valueString, out value))
                        {
                            action.Value = value;
                        }

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
                        actions = actions.Remove(action);
                        break;
                    }
                }
            }

            return actions;
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