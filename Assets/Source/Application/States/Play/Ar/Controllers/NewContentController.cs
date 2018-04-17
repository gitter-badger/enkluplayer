﻿using System;
using System.Collections.Generic;
using CreateAR.SpirePlayer.Assets;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls the new item menu.
    /// </summary>
    [InjectVine("Design.NewContent")]
    public class NewContentController : InjectableIUXController
    {
        /// <summary>
        /// Internal class to group assets by tag.
        /// </summary>
        private class AssetGroup
        {
            /// <summary>
            /// Name of the group.
            /// </summary>
            public readonly string GroupName;

            /// <summary>
            /// The asset in question.
            /// </summary>
            public readonly List<AssetData> Assets = new List<AssetData>();

            /// <summary>
            /// Constructor.
            /// </summary>
            public AssetGroup(string groupName)
            {
                GroupName = groupName;
            }
        }

        /// <summary>
        /// Grid element.
        /// </summary>
        [InjectElements("..(@type==GridWidget)")]
        public GridWidget Grid { get; private set; }

        /// <summary>
        /// Back button.
        /// </summary>
        [InjectElements("..btn-back")]
        public ButtonWidget BtnBack { get; private set; }
        
        /// <summary>
        /// App data.
        /// </summary>
        [Inject]
        public IAssetManager Assets { get; set; }

        /// <summary>
        /// Called when we wish to create a prop.
        /// </summary>
        public event Action<string> OnConfirm;

        /// <summary>
        /// Called when we wish to cancel prop creation.
        /// </summary>
        public event Action OnCancel;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            Assets.Manifest.OnAssetAdded += Manifest_OnAssetEvent;
            Assets.Manifest.OnAssetRemoved += Manifest_OnAssetEvent;
            Assets.Manifest.OnAssetUpdated += Manifest_OnAssetEvent;

            BtnBack.Activator.OnActivated += BackButton_OnActivate;
            Grid.OnSelected += Grid_OnSelected;

            UpdateOptions();
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();

            Assets.Manifest.OnAssetAdded -= Manifest_OnAssetEvent;
            Assets.Manifest.OnAssetRemoved -= Manifest_OnAssetEvent;
            Assets.Manifest.OnAssetUpdated -= Manifest_OnAssetEvent;
        }

        /// <summary>
        /// Deletes old options and creates new ones.
        /// </summary>
        private void UpdateOptions()
        {
            for (var i = Grid.Children.Count - 1; i >= 0; i--)
            {
                var group = Grid.Children[i] as OptionGroup;
                if (null != group)
                {
                    Grid.RemoveChild(group);

                    group.Destroy();
                }
            }

            var options = GenerateOptions();
            for (var i = 0; i < options.Count; i++)
            {
                Grid.AddChild(options[i]);
            }
        }

        /// <summary>
        /// Creates a vine string from asset data.
        /// </summary>
        /// <returns></returns>
        private List<OptionGroup> GenerateOptions()
        {
            var groupElements = new List<OptionGroup>();
            var groups = GroupAssets();

            for (int i = 0, len = groups.Count; i < len; i++)
            {
                var group = groups[i];
                var assets = group.Assets;
                var vine = string.Format(
                    "<?Vine><OptionGroup value='{0}' label='{0}'>",
                    group.GroupName);
                for (int j = 0, jlen = assets.Count; j < jlen; j++)
                {
                    var asset = assets[j];
                    vine += string.Format(
                        "<Option value='{0}' label='{1}' src='thumbs:/{2}' />",
                        asset.Guid,
                        FormatLabel(asset.AssetName),
                        asset.UriThumb);
                }
                vine += "</OptionGroup>";

                groupElements.Add((OptionGroup) Elements.Element(vine));
            }
            
            return groupElements;
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
        /// Formats an assetname for a label.
        /// </summary>
        /// <param name="assetName">The asset's name.</param>
        /// <returns></returns>
        private string FormatLabel(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                return "Unknown";
            }

            var substrings = assetName.Split('.');
            return substrings[0];
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

        /// <summary>
        /// Called when something is selected in the grid.
        /// </summary>
        /// <param name="option">The selected option/</param>
        private void Grid_OnSelected(Option option)
        {
            if (null != OnConfirm)
            {
                OnConfirm(option.Value);
            }
        }

        /// <summary>
        /// Called when manifest is updated.
        /// </summary>
        /// <param name="asset">The asset/</param>
        private void Manifest_OnAssetEvent(Asset asset)
        {
            UpdateOptions();
        }
    }
}