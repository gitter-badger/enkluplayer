using System;
using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
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
        private ElementSchemaProp<float> _spotRangeProp;
        private ElementSchemaProp<float> _angleProp;

        /// <summary>
        /// Cached camera.
        /// </summary>
        private Camera _cam;
        
        /// <inheritdoc />
        public override void Initialize(Element element)
        {
            base.Initialize(element);

            var schema = element.Schema;
            schema.Set("visible", true);

            _typeProp = schema.Get<string>("lightType");
            _spotRangeProp = schema.Get<float>("spot.range");
            _angleProp = schema.Get<float>("spot.angle");
        }
        
        /// <summary>
        /// Called every update.
        /// </summary>
        private void LateUpdate()
        {
            if (null == _cam)
            {
                _cam = Camera.main;
            }
            
            var type = ToLightType(_typeProp.Value);
            switch (type)
            {
                case LightType.Directional:
                {
                    DrawDirectionalGizmo();
                    break;
                }
                case LightType.Point:
                {
                    DrawPointGizmo();
                    break;
                }
                case LightType.Spot:
                {
                    DrawSpotGizmo();
                    break;
                }
            }
        }

        /// <summary>
        /// Draws gizmos for directional lights.
        /// </summary>
        private void DrawDirectionalGizmo()
        {
            var handle = Render.Handle2D("Gizmo");
            if (null != handle)
            {
                // grab transform values here
                var screen = _cam.WorldToScreenPoint(transform.position);
                var forward = _cam.WorldToScreenPoint(transform.position + transform.forward);
                var right = _cam.WorldToScreenPoint(transform.position + transform.right);
                var up = _cam.WorldToScreenPoint(transform.position + transform.up);
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
                        screen + screenShootHalfDistance * screenUp + screenShootHalfDistance * screenRight +
                        screenShootLength * screenForward);
                    ctx.Line(
                        screen + screenShootHalfDistance * screenUp - screenShootHalfDistance * screenRight,
                        screen + screenShootHalfDistance * screenUp - screenShootHalfDistance * screenRight +
                        screenShootLength * screenForward);
                    ctx.Line(
                        screen - screenShootHalfDistance * screenUp - screenShootHalfDistance * screenRight,
                        screen - screenShootHalfDistance * screenUp - screenShootHalfDistance * screenRight +
                        screenShootLength * screenForward);
                    ctx.Line(
                        screen - screenShootHalfDistance * screenUp + screenShootHalfDistance * screenRight,
                        screen - screenShootHalfDistance * screenUp + screenShootHalfDistance * screenRight +
                        screenShootLength * screenForward);
                });
            }
        }

        /// <summary>
        /// Draws gizmos for point lights.
        /// </summary>
        private void DrawPointGizmo()
        {
            // TODO: Draw a sphere.
            /*
            var handle = Render.Handle2D("Gizmo");
            if (null != handle)
            {
                // grab transform values here
                var len = _pointRangeProp.Value;
                var pos = transform.position;
                var extents = new[]
                {
                    _cam.WorldToScreenPoint(pos - len * transform.forward),
                    _cam.WorldToScreenPoint(pos + len * transform.forward),

                    _cam.WorldToScreenPoint(pos - len * transform.up),
                    _cam.WorldToScreenPoint(pos + len * transform.up),

                    _cam.WorldToScreenPoint(pos - len * transform.right),
                    _cam.WorldToScreenPoint(pos + len * transform.right),

                    _cam.WorldToScreenPoint(pos - (len * transform.right + len * transform.up)),
                    _cam.WorldToScreenPoint(pos + (len * transform.right + len * transform.up)),

                    _cam.WorldToScreenPoint(pos - (len * transform.right + len * transform.forward)),
                    _cam.WorldToScreenPoint(pos + (len * transform.right + len * transform.forward)),

                    _cam.WorldToScreenPoint(pos - (len * transform.forward + len * transform.up)),
                    _cam.WorldToScreenPoint(pos + (len * transform.forward + len * transform.up))
                };
                
                handle.Draw(ctx =>
                {
                    // each extent
                    ctx.Color(Color.yellow);

                    ctx.Line(extents[0], extents[1]);
                    ctx.Line(extents[2], extents[3]);
                    ctx.Line(extents[4], extents[5]);
                    ctx.Line(extents[6], extents[7]);
                    ctx.Line(extents[8], extents[9]);
                    ctx.Line(extents[10], extents[11]);
                });
            }*/
        }

        /// <summary>
        /// Draws gizmos for spot lights.
        /// </summary>
        private void DrawSpotGizmo()
        {
            var handle = Render.Handle2D("Gizmo");
            if (null != handle)
            {
                // grab transform values here
                var pos = transform.position;
                var forward = transform.forward;
                var right = transform.right;
                var up = transform.up;

                var screen = _cam.WorldToScreenPoint(pos);

                var range = _spotRangeProp.Value;
                var angle = 90f - _angleProp.Value / 2f;
                var len = range / Mathf.Sin(Mathf.Deg2Rad * angle);
                
                var extents = new[]
                {
                    _cam.WorldToScreenPoint(pos + range * forward + len * right),
                    _cam.WorldToScreenPoint(pos + range * forward - len * right),
                    _cam.WorldToScreenPoint(pos + range * forward + len * up),
                    _cam.WorldToScreenPoint(pos + range * forward - len * up)
                };
                
                handle.Draw(ctx =>
                {
                    ctx.Color(Color.yellow);
                    ctx.Line(screen, extents[0]);
                    ctx.Line(screen, extents[1]);
                    ctx.Line(screen, extents[2]);
                    ctx.Line(screen, extents[3]);

                    ctx.Line(extents[0], extents[2]);
                    ctx.Line(extents[1], extents[2]);
                    ctx.Line(extents[3], extents[0]);
                    ctx.Line(extents[3], extents[1]);
                });
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