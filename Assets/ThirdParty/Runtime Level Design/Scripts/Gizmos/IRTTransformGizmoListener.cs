namespace RLD
{
    public interface IRTDragGizmoListener
    {
        void OnStartDrag();
        void OnEndDrag();
    }

    public interface IRTTransformGizmoListener
    {
        bool OnCanBeTransformed(Gizmo transformGizmo);
        void OnTransformed(Gizmo transformGizmo);
    }
}
