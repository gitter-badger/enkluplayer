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
    public class MobileAppSearchUIView : MonoBehaviourUIElement
    {
        private readonly List<AppsListElementController> _elements = new List<AppsListElementController>();

        private string _lastQuery;
        
        private string _lastDispatchedQuery;
        
        private DateTime _lastChange;
        
        public InputField QueryField;
        
        public Transform Content;

        public AppsListElementController ElementPrefab;

        public float QueryUpdateSecs = 1f;
        
        public string Query
        {
            get
            {
                return _lastDispatchedQuery;
            }
        }
        
        public event Action<string> OnQueryUpdated;

        public event Action<string> OnAppSelected;

        public void Init(Body[] payloadBody)
        {
            ClearErrors();
            ClearElements();
            
            foreach (var app in payloadBody)
            {
                var controller = Instantiate(ElementPrefab, Content);
                controller.Init(app.Id, app.Name, app.Description);
                
                _elements.Add(controller);
            }
        }
        
        public void ShowError(string error)
        {
            ClearErrors();
            ClearElements();
        }

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

        private void ClearErrors()
        {
            //
        }

        private void ClearElements()
        {
            foreach (var element in _elements)
            {
                Destroy(element.gameObject);
            }
            _elements.Clear();
        }
    }
}