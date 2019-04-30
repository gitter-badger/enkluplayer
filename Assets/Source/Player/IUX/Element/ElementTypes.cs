using System;
using System.Linq;
using System.Reflection;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Links an element type with a type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ElementLinkAttribute : Attribute
    {
        /// <summary>
        /// The type.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementLinkAttribute(Type type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// Application-wide message types.
    /// </summary>
    public class ElementTypes
    {
        ///////////////////////////////////////////////////////////////////////
        // Types.
        ///////////////////////////////////////////////////////////////////////
        [ElementLink(typeof(ContainerWidget))]
        public const int CONTAINER = 0;

        [ElementLink(typeof(ImageWidget))]
        public const int IMAGE = 5;

        [ElementLink(typeof(ButtonWidget))]
        public const int BUTTON = 10;

        [ElementLink(typeof(Cursor))]
        public const int CURSOR = 20;

        [ElementLink(typeof(TextWidget))]
        public const int CAPTION = 30;

        [ElementLink(typeof(MenuWidget))]
        public const int MENU = 100;

        [ElementLink(typeof(SubMenuWidget))]
        public const int SUBMENU = 101;

        [ElementLink(typeof(TextCrawlWidget))]
        public const int TEXTCRAWL = 120;

        [ElementLink(typeof(FloatWidget))]
        public const int FLOAT = 130;

        [ElementLink(typeof(ScreenWidget))]
        public const int SCREEN = 131;

        [ElementLink(typeof(ToggleWidget))]
        public const int TOGGLE = 140;

        [ElementLink(typeof(SliderWidget))]
        public const int SLIDER = 150;

        [ElementLink(typeof(SelectWidget))]
        public const int SELECT = 200;

        [ElementLink(typeof(GridWidget))]
        public const int GRID = 201;

        [ElementLink(typeof(Option))]
        public const int OPTION = 210;

        [ElementLink(typeof(OptionGroup))]
        public const int OPTION_GROUP = 211;

        [ElementLink(typeof(ContentWidget))]
        public const int CONTENT = 1000;
        [ElementLink(typeof(ContentWidget))]
        public const int ASSET = 1000;

        [ElementLink(typeof(LightWidget))]
        public const int LIGHT = 1100;

        [ElementLink(typeof(ScanWidget))]
        public const int SCAN = 1200;

        public const int KINECT = 1300;

        public const int CAMERA = 1400;

        [ElementLink(typeof(ScaleTransition))]
        public const int TRANSITION_SCALE = 10000;

        [ElementLink(typeof(Transition))]
        public const int TRANSITION = 10001;

        [ElementLink(typeof(WorldAnchorWidget))]
        public const int WORLD_ANCHOR = 1000000;

        [ElementLink(typeof(QrAnchorWidget))]
        public const int QR_ANCHOR = 1000001;

        /// <summary>
        /// Retrieves a type from an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static int TypeFromElement(Element element)
        {
            var fields = typeof(ElementTypes).GetFields(BindingFlags.Public | BindingFlags.Static);
            var type = element.GetType();
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                var attribute = field
                    .GetCustomAttributes(typeof(ElementLinkAttribute), true)
                    .FirstOrDefault() as ElementLinkAttribute;
                if (null != attribute)
                {
                    if (attribute.Type == type)
                    {
                        return (int) field.GetValue(null);
                    }
                }
            }

            return -1;
        }
    }
}