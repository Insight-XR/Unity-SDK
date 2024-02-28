using Unity.VisualScripting;
using UnityEngine;

public class Target : MonoBehaviour
{
    private new Renderer renderer;
    public GameObject ball;
    

    
    private void Start() => renderer = GetComponent<Renderer>();

    private void OnMouseEnter() => renderer.material.color = Color.red;

    private void OnMouseExit() => renderer.material.color = Color.white;

    public void OnHover()
    {
        renderer.material.color = Color.red;
    }

    public void OnExit()
    {
        renderer.material.color = Color.white;
    }

    public void Shootball()
    {
        //Honestly, using a reference is better than doing get component....
        ball.GetComponent<TargetSelector>().Shoot(transform.position);
    }
}
