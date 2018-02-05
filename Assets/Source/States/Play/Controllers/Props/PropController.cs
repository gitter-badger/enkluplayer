using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class PropController : MonoBehaviour
    {
        private const float POSITION_EPSILON = 0.1f;
        private const float ROTATION_EPSILON = 0.1f;
        private const float SCALE_EPSILON = 0.1f;

        private IPropUpdateDelegate _delegate;

        public PropData Data { get; private set; }
        public ContentWidget Content { get; private set; }

        public void Initialize(
            PropData data,
            ContentWidget content,
            IPropUpdateDelegate @delegate)
        {
            Data = data;
            Content = content;

            _delegate = @delegate;
        }

        public void Uninitialize()
        {
            Data = null;
            Content = null;

            _delegate = null;
        }

        public void Resync(PropData data)
        {
            // force prop back into alignment with data
        }

        private void Update()
        {
            if (null == Data)
            {
                return;
            }

            var isDirty = false;
            var trans = Content.GameObject.transform;

            // check for position changes
            {
                if (!trans.position.Approximately(
                    Data.Position.ToVector(),
                    POSITION_EPSILON))
                {
                    Data.Position = trans.position.ToVec();

                    isDirty = true;
                }
            }

            // check for rotation changes
            {
                if (!trans.rotation.eulerAngles.Approximately(
                    Data.Rotation.ToVector(),
                    ROTATION_EPSILON))
                {
                    Data.Rotation = trans.rotation.eulerAngles.ToVec();

                    isDirty = true;
                }
            }

            // check for scale changes
            {
                if (!trans.localScale.Approximately(
                    Data.Scale.ToVector(),
                    SCALE_EPSILON))
                {
                    Data.Scale = trans.localScale.ToVec();

                    isDirty = true;
                }
            }

            if (isDirty)
            {
                _delegate.Update(Data);
            }
        }
    }
}