using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static event Action OnPlayerCameraToggle;
    public static event Action OnPlayerHUDToggle;

    void Update()
    {
        GetInputs();
    }

    void GetInputs()
    {
        if (Input.GetKeyDown(KeyCode.C))
            OnPlayerCameraToggle?.Invoke();

        if (Input.GetKeyDown(KeyCode.H))
            OnPlayerHUDToggle?.Invoke();
    }
}
