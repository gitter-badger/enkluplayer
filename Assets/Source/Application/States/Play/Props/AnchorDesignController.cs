using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class AnchorDesignController : MonoBehaviour
    {
        private PlayModeConfig _config;
        private GameObject _marker;

        public WorldAnchorWidget Element { get; private set; }

        public bool IsVisualEnabled
        {
            get
            {
                return _marker.activeSelf;
            }
            set
            {
                _marker.SetActive(value);
            }
        }

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
    }
}