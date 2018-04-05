using System;
using CreateAR.Trellis.Messages.GetMyApps;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls view for loading app.
    /// </summary>
    public class LoadAppController : MonoBehaviour
    {
        /// <summary>
        /// Prefab for app list.
        /// </summary>
        public AppsListElementController AppListElement;

        /// <summary>
        /// Container for content.
        /// </summary>
        public Transform Content;
        
        /// <summary>
        /// Called when an app has been selected to load.
        /// </summary>
        public event Action<string> OnAppSelected; 
        
        /// <summary>
        /// Shows information for apps.
        /// </summary>
        /// <param name="apps">The apps to show in the list.</param>
        public void Show(Body[] apps)
        {
            foreach (var app in apps)
            {
                var controller = Instantiate(AppListElement, Content);
                controller.OnSelected += OnAppSelected;   
                controller.Init(app);
            }
        }
    }
}