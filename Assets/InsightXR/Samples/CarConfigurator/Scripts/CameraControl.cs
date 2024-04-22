using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] GameObject[] cameraWaypoints;
    [SerializeField] int cameraCurrentPos = 0;
    [SerializeField] float cameraLerpSpeed = 0.4f;
    [SerializeField] float cameraRotationSpeed = 0.2f;
    Vector3 velocity = Vector3.zero;

    void OnEnable() => InputManager.OnPlayerCameraToggle += SetNextCamera;

    void OnDisable() => InputManager.OnPlayerCameraToggle -= SetNextCamera;

    void LateUpdate()
    {
        SetCameraPosition();
        SetCameraRotation();
    }

    public void SetNextCamera()
    {
        cameraCurrentPos = (cameraCurrentPos + 1) % cameraWaypoints.Length;
    }

    void SetCameraPosition()
    {
        transform.position = Vector3.SmoothDamp(transform.position, cameraWaypoints[cameraCurrentPos].transform.position, ref velocity, cameraLerpSpeed);
    }

    void SetCameraRotation()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, cameraWaypoints[cameraCurrentPos].transform.rotation, Time.deltaTime / cameraRotationSpeed);
    }
}
