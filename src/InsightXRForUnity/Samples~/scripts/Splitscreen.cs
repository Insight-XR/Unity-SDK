using UnityEngine;

public class SplitScreen : MonoBehaviour
{
    public Camera vrCamera;
    public Camera thirdPersonCamera;
    public Camera topDownCamera;

    void Start()
    {
        // Set VR camera to render to the left half of the screen
        vrCamera.rect = new Rect(0, 0, 1.0f, 0.6f);

        // Set third-person camera to render in the top-right corner of the screen
        thirdPersonCamera.rect = new Rect(0.6f, 0.6f, 0.4f, 0.4f);

        // Set top-down camera to render in the bottom-right corner of the screen
        topDownCamera.rect = new Rect(0.0f, 0.6f, 0.4f, 0.4f);
    }
}
