using System.IO;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer;
using CreateAR.SpirePlayer.Test;
using strange.extensions.injector.impl;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Application;
using PlayMode = CreateAR.SpirePlayer.PlayMode;

namespace CreateAR.Spire.Editor
{
    public class AppEditorWindow : EditorWindow
    {
        [Inject]
        public IAppDataManager AppData { get; set; }
        [Inject]
        public IFileManager Files { get; set; }
        [Inject]
        public IAssetManager Assets { get; set; }

        private AppView _appView;

        [MenuItem("Tools/App Editor")]
        private static void Open()
        {
            GetWindow<AppEditorWindow>();
        }
        
        private void OnEnable()
        {
            titleContent = new GUIContent("App Editor");

            var binder = new InjectionBinder();
            binder.Load(new SpirePlayerModule(PlayMode.Release));

            binder.injector.Inject(this);

            // configure logs
            Log.AddLogTarget(new UnityLogTarget(new DefaultLogFormatter()));

            // configure filesystem
            Files.Register(
                FileProtocols.APP,
                new SystemXmlSerializer(),
                new LocalFileSystem("Assets/Data/App"));

            // configure asset manager
            Assets.Initialize(new AssetManagerConfiguration
            {
                Loader = new EditorAssetLoader(),
                Queries = new StandardQueryResolver(),
                Service = new DummyAssetUpdateService()
            });

            // configure components
            _appView = new AppView(AppData);
            _appView.OnRepaintRequested += Repaint;
        }

        private void OnDisable()
        {
            //
        }

        private void OnGUI()
        {
            GUI.skin = (GUISkin) EditorGUIUtility.Load("CreateAR.Editor.guiskin");

            EditorGUILayout.BeginVertical(
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));
            {
                DrawToolbar();

                EditorGUILayout.BeginHorizontal(
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true));
                {
                    _appView.Draw();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(
                EditorStyles.toolbar,
                GUILayout.ExpandWidth(true));
            {
                if (GUILayout.Button(
                    "Open",
                    EditorStyles.toolbarButton,
                    GUILayout.ExpandWidth(false)))
                {
                    OpenApp();
                }

                if (GUILayout.Button(
                    "Publish",
                    EditorStyles.toolbarButton,
                    GUILayout.ExpandWidth(false)))
                {
                    PublishApp();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void PublishApp()
        {
            throw new System.NotImplementedException();
        }

        private void OpenApp()
        {
            var appPath = EditorUtility.OpenFilePanel(
                "App",
                Path.Combine(Application.dataPath, "Data/App"),
                "local");
            if (string.IsNullOrEmpty(appPath))
            {
                return;
            }

            var serializer = new SystemXmlSerializer();
            var bytes = File.ReadAllBytes(appPath);
            object @object;
            serializer.Deserialize(typeof(AppData), ref bytes, out @object);

            var app = (AppData) @object;
            var appName = app.Id;

            AppData
                .Load(appName)
                .OnSuccess(_ =>
                {
                    Log.Info(this, "AppData loaded.");

                    _appView.Initialize();
                })
                .OnFailure(exception =>
                {
                    Log.Error(this, "Could not load AppData : {0}.", exception);
                });
        }
    }
}
