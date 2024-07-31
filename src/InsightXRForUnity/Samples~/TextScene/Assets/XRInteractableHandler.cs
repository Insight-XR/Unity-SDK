using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class XRInteractableHandler : MonoBehaviour
{
    public TextMeshProUGUI textComponent;

    private void OnEnable()
    {
        var interactable = GetComponent<XRBaseInteractable>();
        interactable.selectEntered.AddListener(OnSphereSelected);
    }

    private void OnDisable()
    {
        var interactable = GetComponent<XRBaseInteractable>();
        interactable.selectEntered.RemoveListener(OnSphereSelected);
    }

    private void OnSphereSelected(SelectEnterEventArgs args)
    {
        if (textComponent != null)
        {
            textComponent.text = "Welcome to InsightXR";
            Debug.Log("Text updated to 'Welcome to InsightXE'");
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI component is not assigned");
        }
    }
}
