using UnityEngine;

public class Target : MonoBehaviour
{
    private new Renderer renderer;

    private void Start() => renderer = GetComponent<Renderer>();

    private void OnMouseEnter() => renderer.material.color = Color.red;

    private void OnMouseExit() => renderer.material.color = Color.white;
}
