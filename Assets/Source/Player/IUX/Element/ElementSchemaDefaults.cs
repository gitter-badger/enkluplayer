using System;
using System.Collections.Generic;
using CreateAR.EnkluPlayer;
using CreateAR.EnkluPlayer.IUX;

namespace Source.Player.IUX
{
    public class ElementSchemaDefaults
    {
        /// <summary>
        /// All widgets inherit this base schema
        /// </summary>
        private readonly ElementSchema _baseSchema = new ElementSchema("Base");
        
        /// <summary>
        /// Lookup from element type to base schema for that type.
        /// </summary>
        private readonly Dictionary<int, ElementSchema> _typeSchema = new Dictionary<int, ElementSchema>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementSchemaDefaults()
        {
            // TODO: Load this all from data
            _baseSchema.Set("tweenIn", TweenType.Responsive);
            _baseSchema.Set("tweenOut", TweenType.Deliberate);
            _baseSchema.Set("color", Col4.White);
            _baseSchema.Set("virtualColor", "None");
            _baseSchema.Set("colorMode", WidgetColorMode.InheritColor);
            _baseSchema.Set("layerMode", LayerMode.Default);
            _baseSchema.Set("autoDestroy", false);
            _baseSchema.Set("font", "Watchword_bold");
            
            // load defaults
            var buttonSchema = _typeSchema[ElementTypes.BUTTON] = new ElementSchema("Base.Button");
            buttonSchema.Load(new ElementSchemaData
            {
                Ints = new Dictionary<string, int>
                {
                    { "fontSize", 70 }
                },
                Strings = new Dictionary<string, string>
                {
                    {"ready.color", VirtualColor.Ready.ToString()},
                    {"ready.captionColor", VirtualColor.Primary.ToString()},
                    {"ready.tween", TweenType.Responsive.ToString()},

                    {"activating.color", VirtualColor.Interacting.ToString()},
                    {"activating.captionColor", VirtualColor.Interacting.ToString()},
                    {"activating.tween", TweenType.Responsive.ToString()},

                    {"activated.color", VirtualColor.Interacting.ToString()},
                    {"activated.captionColor", VirtualColor.Interacting.ToString()},
                    {"activated.tween", TweenType.Responsive.ToString()}
                },
                Floats = new Dictionary<string, float>
                {
                    {"ready.frameScale", 1.0f},
                    {"activating.frameScale", 1.1f},
                    {"activated.frameScale", 1.0f},
                    {"label.padding", 60},
                    {"icon.scale", 1f},
                    {"fill.duration.multiplier", 1f},
                    {"aim.multiplier", 1f},
                    {"stability.multiplier", 1f}
                },
                Vectors = new Dictionary<string, Vec3>
                {
                    { "position", new Vec3(0f, 0f, 0f) },

                    { "ready.scale", new Vec3(1, 1, 1) },
                    { "activating.scale", new Vec3(1.1f, 1.1f, 1.1f) },
                    { "activated.scale", new Vec3(1, 1, 1) }
                }
            });
            buttonSchema.Inherit(_baseSchema);
            _typeSchema[ElementTypes.SELECT] = _typeSchema[ElementTypes.TOGGLE] = buttonSchema;
            
            var textSchema = new ElementSchema("Base.Text");
            textSchema.Load(new ElementSchemaData
            {
                Strings = new Dictionary<string, string>
                {
                    { "verticalOverflow", "Overflow" },
                    { "font", "Watchword_bold" }  
                },
                Ints = new Dictionary<string, int>
                {
                    { "fontSize", 80 }
                },
                Floats = new Dictionary<string, float>
                {
                    { "lineSpacing", 1f }
                }
            });
            _typeSchema[ElementTypes.CAPTION] = textSchema;
            
            var menuSchema = _typeSchema[ElementTypes.MENU] = new ElementSchema("Base.Menu");
            menuSchema.Load(new ElementSchemaData
            {
                Strings = new Dictionary<string, string>
                {
                    { "layout", "Radial" }
                },
                Floats = new Dictionary<string, float>
                {
                    { "layout.radius", 0.8f },
                    { "layout.degrees", 25f },
                    { "divider.offset", 0f }
                },
                Ints = new Dictionary<string, int>
                {
                    { "fontSize", 80 },
                    { "header.width", 700 },
                    { "page.size", 4 },
                }
            });
            menuSchema.Inherit(_baseSchema);

            var submenuSchema = _typeSchema[ElementTypes.SUBMENU] = new ElementSchema("Base.SubMenu");
            submenuSchema.Inherit(menuSchema);

            var gridSchema = _typeSchema[ElementTypes.GRID] = new ElementSchema("Base.Grid");
            gridSchema.Load(new ElementSchemaData
            {
                Floats = new Dictionary<string, float>
                {
                    { "padding.vertical", .15f },
                    { "padding.horizontal", .15f }
                }
            });

            var imageSchema = _typeSchema[ElementTypes.IMAGE] = new ElementSchema("Base.Image");
            imageSchema.Load(new ElementSchemaData());

            var floatSchema = _typeSchema[ElementTypes.FLOAT] = new ElementSchema("Base.Float");
            floatSchema.Load(new ElementSchemaData
            {
                Strings = new Dictionary<string, string>
                {
                    { "face", "Camera" }
                },
                Floats = new Dictionary<string, float>
                {
                    { "fov.reorient", 3.5f }
                },
                Bools = new Dictionary<string, bool>
                {
                    { "focus.visible", true }
                },
                Vectors = new Dictionary<string, Vec3>
                {
                    { "position", new Vec3(0, 0, 2) }
                }
            });

            var sliderSchema = _typeSchema[ElementTypes.SLIDER] = new ElementSchema("Base.Slider");
            sliderSchema.Load(new ElementSchemaData
            {
                Floats = new Dictionary<string, float>
                {
                    { "size.max", 3f },
                    { "size.min", 1f },
                    { "radius", 0.25f },
                    { "length", 0.1f },
                },
                Strings = new Dictionary<string, string>
                {
                    { "axis", "x" }
                }
            });

            var qrAnchorSchema = _typeSchema[ElementTypes.QR_ANCHOR] = new ElementSchema("Base.QrAnchor");
            qrAnchorSchema.Load(new ElementSchemaData
            {
                Bools = new Dictionary<string, bool>
                {
                    { "visible", false }
                }
            });

            var lightSchema = _typeSchema[ElementTypes.LIGHT] = new ElementSchema("Base.Light");
            lightSchema.Load(new ElementSchemaData());

            var screenSchema = _typeSchema[ElementTypes.SCREEN] = new ElementSchema("Base.Screen");
            screenSchema.Load(new ElementSchemaData
            {
                Floats = new Dictionary<string, float>
                {
                    {"distance", 1.2f},
                    {"stabilization", 2f},
                    {"smoothing", 15f}
                }
            });
        }

        /// <summary>
        /// Returns whether this object contains a schema for the specified type.
        /// </summary>
        /// <param name="type">The type in question.</param>
        /// <returns>Whether a schema of this type exists in this object.</returns>
        public bool Has(int type)
        {
            return _typeSchema.ContainsKey(type);
        }
        
        /// <summary>
        /// Gets the default schema for the specified type. Returns a default schema if the specified does not exist.
        /// </summary>
        /// <param name="type">The type of schema requested.</param>
        /// <returns>The requested schema or a default schema if it does not exist.</returns>
        public ElementSchema Get(int type)
        {
            ElementSchema schema;
            if (_typeSchema.TryGetValue(type, out schema))
            {
                return schema;
            }

            return _baseSchema;
        }
    }
}