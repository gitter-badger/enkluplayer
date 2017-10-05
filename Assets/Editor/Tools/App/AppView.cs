using System;
using System.Linq;
using CreateAR.Commons.Unity.Editor;
using UnityEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.Editor
{
    public class AppView : IEditorView
    {
        private const int LIST_WIDTH = 200;

        private readonly IAppDataManager _appData;
        private readonly ListComponent _sceneList = new ListComponent();
        private readonly TabCollectionComponent _tabs = new TabCollectionComponent();

        private readonly ScriptTab _scriptTab;
        private Vector2 _scrollPosition;

        public event Action OnRepaintRequested;

        public AppView(IAppDataManager appData)
        {
            _appData = appData;

            _sceneList.OnRepaintRequested += Repaint;
            _sceneList.OnSelected += Scenes_OnSelected;

            _scriptTab = new ScriptTab(_appData);
            _tabs.Tabs = new TabComponent[]
            {
                _scriptTab
            };
            _tabs.OnRepaintRequested += Repaint;
        }

        public void Initialize()
        {
            _sceneList.Populate(
                _appData
                    .GetAll<SceneData>()
                    .Select(data => new ListItem(data.Name, data)));
            _sceneList.Selected = _sceneList.Items.FirstOrDefault();

            Repaint();
        }

        public void Uninitialize()
        {
            Repaint();
        }

        public void Draw()
        {
            EditorGUILayout.BeginHorizontal(
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(
                    _scrollPosition,
                    GUILayout.Width(LIST_WIDTH),
                    GUILayout.ExpandHeight(true));
                {
                    _sceneList.Draw();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginVertical(
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true));
                {
                    _tabs.Draw();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void Repaint()
        {
            if (null != OnRepaintRequested)
            {
                OnRepaintRequested();
            }
        }

        private void Scenes_OnSelected(ListItem listItem)
        {
            var appData = _appData.GetAll<AppData>().FirstOrDefault();
            if (null == appData)
            {
                return;
            }

            _scriptTab.Initialize(appData, (SceneData) listItem.Value);
        }
    }
}