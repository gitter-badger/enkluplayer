using UnityEngine;

namespace CreateAR.SpirePlayer.Scripting
{
    /// <summary>
    /// Represents the user, for JsApi libraries to tie into. Currently not exported for scripts to use.
    /// </summary>
    public class PlayerJs : InjectableMonoBehaviour, IEntityJs
    {
        /// <summary>
        /// Underlying Dummy <see cref="IElementTransformJsApi"/> implementation. Values are populated from Unity's transform.
        /// </summary>
        private class PlayerTransformJsApi : IElementTransformJsApi
        {
            /// <summary>
            /// Position.
            /// </summary>
            public Vec3 position { get; set; }

            /// <summary>
            /// Rotation.
            /// </summary>
            public Quat rotation { get; set; }

            /// <summary>
            /// Scale.
            /// </summary>
            public Vec3 scale { get; set; }
        }

        /// <summary>
        /// The transform interface.
        /// </summary>
        public new IElementTransformJsApi transform { get; private set; }

        /// <summary>
        /// Called by Unity. Basic setup.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            transform = new PlayerTransformJsApi();
        }

        /// <summary>
        /// Called by Unity. Responsible for syncing Unity's transform with our <see cref="PlayerTransformJsApi"/>.
        /// </summary>
        protected void Update()
        {
            Vector3 position = gameObject.transform.position;
            Quaternion rotation = gameObject.transform.rotation;
            Vector3 scale = gameObject.transform.localScale;

            transform.position.Set(position.x, position.y, position.z);
            transform.rotation.Set(rotation.x, rotation.y, rotation.z, rotation.w);
            transform.scale.Set(scale.x, scale.y, scale.z);
        }
    }
}