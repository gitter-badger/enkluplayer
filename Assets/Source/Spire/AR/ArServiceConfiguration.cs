﻿using UnityEngine;

namespace CreateAR.SpirePlayer.AR
{
    public class ArServiceConfiguration : MonoBehaviour
    {
        public bool ShowCameraFeed;
        public bool EnablePlaneDetection;
        public bool EnableLightEstimation;
        public bool EnablePointCloud;

        public Material CameraMaterial;
    }
}