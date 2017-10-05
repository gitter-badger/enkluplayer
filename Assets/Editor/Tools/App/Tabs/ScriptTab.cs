using System.Linq;
using CreateAR.Commons.Unity.Editor;

namespace CreateAR.Spire.Editor
{
    public class ScriptTab : TabComponent
    {
        private readonly IAppDataManager _appData;
        private readonly ListComponent _scripts = new ListComponent();

        private AppData _app;
        private SceneData _scene;
        
        public ScriptTab(IAppDataManager appData)
        {
            _appData = appData;
            _scripts.OnRepaintRequested += Repaint;

            Label = "Scripts";
        }

        public void Initialize(AppData app, SceneData scene)
        {
            _scripts.Populate(
                scene
                    .Scripts
                    .Select(scriptId => _appData.Get<ScriptData>(scriptId))
                    .Select(data => new ListItem(
                        data.Name,
                        data)));
        }

        public override void Draw()
        {
            base.Draw();


        }
    }
}
