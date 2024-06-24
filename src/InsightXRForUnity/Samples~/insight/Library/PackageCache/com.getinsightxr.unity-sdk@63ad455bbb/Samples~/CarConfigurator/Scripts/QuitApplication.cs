using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuitApplication : MonoBehaviour
{
#if UNITY_STANDALONE
    [SerializeField] private TMP_Text exitButtonText;
#endif

    public void Awake()
    {
#if UNITY_EDITOR || UNITY_WEBGL
        gameObject.SetActive(false);
#endif
#if UNITY_STANDALONE_WIN
        exitButtonText.text = "Exit to Windows";
#elif UNITY_STANDALONE_OSX
        exitButtonText.text = "Exit to macOS";
#elif UNITY_STANDALONE_LINUX
        exitButtonText.text = "Exit to Linux";
#endif
    }

#if UNITY_STANDALONE
    public void QuitToDesktop()
    {
        Application.Quit();
    }
#endif
}
