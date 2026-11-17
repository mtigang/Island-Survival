using UnityEngine;

namespace IslandSurvival.Environment
{
    /// <summary>
    /// Camera-side atmosphere: distance fog feel, slight color grade via camera background and light.
    /// </summary>
    public class AtmosphereController : MonoBehaviour
    {
        public Camera cam;
        public float viewDistance = 320f;

        void Start()
        {
            if (cam == null) cam = Camera.main;
            if (cam != null)
            {
                cam.farClipPlane = viewDistance;
                cam.nearClipPlane = 0.07f;
                cam.fieldOfView = 72f;
                cam.allowHDR = true;
                cam.allowMSAA = true;
            }
            QualitySettings.shadowDistance = 90f;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadowCascades = 4;
            QualitySettings.antiAliasing = 4;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            QualitySettings.softParticles = true;
            QualitySettings.lodBias = 1.2f;
            Application.targetFrameRate = 60;
        }
    }
}
