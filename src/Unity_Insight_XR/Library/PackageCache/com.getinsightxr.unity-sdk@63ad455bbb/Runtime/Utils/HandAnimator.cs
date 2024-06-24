using System;
using System.Collections;
using System.Collections.Generic;
using InsightXR.VR;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class HandAnimator : MonoBehaviour
{
    public InputActionProperty pinchAnimationAction;
    public InputActionProperty gripAnimationAction;
    private TriggerInputDetector XRinput;
    public Animator handAnimator;


    private void Start()
    {
        XRinput = FindObjectOfType<TriggerInputDetector>();

        if (handAnimator == null)
        {
            Debug.Log("Hand Animator cannot be Found");
            this.enabled = false;
        }
    }

    void Update()
    {
        handAnimator.SetFloat("Trigger", pinchAnimationAction.action.ReadValue<float>());
        handAnimator.SetFloat("Grip", gripAnimationAction.action.ReadValue<float>());
    }

    public (float, float) GetData() => (pinchAnimationAction.action.ReadValue<float>(),
        gripAnimationAction.action.ReadValue<float>());

}