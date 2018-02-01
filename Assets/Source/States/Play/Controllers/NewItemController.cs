using System;
using System.Collections.Generic;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.IUX;
using CreateAR.SpirePlayer.Vine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the new item menu.
    /// </summary>
    public class NewItemController : InjectableMonoBehaviour
    {
        private class AssetGroup
        {
            public readonly string GroupName;
            public readonly List<AssetData> Assets = new List<AssetData>();

            public AssetGroup(string groupName)
            {
                GroupName = groupName;
            }
        }

        /// <summary>
        /// Events to listen to.
        /// </summary>
        private IUXEventHandler _events;

        /// <summary>
        /// Container to add everything to.
        /// </summary>
        private Element _container;

        /// <summary>
        /// Grid element.
        /// </summary>
        private GridWidget _grid;

        /// <summary>
        /// Back button.
        /// </summary>
        private ButtonWidget _backButton;

        /// <summary>
        /// Parses vines.
        /// </summary>
        [Inject]
        public VineImporter Parser { get; set; }

        /// <summary>
        /// Creates elements.
        /// </summary>
        [Inject]
        public IElementFactory Elements{ get; set; }

        /// <summary>
        /// App data.
        /// </summary>
        [Inject]
        public IAssetManager Assets { get; set; }

        /// <summary>
        /// Called when we wish to create a prop.
        /// </summary>
        public event Action OnConfirm;

        /// <summary>
        /// Called when we wish to cancel prop creation.
        /// </summary>
        public event Action OnCancel;

        /// <summary>
        /// Initializes the controller + readies it for show/hide.
        /// </summary>
        /// <param name="events">Events to listen to.</param>
        /// <param name="container">Container to add elements to.</param>
        public void Initialize(IUXEventHandler events, Element container)
        {
            _events = events;
            _container = container;
        }

        /// <summary>
        /// Shows the menu.
        /// </summary>
        public void Show()
        {
            {
                var description = Parser.Parse(
                    @"<?Vine><Button id='btn-back' icon='cancel' />");

                // TODO: REMOVE HACK
                description.Elements[0].Schema.Vectors["position"] = new Vec3(-0.35f, 0, 0);

                _backButton = (ButtonWidget) Elements.Element(description);
                _backButton.Activator.OnActivated += BackButton_OnActivate;
                _container.AddChild(_backButton);
            }

            {
                var vine = string.Format(
                    @"<Grid fontSize=20>{0}</Grid>",
                    AssetsToVine());
                _grid = (GridWidget) Elements.Element(Parser.Parse(vine));
                _container.AddChild(_grid);
            }
        }

        /// <summary>
        /// Hides the menu.
        /// </summary>
        public void Hide()
        {
            _backButton.Activator.OnActivated -= BackButton_OnActivate;
            _backButton.Destroy();

            _grid.Destroy();
        }

        /// <summary>
        /// Uninitializes controller. Show/Hide should not be called again
        /// until Initialize is called.
        /// </summary>
        public void Uninitialize()
        {
            
        }

        /// <summary>
        /// Creates a vine string from asset data.
        /// </summary>
        /// <returns></returns>
        private string AssetsToVine()
        {
            var groups = GroupAssets();
            var vine = "";

            for (int i = 0, len = groups.Count; i < len; i++)
            {
                var group = groups[i];
                var assets = group.Assets;
                vine += string.Format(
                    "<OptionGroup value='{0}' label='{0}'>",
                    group.GroupName);
                for (int j = 0, jlen = assets.Count; j < jlen; j++)
                {
                    var asset = assets[i];
                    vine += string.Format(
                        "<Option value='{0}' label='{1}' src='{2}' />",
                        asset.Guid,
                        asset.Description,
                        asset.UriThumb);
                }
                vine += "</OptionGroup>";
            }

            return vine;
        }

        /// <summary>
        /// Groups assets together by tags.
        /// </summary>
        /// <returns></returns>
        private List<AssetGroup> GroupAssets()
        {
            var assets = Assets.Manifest.All;
            var groups = new List<AssetGroup>();
            for (int i = 0, ilen = assets.Length; i < ilen; i++)
            {
                var asset = assets[i];

                var found = false;
                for (int j = 0, jlen = groups.Count; j < jlen; j++)
                {
                    var group = groups[j];
                    if (group.GroupName == asset.Tags)
                    {
                        group.Assets.Add(asset);
                        found = true;

                        break;
                    }
                }

                if (!found)
                {
                    var group = new AssetGroup(asset.Tags);
                    group.Assets.Add(asset);

                    groups.Add(group);
                }
            }

            return groups;
        }

        /// <summary>
        /// Called when the back button has been pressed.
        /// </summary>
        /// <param name="activatorPrimitive">The primitive.</param>
        private void BackButton_OnActivate(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnCancel)
            {
                OnCancel();
            }
        }
    }
}