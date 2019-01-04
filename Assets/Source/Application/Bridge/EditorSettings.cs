using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using RLD;
using Source.Messages.ToApplication;
using Object = UnityEngine.Object;

namespace CreateAR.EnkluPlayer
{
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
                
                foreach (var id in all)
                {
                    var root = Scenes.Root(id);
                    root.Find("..(@type==ScanWidget)", scans);
                    
                    foreach (var scan in scans)
                    {
                        scan.Schema.Set("visible", _meshScan);
                    }
                }

                if (null != OnChanged)
                {
                    OnChanged(new SettingChangedArgs(EditorSettingsTypes.MeshScan, _meshScan));                            
                }
                               
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
                
                if (null != OnChanged)
                {
                    OnChanged(new SettingChangedArgs(EditorSettingsTypes.Grid, _grid));
                }
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
                
                if (null != OnChanged)
                {
                    OnChanged(new SettingChangedArgs(EditorSettingsTypes.ElementGizmos, _elementGizmos));   
                }               
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
                
                if (null != OnChanged)
                {
                    OnChanged(new SettingChangedArgs(EditorSettingsTypes.HierarchyLines, _hierarchyLines));   
                }               
            }
        }

//        public EditorSettings(IAppSceneManager scenes)
//        {
//            _scenes = scenes;
//        }

        /// <summary>
        /// Sets any setting.
        /// </summary>
        public void SetSetting(string name, bool value)
        {
            Log.Info(this, "Setting {0} to {1}", name, value);
            switch (name)
            {
                case "MeshScan":
                    MeshScan = value;
                    break;
                
                case "Grid":
                    Grid = value;
                    break;
                
                case "ElementGizmos":
                    ElementGizmos = value;
                    break;
                
                case "HierarchyLines":
                    HierarchyLines = value;
                    break;

                default:
                {
                    Log.Warning(this, "Could not set setting {0}. No such property.", name);
                    break;
                }
            }
        }
        
        public void PopulateFromEvent(EditorSettingsEvent obj)
        {
            MeshScan = obj.MeshScan;
            Grid = obj.Grid;
            ElementGizmos = obj.ElementGizmos;
            HierarchyLines = obj.HierarchyLines;
        }
    }

    public enum EditorSettingsTypes
    {
        MeshScan,
        Grid,
        ElementGizmos,
        HierarchyLines
    }
    
    public class SettingChangedArgs : EventArgs
    {
        /// <summary>
        /// Backing variable for Type.
        /// </summary>
        private EditorSettingsTypes _type;

        /// <summary>
        /// Backing variable for Value.
        /// </summary>
        private bool _value;
        
        public SettingChangedArgs(EditorSettingsTypes type, bool value)
        {
            _type = type;
            _value = value;
        }

        public EditorSettingsTypes Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public bool Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}