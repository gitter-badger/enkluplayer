using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Trellis.Messages.GetMyApps;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class LoadAppController : MonoBehaviour
    {
        private Body[] _apps;

        public event Action<string> OnAppSelected; 
        
        public void Show(Body[] apps)
        {
            _apps = apps;
            
            Log.Info(this, "Show {0} apps.", _apps.Length);
        }

        private void OnGUI()
        {
            if (null == _apps)
            {
                return;
            }
            
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                GUILayout.FlexibleSpace();
                
                GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                {
                    foreach (var app in _apps)
                    {
                        GUILayout.BeginHorizontal("box");
                        {
                            if (GUILayout.Button(app.Name, GUILayout.Height(60), GUILayout.Width(400)))
                            {
                                if (null != OnAppSelected)
                                {
                                    OnAppSelected(app.Id);
                                }
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndVertical();
                
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }
    }
}