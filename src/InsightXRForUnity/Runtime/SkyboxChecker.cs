using UnityEngine;
namespace InsightDesk
{
    public class SkyboxChecker : MonoBehaviour
    {
        private Material lastSkybox = null;

        void Start()
        {
            CheckCurrentSkybox();
        }

        void Update()
        {
            CheckCurrentSkybox();
        }

        void CheckCurrentSkybox()
        {
            Material currentSkybox = RenderSettings.skybox;
            if (currentSkybox != lastSkybox)
            {
                if (currentSkybox != null)
                {
                    // Debug.Log("Current Skybox: " + currentSkybox.name);
                    TrackingManagerWorker.SetSkybox(currentSkybox.name); // Set skybox in TrackingManagerWorker
                }
                else
                {
                    // Debug.Log("No skybox is currently set in the scene.");
                    TrackingManagerWorker.SetSkybox(null); // Set skybox in TrackingManagerWorker
                }
                lastSkybox = currentSkybox;
            }
        }
    }
}