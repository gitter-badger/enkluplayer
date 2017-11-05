using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// A widget that toggles another widget on or off depending on highlight.
    /// </summary>
    public class InteractableWidget : Widget
    {
        /// <summary>
        /// Interaction Locked to a Specific Widget.
        /// </summary>
        public static bool OnRails = false;

        /// <summary>
        /// If true, the widget starts highlighted.
        /// </summary>
        public bool IsHighlighted = false;

        /// <summary>
        /// If true, is locked and cannot be interacted with.
        /// </summary>
        public bool IsInteractionEnabled = true;

        /// <summary>
        /// Hides the highlight widget.
        /// </summary>
        public bool HideHighlightWidget;

        /// <summary>
        /// Shows if highlighted.
        /// </summary>
        public Widget ShowIfHighlightedWidget;

        /// <summary>
        /// Returns true if interactable.
        /// </summary>
        public bool IsInteractable
        {
            get
            {
                return
                    IsInteractionEnabled
                && (!OnRails || IsHighlighted);
            }
        }

        /// <summary>
        /// Initializes the InteractableWidget.
        /// </summary>
        /// <param name="schema"></param>
        public virtual void SetSchema(InteractableSchema schema)
        {
            IsHighlighted |= schema.Highlight;
            HighlightPriority = schema.HighlightPriority;
        }

        /// <summary>
        /// Updates visibility of ShowIfHighlightedWidget.
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (IsVisible)
            {
                if (ShowIfHighlightedWidget != null)
                {
                    var isHighlighted = false;
                    var highlightWidget = Elements.Highlighted;
                    if (highlightWidget != null)
                    {
                        if (this == (InteractableWidget) highlightWidget)
                        {
                            if (IsDescendant(highlightWidget.Transform, transform)
                                || IsDescendant(transform, highlightWidget.Transform))
                            {
                                isHighlighted = true;
                            }
                        }
                    }

                    ShowIfHighlightedWidget.LocalVisible = isHighlighted && !HideHighlightWidget;
                }
            }
        }

        /// <summary>
        /// Checks if there is a child/parent relationship.
        /// </summary>
        private static bool IsDescendant(Transform ancestor, Transform descendant)
        {
            if (descendant == ancestor)
            {
                return true;
            }

            if (descendant.IsChildOf(ancestor))
            {
                return true;
            }

            if (descendant.parent != null)
            {
                return IsDescendant(ancestor, descendant.parent);
            }

            return false;
        }
    }
}