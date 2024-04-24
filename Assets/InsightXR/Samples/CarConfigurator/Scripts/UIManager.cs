using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas canvas;

    void Awake() => canvas = GetComponent<Canvas>();

    void OnEnable() => InputManager.OnPlayerHUDToggle += ToggleHUD;

    void OnDisable() => InputManager.OnPlayerHUDToggle += ToggleHUD;

    void ToggleHUD() => canvas.enabled = !canvas.enabled;
}
