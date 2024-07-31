using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ReturnToOrigin : MonoBehaviour
{
    [SerializeField] private Pose originPose;

    private XRGrabInteractable grabinteractable;

    private void Awake()
    {
        grabinteractable = GetComponent<XRGrabInteractable>();
        originPose.position = transform.position;
        originPose.rotation = transform.rotation;
    }

    private void OnEnable()
    {
        grabinteractable.selectExited.AddListener(LaserGunReleased);
    }

    private void OnDisable()
    {
        grabinteractable.selectExited.RemoveListener(LaserGunReleased);
    }

    private void LaserGunReleased(SelectExitEventArgs arg0)
    {
        transform.position = originPose.position;
        transform.rotation = originPose.rotation; 
    }
}
