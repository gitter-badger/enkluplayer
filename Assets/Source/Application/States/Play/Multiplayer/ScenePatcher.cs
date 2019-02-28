using System.Collections.Generic;
using Enklu.Data;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// This class provides function for applying scene diffs to the scene graph.
    /// </summary>
    public class ScenePatcher
    {
        /// <summary>
        /// Dependencies
        /// </summary>
        private readonly IAppSceneManager _scenes;
        private readonly IElementActionStrategyFactory _patcherFactory;

        private Element _root;
        private IElementActionStrategy _patcher;

        /// <summary>
        /// Creates a new <see cref="ScenePatcher"/> instance.
        /// </summary>
        public ScenePatcher(IAppSceneManager scenes, IElementActionStrategyFactory patcherFactory)
        {
            _scenes = scenes;
            _patcherFactory = patcherFactory;
        }

        /// <summary>
        /// Initializes the scene patcher with the root scene.
        /// </summary>
        public void Initialize()
        {
            if (_scenes.All.Length == 0)
            {
                Log.Error(this, "Tried to initialize SceneEventHandler but scene manager has no scenes!");
                return;
            }

            _root = _scenes.Root(_scenes.All[0]);
            _patcher = _patcherFactory.Instance(_root);
        }

        /// <summary>
        /// Applies the provided <see cref="ElementActionData"/> to the scene.
        /// </summary>
        public void Apply(List<ElementActionData> actions)
        {
            Log.Warning(this, "Applying {0} actions.", actions.Count);

            // TODO: Consider reconnection use-case:
            // TODO:   - Create occurs while connected
            // TODO:   - Delete happens during reconnect (and cancels create)
            for (int i = 0; i < actions.Count; ++i)
            {
                var action = actions[i];
                string error;

                switch (action.Type)
                {
                    case ElementActionTypes.CREATE:
                    {
                        if (!_patcher.ApplyCreateAction(action, out error))
                        {
                            Log.Error(this,
                                "Apply: Could not apply create action: {0}.",
                                error);
                        }
                        break;
                    }

                    case ElementActionTypes.UPDATE:
                    {
                        if (!_patcher.ApplyUpdateAction(action, out error))
                        {
                            Log.Error(this,
                                "Apply: Could not apply update action: {0}.",
                                error);
                        }

                        break;
                    }

                    case ElementActionTypes.DELETE:
                    {
                        if (!_patcher.ApplyDeleteAction(action, out error))
                        {
                            Log.Error(this,
                                "Apply: Could not apply delete action: {0}.",
                                error);
                        }

                        break;
                    }

                    case ElementActionTypes.MOVE:
                    {
                        Log.Error(this, "ScenePatcher does not support ElementActionTypes.MOVE");
                        break;
                    }

                    default:
                    {
                        Log.Error(this, "ScenePatcher does not support ElementActionType: {0}", action.Type);
                        break;
                    }
                }
            }
        }
    }
}