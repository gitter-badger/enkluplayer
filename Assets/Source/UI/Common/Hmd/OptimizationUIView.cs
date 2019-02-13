using System;
using System.Collections.Generic;
using System.Linq;
using CreateAR.EnkluPlayer.Assets;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Various optimization controls.
    /// </summary>
    public class OptimizationUIView : MonoBehaviourIUXController
    {
        /// <summary>
        /// Called when the ui should be closed.
        /// </summary>
        public event Action OnClose;
        
        /// <summary>
        /// Dependencies.
        /// </summary>
        [Inject]
        public IAppDataManager Data { get; set; }
        [Inject]
        public IAssetManager Assets { get; set; }

        /// <summary>
        /// Elements.
        /// </summary>
        [InjectElements("..btn-close")]
        public ButtonWidget BtnClose { get; set; }
        [InjectElements("..slt-tab")]
        public SelectWidget SltTab { get; set; }
        [InjectElements("..tab-scripts")]
        public ContainerWidget TabScripts { get; set; }
        [InjectElements("..tab-assets")]
        public ContainerWidget TabAssets{ get; set; }
        [InjectElements("..tab-anchors")]
        public ContainerWidget TabAnchors { get; set; }
        [InjectElements("..scripts-list")]
        public ContainerWidget ScriptListContainer { get; set; }
        [InjectElements("..assets-list")]
        public ContainerWidget AssetListContainer { get; set; }

        /// <summary>
        /// Backing data for categories.
        /// </summary>
        private readonly Dictionary<string, bool> _scriptCategories = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> _assetCategories = new Dictionary<string, bool>();

        /// <inheritdoc />
        public override void Added()
        {
            base.Added();

            Data.OnUpdated += Data_OnUpdated;
            Assets.Manifest.OnAssetUpdated += Assets_OnUpdated;
            Assets.Manifest.OnAssetAdded += Assets_OnUpdated;
            Assets.Manifest.OnAssetRemoved += Assets_OnUpdated;
        }

        /// <inheritdoc />
        public override void Removed()
        {
            base.Removed();

            Data.OnUpdated -= Data_OnUpdated;
            Assets.Manifest.OnAssetUpdated -= Assets_OnUpdated;
            Assets.Manifest.OnAssetAdded -= Assets_OnUpdated;
            Assets.Manifest.OnAssetRemoved -= Assets_OnUpdated;
        }

        /// <inheritdoc />
        protected override void AfterElementsCreated()
        {
            base.AfterElementsCreated();

            // close button
            BtnClose.OnActivated += _ =>
            {
                if (null != OnClose)
                {
                    OnClose();
                }
            };

            // tab switching
            SltTab.OnValueChanged += Tabs_OnChanged;

            // category options
            RebuildScriptOptions();
            RebuildAssetOptions();
        }

        /// <summary>
        /// Rebuilds all the options for scripts.
        /// </summary>
        private void RebuildScriptOptions()
        {
            ClearChildren(ScriptListContainer);

            // build categories
            var scripts = Data.GetAll<ScriptData>();
            var categories = new HashSet<string>();
            for (int i = 0, len = scripts.Length; i < len; i++)
            {
                var tags = scripts[i].Tags;
                for (int j = 0, jlen = tags.Length; j < jlen; j++)
                {
                    categories.Add(tags[j].Trim());
                }
            }

            var categoryList = categories.ToList();
            categoryList.Sort(StringComparer.InvariantCulture);

            for (int i = 0, len = categoryList.Count; i < len; i++)
            {
                var category = categoryList[i];

                bool value;
                if (!_scriptCategories.TryGetValue(category, out value))
                {
                    value = _scriptCategories[category] = true;
                }

                var toggle = (ToggleWidget) Elements.Element(string.Format(
                    @"<Toggle label='{0}' position=(0, {1}, 0) value={2}/>",
                    categoryList[i],
                    -i * 0.1f,
                    value ? "true" : "false"));
                toggle.OnValueChanged += ScriptCategory_Selected;

                ScriptListContainer.AddChild(toggle);
            }
        }

        /// <summary>
        /// Rebuilds all the options for assets.
        /// </summary>
        private void RebuildAssetOptions()
        {
            ClearChildren(AssetListContainer);

            // build categories
            var assets = Assets.Manifest.All;
            var categories = new HashSet<string>();
            for (int i = 0, len = assets.Length; i < len; i++)
            {
                var tags = assets[i].Tags.Split(',');
                for (int j = 0, jlen = tags.Length; j < jlen; j++)
                {
                    categories.Add(tags[j].Trim());
                }
            }

            var categoryList = categories.ToList();
            categoryList.Sort(StringComparer.InvariantCulture);
            for (int i = 0, len = categoryList.Count; i < len; i++)
            {
                var category = categoryList[i];

                bool value;
                if (!_assetCategories.TryGetValue(category, out value))
                {
                    value = _assetCategories[category] = true;
                }

                var toggle = (ToggleWidget) Elements.Element(string.Format(
                    @"<Toggle label='{0}' position=(0, {1:0.0}, 0) value={2}/>",
                    categoryList[i],
                   -i * 0.1f,
                    value ? "true" : "false"));
                toggle.OnValueChanged += AssetCategory_Selected;

                AssetListContainer.AddChild(toggle);
            }
        }

        /// <summary>
        /// Called when the tab selection has changed.
        /// </summary>
        /// <param name="select">The select widget.</param>
        private void Tabs_OnChanged(SelectWidget select)
        {
            var selection = select.Selection.Value;

            TabScripts.Schema.Set("visible", "scripts" == selection);
            TabAnchors.Schema.Set("visible", "anchors" == selection);
            TabAssets.Schema.Set("visible", "assets" == selection);
        }

        /// <summary>
        /// Called when an asset category has been toggled.
        /// </summary>
        /// <param name="toggle">The toggle.</param>
        private void AssetCategory_Selected(ToggleWidget toggle)
        {
            var category = toggle.Schema.Get<string>("label").Value;
            _assetCategories[category] = toggle.Value;

            // TODO: toggle active assets.
        }

        /// <summary>
        /// Called when a script category has been toggled.
        /// </summary>
        /// <param name="toggle">The toggle.</param>
        private void ScriptCategory_Selected(ToggleWidget toggle)
        {
            var category = toggle.Schema.Get<string>("label").Value;
            _scriptCategories[category] = toggle.Value;

            // TODO: toggle active scripts.
        }

        /// <summary>
        /// Called when static data has been updated.
        /// </summary>
        /// <param name="data">The data.</param>
        private void Data_OnUpdated(StaticData data)
        {
            var script = data as ScriptData;
            if (null != script)
            {
                RebuildScriptOptions();
            }
        }

        /// <summary>
        /// Called when assets have changed in some way.
        /// </summary>
        /// <param name="data">The data.</param>
        private void Assets_OnUpdated(AssetData data)
        {
            RebuildScriptOptions();
        }

        /// <summary>
        /// Clears all children from an element.
        /// </summary>
        /// <param name="el"></param>
        private static void ClearChildren(Element el)
        {
            while (el.Children.Count > 0)
            {
                el.RemoveChild(el.Children[0]);
            }
        }
    }
}