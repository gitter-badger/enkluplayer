using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    [InjectVine("Prop.Edit")]
    public class PropEditController : InjectableIUXController
    {
        [InjectElements("..btn-move")]
        public ButtonWidget BtnMove { get; private set; }

        [InjectElements("..btn-delete")]
        public ButtonWidget BtnDelete { get; private set; }

        [InjectElements("..toggle-fade")]
        public ToggleWidget ToggleFade { get; private set; }

        public void Initialize(PropData prop)
        {

        }
    }
}