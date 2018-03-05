using System;
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
            /// the content being moved.
            /// </summary>
            public ContentDesignController Content;

            /// <summary>
            /// Manages line rendering.
            /// </summary>
            public ILineManager Lines;

            /// <summary>
            /// Call to request reparent.
            /// </summary>
            public Action<Element> Reparent;

            /// <summary>
            /// Call to cancel reparent.
            /// </summary>
            public Action Cancel;
        }

        /// <summary>
        /// Line to our parent.
        /// </summary>
        private readonly LineData _line = new LineData();

        /// <summary>
        /// Context passed in to init.
        /// </summary>
        private ReparentDesignControllerContext _context;

        /// <summary>
        /// Selection menu.
        /// </summary>
        private ElementSelectionMenuController _selectionMenu;
        
        /// <inheritdoc />
        public override void Initialize(Element element, object context)
        {
            base.Initialize(element, context);

            _context = (ReparentDesignControllerContext) context;
            _context.Lines.Add(_line);

            InitializeSelectionMenu();
        }
        
        /// <inheritdoc />
        public override void Uninitialize()
        {
            base.Uninitialize();

            _context.Lines.Remove(_line);
            
            UninitializeSelectionMenu();
        }
        
        /// <inheritdoc cref="MonoBehaviour"/>
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

        /// <summary>
        /// Turns on the selection menu.
        /// </summary>
        private void InitializeSelectionMenu()
        {
            _selectionMenu = gameObject.GetComponent<ElementSelectionMenuController>()
                       ?? gameObject.AddComponent<ElementSelectionMenuController>();
            _selectionMenu.OnSelected += SelectionMenu_OnSelected;
            _selectionMenu.enabled = true;

            if (_context.Content.Element == Element)
            {
                _selectionMenu.MarkAsTarget();
            }
        }

        /// <summary>
        /// Turns off the selection menu.
        /// </summary>
        private void UninitializeSelectionMenu()
        {
            _selectionMenu.OnSelected -= SelectionMenu_OnSelected;
            _selectionMenu.enabled = false;
        }

        /// <summary>
        /// Called when we make a selection.
        /// </summary>
        private void SelectionMenu_OnSelected()
        {
            if (_selectionMenu.IsTarget)
            {
                _context.Cancel();
            }
            else
            {
                _context.Reparent(Element);
            }
        }
    }
}