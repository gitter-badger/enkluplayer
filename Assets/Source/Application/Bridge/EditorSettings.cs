using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using RLD;
using CreateAR.EnkluPlayer;
using Object = UnityEngine.Object;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// This object represents the current state of the editor's settings.
    /// When changed, it will dispatch event OnChanged.
    /// </summary>
    public class EditorSettings
    {
        /// <summary>
        /// Backing variable for MeshScan.
        /// </summary>
        private bool _meshScan;
        
        /// <summary>
        /// Backing variable for Grid.
        /// </summary>
        private bool _grid;
        
        /// <summary>
        /// Backing variable for ElementGizmos.
        /// </summary>
        private bool _elementGizmos;
        
        /// <summary>
        /// Backing variable for HierarchyLines.
        /// </summary>
        private bool _hierarchyLines;

        /// <summary>
        /// Fired when the value of any setting has changed.
        /// </summary>
        public event Action<SettingChangedArgs> OnChanged;

        /// <summary>
        /// All scenes.
        /// </summary>
        [Inject]
        public IAppSceneManager Scenes { get; set; }

        /// <summary>
        /// Whether the mesh scan is visible.
        /// </summary>
        public bool MeshScan
        {
            get { return _meshScan; }
            set
            {
                _meshScan = value;
                
                var scans = new List<Element>();
                var all = Scenes.All;
                
                for (var i = 0; i < all.Length; i++){
                    var id = all[i];
                    var root = Scenes.Root(id);
                    root.Find("..(@type==ScanWidget)", scans);

                    for (var j = 0; j < scans.Count; j++)
                    {
                        var scan = scans[j];
                        scan.Schema.Set("visible", _meshScan);
                    }
                }

                Notify(EditorSettingsTypes.MeshScan);
            }
        }

        /// <summary>
        /// Whether the grid is visible.
        /// </summary>
        public bool Grid
        {
            get { return _grid; }
            set
            {
                _grid = value;
                
                var sceneGrid = Object.FindObjectOfType<RTSceneGrid>();
                if (null != sceneGrid)
                {
                    sceneGrid.Settings.IsVisible = _grid;
                }
                
                Notify(EditorSettingsTypes.Grid);
            }
        }
        
        /// <summary>
        /// Whether the element gizmos are visible.
        /// </summary>
        public bool ElementGizmos
        {
            get { return _elementGizmos; }
            set
            {
                _elementGizmos = value;
                
                var scene = Object.FindObjectOfType<RTScene>();
                if (null != scene)
                {
                    scene.LookAndFeel.DrawLightIcons = _elementGizmos;
                    scene.LookAndFeel.DrawParticleSystemIcons = _elementGizmos;
                }

                var gizmos = Object.FindObjectOfType<GizmoManager>();
                if (null != gizmos)
                {
                    gizmos.IsVisible = _elementGizmos;
                }
                
                Notify(EditorSettingsTypes.ElementGizmos);               
            }
        }

        /// <summary>
        /// Whether the hierarchy lines are visible.
        /// </summary>
        public bool HierarchyLines
        {
            get { return _hierarchyLines; }
            set
            {
                _hierarchyLines = value;
                
                var renderer = Object.FindObjectOfType<HierarchyLineRenderer>();
                if (null != renderer)
                {
                    renderer.enabled = _hierarchyLines;
                }
                
                Notify(EditorSettingsTypes.HierarchyLines);               
            }
        }

        /// <summary>
        /// Sets any setting.
        /// </summary>
        public void Set(string name, bool value)
        {
            Log.Info(this, "Setting {0} to {1}", name, value);
            switch (name)
            {
                case "MeshScan":
                {
                    MeshScan = value;
                    break;   
                }

                case "Grid":
                {
                    Grid = value;
                    break;   
                }

                case "ElementGizmos":
                {
                    ElementGizmos = value;
                    break;
                }

                case "HierarchyLines":
                {
                    HierarchyLines = value;
                    break;   
                }

                default:
                {
                    Log.Warning(this, "Could not set setting {0}. No such property.", name);
                    break;
                }
            }
        }

        /// <summary>
        /// Populates settings from an EditorSettingsEvent
        /// </summary>
        /// <param name="obj">The event in question</param>
        public void Populate(EditorSettingsEvent obj)
        {
            MeshScan = obj.MeshScan;
            Grid = obj.Grid;
            ElementGizmos = obj.ElementGizmos;
            HierarchyLines = obj.HierarchyLines;
        }

        /// <summary>
        /// Requests the utility to run through all values and update the necessary components.
        /// </summary>
        public void Update()
        {
            MeshScan = MeshScan;
            Grid = Grid;
            ElementGizmos = ElementGizmos;
            HierarchyLines = HierarchyLines;
        }
        
        /// <summary>
        /// Emits an event that states that something has changed.
        /// </summary>
        private void Notify(EditorSettingsTypes type = EditorSettingsTypes.All)
        {
            if (null != OnChanged)
            {
                OnChanged(new SettingChangedArgs(type));   
            } 
        }
    }

    /// <summary>
    /// The types of settings that may be set.
    /// </summary>
    public enum EditorSettingsTypes
    {
        MeshScan,
        Grid,
        ElementGizmos,
        HierarchyLines,
        All
    }
    
    /// <summary>
    /// The args for a settings change event.
    /// </summary>
    public class SettingChangedArgs : EventArgs
    {
        public EditorSettingsTypes Type { get; set; }
        
        public SettingChangedArgs(EditorSettingsTypes type)
        {
            Type = type;
        }
    }
}