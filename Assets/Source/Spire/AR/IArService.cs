namespace CreateAR.SpirePlayer.AR
{
    public interface IArService
    {
        ArAnchor[] Anchors { get; }
        ArServiceConfiguration Config { get; }
        
        void Setup(ArServiceConfiguration config);
        void Teardown();
    }
}