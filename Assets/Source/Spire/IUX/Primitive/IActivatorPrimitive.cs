namespace CreateAR.SpirePlayer.UI
{
    public interface IActivatorPrimitive : IInteractivePrimitive
    {
        void SetStabilityRotation(float degress);
        void SetActivationFill(float percent);
        void SetAimScale(float aimScale);
    }
}
