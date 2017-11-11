using UnityEngine;

namespace CreateAR.SpirePlayer.AR
{
    public class ArDebugRenderer : InjectableMonoBehaviour
    {
        [Inject]
        public IArService Ar { get; set; }
        
        private void Update()
        {
            if (null == Ar.Config || !Ar.Config.DrawPlanes)
            {
                return;
            }
            
            var handle = Render.Handle("ar");
            if (null != handle)
            {
                handle.Draw(ctx =>
                {
                    ctx.Color(Color.red);

                    foreach (var plane in Ar.Anchors)
                    {
                        ctx.Prism(new Bounds(
                            plane.Center,
                            new Vector3(
                                plane.Extents.x,
                                .1f,
                                plane.Extents.z)));
                    }
                });
            }
        }
    }
}