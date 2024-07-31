using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimatorController : MonoBehaviour
{
    [SerializeField] InputActionProperty inputTriggerAction;

    [SerializeField] InputActionProperty inputGripAction;

    [SerializeField]
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        float triggerVal = inputTriggerAction.action.ReadValue<float>();

        float gripVal = inputGripAction.action.ReadValue<float>();

        anim.SetFloat("Trigger", triggerVal);
        anim.SetFloat("Grip", gripVal);

    }
}