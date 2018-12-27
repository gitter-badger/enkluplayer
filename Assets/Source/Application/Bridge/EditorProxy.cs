using System;
using Source.Messages.ToApplication;

namespace CreateAR.EnkluPlayer
{
    public class EditorProxy
    {
        public EditorSettings Settings = new EditorSettings();
    }

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
        /// Whether the mesh scan is visible.
        /// </summary>
        public bool MeshScan
        {
            get { return _meshScan; }
            set
            {
                _meshScan = value;
                if (OnChanged != null)
                    OnChanged(new SettingChangedArgs(EditorSettingsTypes.MeshScan, _meshScan));               
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
                if (OnChanged != null)
                    OnChanged(new SettingChangedArgs(EditorSettingsTypes.Grid, _grid));               
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
                if (OnChanged != null)
                    OnChanged(new SettingChangedArgs(EditorSettingsTypes.ElementGizmos, _elementGizmos));               
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
                if (OnChanged != null)
                    OnChanged(new SettingChangedArgs(EditorSettingsTypes.HierarchyLines, _hierarchyLines));               
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