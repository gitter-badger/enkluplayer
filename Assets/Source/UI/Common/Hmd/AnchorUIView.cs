using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Debug UI that displays Anchor related information.
    /// </summary>
    public class AnchorUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        [Inject]
        public IAppSceneManager SceneManager { get; set; }

        /// <summary>
        /// Injected Elements.
        /// </summary>
        [InjectElements("..btn-close")] 
        public ButtonWidget BtnClose { get; set; }
        
        [InjectElements("..txt-anchors")] 
        public TextWidget TxtAnchors { get; set; }
        
        /// <summary>
        /// Cache of all anchors in the scene when this window opens.
        /// </summary>
        private readonly List<WorldAnchorWidget> _anchors = new List<WorldAnchorWidget>();
        
        /// <summary>
        /// Invoked when the UI should close.
        /// </summary>
        public event Action OnClose;
        
        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            BtnClose.OnActivated += widget =>
            {
                if (OnClose != null)
                {
                    OnClose();
                }
            };
            
            SceneManager.Root(SceneManager.All.FirstOrDefault()).Find("..(@type=WorldAnchorWidget)", _anchors);
        }

        /// <summary>
        /// Updates the UI.
        /// </summary>
        private void Update()
        {
            var output = "";

            for (int i = 0, len = _anchors.Count; i < len; i++)
            {
                var anchor = _anchors[i];
                output += string.Format("[{0}] {1}\n", anchor.Status, anchor.Name);
            }

            if (TxtAnchors != null)
            {
                TxtAnchors.Label = output;
            }
        }
    }
}