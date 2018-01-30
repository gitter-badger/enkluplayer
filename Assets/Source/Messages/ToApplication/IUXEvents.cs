namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Base class for IUX events.
    /// </summary>
    public abstract class IUXEvent
    {
        /// <summary>
        /// Message type.
        /// </summary>
        public readonly int Type;

        /// <summary>
        /// Element that fired event.
        /// </summary>
        public readonly Element Target;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Message type.</param>
        /// <param name="target">The target of the message.</param>
        protected IUXEvent(int type, Element target)
        {
            Type = type;
            Target = target;
        }
    }

    /// <summary>
    /// Fired when a widget has been focused on.
    /// </summary>
    public class WidgetFocusEvent : IUXEvent
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public WidgetFocusEvent(Widget target)
            : base(MessageTypes.WIDGET_FOCUS, target)
        {
            //
        }
    }

    /// <summary>
    /// Fired when a widget has been unfocused.
    /// </summary>
    public class WidgetUnfocusEvent : IUXEvent
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public WidgetUnfocusEvent(Widget target)
            : base(MessageTypes.WIDGET_UNFOCUS, target)
        {
            //
        }
    }

    /// <summary>
    /// Fired when a button has been activated.
    /// </summary>
    public class ButtonActivateEvent : IUXEvent
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ButtonActivateEvent(Widget target)
            : base(MessageTypes.BUTTON_ACTIVATE, target)
        {
            //
        }
    }
}