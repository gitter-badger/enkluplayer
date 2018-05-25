using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using CreateAR.Trellis.Messages.SearchPublishedApps;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace CreateAR.SpirePlayer.Mobile
{
    /// <summary>
    /// UI for viewing and searching public apps.
    /// </summary>
    public class MobileAppSearchUIView : MonoBehaviourUIElement
    {
        /// <summary>
        /// Current app elements.
        /// </summary>
        private readonly List<AppsListElementController> _elements = new List<AppsListElementController>();

        /// <summary>
        /// Last query value.
        /// </summary>
        private string _lastQuery;
        
        /// <summary>
        /// Last query value we dispatched.
        /// </summary>
        private string _lastDispatchedQuery;
        
        /// <summary>
        /// Time at last change.
        /// </summary>
        private DateTime _lastChange;
        
        /// <summary>
        /// The search field.
        /// </summary>
        public InputField QueryField;
        
        /// <summary>
        /// Transform to add list elements to.
        /// </summary>
        public Transform Content;

        /// <summary>
        /// Prefab to populate list with.
        /// </summary>
        public AppsListElementController ElementPrefab;

        /// <summary>
        /// How often to update the query.
        /// </summary>
        public float QueryUpdateSecs = 1f;
        
        /// <summary>
        /// The query.
        /// </summary>
        public string Query
        {
            get
            {
                return _lastDispatchedQuery;
            }
        }
        
        /// <summary>
        /// Called when query has been updated.
        /// </summary>
        public event Action<string> OnQueryUpdated;

        /// <summary>
        /// Called when app has been selected.
        /// </summary>
        public event Action<string> OnAppSelected;

        /// <summary>
        /// Called to sign in.
        /// </summary>
        public event Action OnSignIn;

        /// <summary>
        /// Initializes the view with apps.
        /// </summary>
        /// <param name="apps">The apps.</param>
        public void Init(Body[] apps)
        {
            ClearErrors();
            ClearElements();
            
            foreach (var app in apps)
            {
                var controller = Instantiate(ElementPrefab, Content);
                controller.OnSelected += Controller_OnSelected;
                controller.Init(app.Id, app.Name, app.Description);
                
                _elements.Add(controller);
            }
        }

        /// <summary>
        /// Displays an error.
        /// </summary>
        /// <param name="error">The error to display.</param>
        public void ShowError(string error)
        {
            ClearErrors();
            ClearElements();
        }

        /// <summary>
        /// Called when sign in is clicked.
        /// </summary>
        public void SignInClicked()
        {
            if (null != OnSignIn)
            {
                OnSignIn();
            }
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        private void Update()
        {
            var query = QueryField.text;
            if (_lastQuery != query)
            {
                _lastChange = DateTime.Now;
                _lastQuery = query;
            }

            if (_lastDispatchedQuery != _lastQuery
                && DateTime.Now.Subtract(_lastChange).TotalSeconds > QueryUpdateSecs)
            {
                _lastDispatchedQuery = _lastQuery;

                if (null != OnQueryUpdated)
                {
                    OnQueryUpdated(_lastDispatchedQuery);
                }
            }
        }

        /// <summary>
        /// Clears errors out.
        /// </summary>
        private void ClearErrors()
        {
            //
        }

        /// <summary>
        /// Clears elements out.
        /// </summary>
        private void ClearElements()
        {
            foreach (var element in _elements)
            {
                Destroy(element.gameObject);
            }
            _elements.Clear();
        }
        
        /// <summary>
        /// Called when a controller has been selected.
        /// </summary>
        /// <param name="appId">The app id.</param>
        private void Controller_OnSelected(string appId)
        {
            if (null != OnAppSelected)
            {
                OnAppSelected(appId);
            }
        }
    }
}