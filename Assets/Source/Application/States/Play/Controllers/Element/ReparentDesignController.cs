using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Design controller for reparenting.
    /// </summary>
    public class ReparentDesignController : ElementDesignController
    {
        /// <summary>
        /// Context passed into init.
        /// </summary>
        public class ReparentDesignControllerContext
        {
            /// <summary>
            /// Manages line rendering.
            /// </summary>
            public ILineManager Lines;
        }

        /// <summary>
        /// Line to our parent.
        /// </summary>
        private readonly LineData _line = new LineData();

        /// <summary>
        /// Context passed in to init.
        /// </summary>
        private ReparentDesignControllerContext _context;

        /// <inheritdoc />
        public override void Initialize(Element element, object context)
        {
            base.Initialize(element, context);

            _context = (ReparentDesignControllerContext) context;
            _context.Lines.Add(_line);
            _context.Lines.IsEnabled = true;
        }

        /// <inheritdoc />
        public override void Uninitialize()
        {
            base.Uninitialize();

            _context.Lines.Remove(_line);
            _context.Lines.IsEnabled = false;
        }

        /// <summary>
        /// Update the line to parent.
        /// </summary>
        private void Update()
        {
            _line.Start = _line.End = transform.position;

            if (null != Element)
            {
                var parent = FindUnityParent(Element);
                if (null != parent)
                {
                    _line.End = parent.GameObject.transform.position;
                    _line.Enabled = true;
                    
                    return;
                }
            }

            _line.Enabled = false;
        }

        /// <summary>
        /// Traverses up the hierarchy until a unity parent is found.
        /// </summary>
        /// <param name="element">The starting element.</param>
        /// <returns></returns>
        private IUnityElement FindUnityParent(Element element)
        {
            while (null != element)
            {
                element = element.Parent;

                var unityParent = element as IUnityElement;
                if (null != unityParent)
                {
                    return unityParent;
                }
            }

            return null;
        }
    }
}