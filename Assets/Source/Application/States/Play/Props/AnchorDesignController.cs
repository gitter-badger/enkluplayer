using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class AnchorDesignController : MonoBehaviour
    {
        private PlayModeConfig _config;
        private GameObject _marker;

        public WorldAnchorWidget Element { get; private set; }

        public void Initialize(
            PlayModeConfig config,
            WorldAnchorWidget element)
        {
            _config = config;

            Element = element;

            _marker = Instantiate(_config.AnchorPrefab, transform);
            _marker.transform.position = Vector3.zero;
            _marker.transform.localRotation = Quaternion.identity;
        }

        private void OnEnable()
        {
            if (null != _marker)
            {
                _marker.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if (null != _marker)
            {
                _marker.SetActive(false);
            }
        }
    }
}