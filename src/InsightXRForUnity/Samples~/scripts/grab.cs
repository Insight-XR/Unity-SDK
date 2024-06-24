using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VRObjectManipulation : MonoBehaviour
{
    private XRGrabInteractable interactable; // Reference to XRGrabInteractable component
    private bool isObjectHeld = false; // Flag to check if the object is currently held

    private void Start()
    {
        // Try to get XRGrabInteractable component
        interactable = GetComponent<XRGrabInteractable>();

        // If the object doesn't have XRGrabInteractable component, add it
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRGrabInteractable>();
        }

        // Subscribe to the select entering event
        interactable.onSelectEntered.AddListener(OnGrab);
        
        // Subscribe to the select exiting event
        interactable.onSelectExited.AddListener(OnRelease);
    }

    private void OnGrab(XRBaseInteractor interactor)
    {
        isObjectHeld = true; // Set the flag to true when the object is grabbed
    }

    private void OnRelease(XRBaseInteractor interactor)
    {
        isObjectHeld = !isObjectHeld; // Toggle the flag when the object is released
    }

    private void Update()
    {
        if (isObjectHeld) // If the object is currently held
        {
            MoveObject(); // Move the object to the position and rotation of its parent
        }
    }

    private void MoveObject()
    {
        if (transform.parent != null) // If the object has a parent
        {
            // Move the object to the position and rotation of its parent
            transform.position = transform.parent.position;
            transform.rotation = transform.parent.rotation;
        }
    }
}
