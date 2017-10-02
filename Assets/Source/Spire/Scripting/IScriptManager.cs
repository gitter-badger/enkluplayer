using CreateAR.SpirePlayer;

namespace CreateAR.Spire
{
    public class ScriptManager : IScriptManager
    {
        private IAssetManager _assets;

        public ScriptManager(IAssetManager assets)
        {
            _assets = assets;
        }
    }

    public interface IScriptManager
    {

    }
}