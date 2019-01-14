using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using RLD;
using CreateAR.EnkluPlayer;
using Source.Player.IUX;
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
        /// Injected reference to schema defaults, used to set initial values.
        /// </summary>
        private readonly ElementSchemaDefaults _elementSchemaDefaults;
        
        /// <summary>
        /// The current settings.
        /// </summary>
        private readonly Dictionary<EditorSettingsType, bool> _settings = new Dictionary<EditorSettingsType, bool>
        {
            {EditorSettingsType.Grid, true},
            {EditorSettingsType.MeshScan, true},
            {EditorSettingsType.ElementGizmos, true},
            {EditorSettingsType.HierarchyLines, true}
        };
        
        /// <summary>
        /// Fired when the value of any setting has changed.
        /// </summary>
        public event Action<EditorSettingsType> OnChanged;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="elementSchemaDefaults">Injected ElementSchemaDefaults.</param>
        public EditorSettings(ElementSchemaDefaults elementSchemaDefaults)
        {
            _elementSchemaDefaults = elementSchemaDefaults;
        }

        /// <summary>
        /// Sets any settings, via string keys.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="value">The new value of the setting.</param>
        public void Set(string name, bool value)
        {
            Log.Info(this, "Setting {0} to {1}", name, value);
            var type = EnumExtensions.Parse(name, EditorSettingsType.None);
            if (type != EditorSettingsType.None)
            {
                Set(type, value);
            }
        }

        /// <summary>
        /// Sets any setting, via enum.
        /// </summary>
        /// <param name="type">The type of setting.</param>
        /// <param name="value">The new value of the setting.</param>
        public void Set(EditorSettingsType type, bool value)
        {
            _settings[type] = value;
            Update(type);
        }

        /// <summary>
        /// Gets the value of a setting.
        /// </summary>
        /// <param name="type">The type of setting to get.</param>
        /// <returns>The value of the setting, or false if the setting does not exist.</returns>
        public bool Get(EditorSettingsType type)
        {
            bool value;
            _settings.TryGetValue(type, out value);
            return value;
        }

        /// <summary>
        /// Populates settings from an EditorSettingsEvent
        /// </summary>
        /// <param name="obj">The event in question</param>
        public void Populate(EditorSettingsEvent obj)
        {
            Set(EditorSettingsType.MeshScan, obj.MeshScan);
            Set(EditorSettingsType.Grid, obj.Grid);
            Set(EditorSettingsType.ElementGizmos, obj.ElementGizmos);
            Set(EditorSettingsType.HierarchyLines, obj.HierarchyLines);
        }

        /// <summary>
        /// Requests the utility to run through all values and update the necessary components.
        /// </summary>
        public void Update(EditorSettingsType type = EditorSettingsType.All)
        {
            // Grid.
            var sceneGrid = Object.FindObjectOfType<RTSceneGrid>();
            if (null != sceneGrid)
            {
                sceneGrid.Settings.IsVisible = Get(EditorSettingsType.Grid);
            }
            
            // Scan.
            if (_elementSchemaDefaults.Has(ElementTypes.SCAN))
            {
                var schema = _elementSchemaDefaults.Get(ElementTypes.SCAN);
                schema.Set("visible", Get(EditorSettingsType.MeshScan));
            }
            
            // ElementGizmos
            var scene = Object.FindObjectOfType<RTScene>();
            var elementGizmos = Get(EditorSettingsType.ElementGizmos);
            if (null != scene)
            {
                scene.LookAndFeel.DrawLightIcons = elementGizmos;
                scene.LookAndFeel.DrawParticleSystemIcons = elementGizmos;
            }

            var gizmos = Object.FindObjectOfType<GizmoManager>();
            if (null != gizmos)
            {
                gizmos.IsVisible = elementGizmos;
            }
            
            // HierarchyLines
            var renderer = Object.FindObjectOfType<HierarchyLineRenderer>();
            if (null != renderer)
            {
                renderer.enabled = Get(EditorSettingsType.HierarchyLines);
            }
            
            if (null != OnChanged)
            {
                OnChanged(type);   
            }
        }
    }

    /// <summary>
    /// The types of settings that may be set.
    /// </summary>
    public enum EditorSettingsType
    {
        None,
        MeshScan,
        Grid,
        ElementGizmos,
        HierarchyLines,
        All
    }
}