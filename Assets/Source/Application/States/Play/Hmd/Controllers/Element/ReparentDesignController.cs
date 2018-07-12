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
            /// Call to request reparent.
            /// </summary>
            public Action<Element> Reparent;

            /// <summary>
            /// Call to cancel reparent.
            /// </summary>
            public Action Cancel;
        }
        
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

            InitializeSelectionMenu();
        }
        
        /// <inheritdoc />
        public override void Uninitialize()
        {
            base.Uninitialize();
            
            UninitializeSelectionMenu();
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