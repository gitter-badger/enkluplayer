using System;
using CreateAR.Commons.Unity.Logging;
using RTEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Renders a gizmo for a light.
    /// </summary>
    public class LightWidgetGizmoRenderer : MonoBehaviourGizmoRenderer
    {
        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<string> _typeProp;

        /// <summary>
        /// Cached camera.
        /// </summary>
        private Camera _cam;

        /// <summary>
        /// Renders a texture to screen.
        /// </summary>
        public GizmoTextureRenderer TextureRenderer;

        /// <inheritdoc />
        public override void Initialize(Element element)
        {
            base.Initialize(element);

            var schema = element.Schema;
            schema.Set("visible", true);

            _typeProp = schema.Get<string>("lightType");
            
            TextureRenderer.enabled = true;
        }

        /// <summary>
        /// Called on awake.
        /// </summary>
        private void Awake()
        {
            TextureRenderer.enabled = false;
        }

        /// <summary>
        /// Called every update.
        /// </summary>
        private void LateUpdate()
        {
            if (null == _cam)
            {
                var editorCam = FindObjectOfType<EditorCamera>();
                if (null == editorCam)
                {
                    return;
                }

                _cam = editorCam.GetComponent<Camera>();
            }
            
            // grab transform values here
            var screen = _cam.WorldToScreenPoint(transform.position);
            var forward = _cam.WorldToScreenPoint(transform.position + transform.forward);
            var right = _cam.WorldToScreenPoint(transform.position + transform.right);
            var up = _cam.WorldToScreenPoint(transform.position + transform.up);

            var type = ToLightType(_typeProp.Value);
            switch (type)
            {
                case LightType.Directional:
                {
                    var handle = Render.Handle2D("Gizmo");
                    if (null != handle)
                    {
                        handle.Draw(ctx =>
                        {
                            var screenForward = (forward - screen).normalized;
                            var screenRight = (right - screen).normalized;
                            var screenUp = (up - screen).normalized;
                            var screenLength = 100f;
                            
                            // main forward
                            ctx.Color(Color.yellow);
                            ctx.Line(screen, screen + screenLength * screenForward);

                            // box
                            const float screenBoxDistance = 50f;
                            const float screenBoxHalfDistance = screenBoxDistance / 2f;

                            ctx.Line(
                                screen + screenBoxHalfDistance * screenUp + screenBoxHalfDistance * screenRight,
                                screen + screenBoxHalfDistance * screenUp - screenBoxHalfDistance * screenRight);
                            ctx.Line(
                                screen + screenBoxHalfDistance * screenUp - screenBoxHalfDistance * screenRight,
                                screen - screenBoxHalfDistance * screenUp - screenBoxHalfDistance * screenRight);
                            ctx.Line(
                                screen - screenBoxHalfDistance * screenUp - screenBoxHalfDistance * screenRight,
                                screen - screenBoxHalfDistance * screenUp + screenBoxHalfDistance * screenRight);
                            ctx.Line(
                                screen - screenBoxHalfDistance * screenUp + screenBoxHalfDistance * screenRight,
                                screen + screenBoxHalfDistance * screenUp + screenBoxHalfDistance * screenRight);

                            // shoots
                            const float screenShootDistance = 40f;
                            const float screenShootHalfDistance = screenShootDistance / 2f;
                            const float screenShootLength = 75f;

                            ctx.Line(
                                screen + screenShootHalfDistance * screenUp + screenShootHalfDistance * screenRight,
                                screen + screenShootHalfDistance * screenUp + screenShootHalfDistance * screenRight + screenShootLength * screenForward);
                            ctx.Line(
                                screen + screenShootHalfDistance * screenUp - screenShootHalfDistance * screenRight,
                                screen + screenShootHalfDistance * screenUp - screenShootHalfDistance * screenRight + screenShootLength * screenForward);
                            ctx.Line(
                                screen - screenShootHalfDistance * screenUp - screenShootHalfDistance * screenRight,
                                screen - screenShootHalfDistance * screenUp - screenShootHalfDistance * screenRight + screenShootLength * screenForward);
                            ctx.Line(
                                screen - screenShootHalfDistance * screenUp + screenShootHalfDistance * screenRight,
                                screen - screenShootHalfDistance * screenUp + screenShootHalfDistance * screenRight + screenShootLength * screenForward);
                        });
                    }
                    break;
                }
                case LightType.Point:
                {
                    Log.Info(this, "Point");
                        break;
                }
                case LightType.Spot:
                {
                    Log.Info(this, "Spot");
                    break;
                }
            }
        }
        
        /// <summary>
        /// Parses light type.
        /// </summary>
        /// <param name="value">String value to parse.</param>
        /// <returns></returns>
        private static LightType ToLightType(string value)
        {
            try
            {
                return (LightType) Enum.Parse(typeof(LightType), value);
            }
            catch
            {
                return LightType.Directional;
            }
        }
    }
}