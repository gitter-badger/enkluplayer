using System;
using CreateAR.Trellis.Messages.GetMyApps;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls view for loading app.
    /// </summary>
    public class MobileAppSelectionUIView : MonoBehaviourUIElement, IAppSelectionUIView
    {
        /// <summary>
        /// Backing variable for Apps property.
        /// </summary>
        private Body[] _apps;
        
        /// <summary>
        /// Prefab for app list.
        /// </summary>
        public AppsListElementController AppListElement;

        /// <summary>
        /// Container for content.
        /// </summary>
        public Transform Content;
        
        /// <inheritdoc />
        public event Action<string> OnAppSelected;

        /// <inheritdoc />
        public Body[] Apps
        {
            get { return _apps; }
            set
            {
                _apps = value ?? new Body[0];
                
                foreach (var app in _apps)
                {
                    var controller = Instantiate(AppListElement, Content);
                    controller.OnSelected += OnAppSelected;   
                    controller.Init(app);
                }
            }
        }
    }
}