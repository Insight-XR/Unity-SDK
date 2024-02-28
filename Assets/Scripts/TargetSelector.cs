using UnityEngine;

public class TargetSelector : MonoBehaviour
{
    [Header("Broadcasting to")]
    [SerializeField] private InternalEventChannel ShotEventChannel = default;
    public float forceSize;
    private new Camera      camera;
    private new Rigidbody   rigidbody;
    private void Start(){
        camera      = Camera.main;
        rigidbody   = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            var ray = camera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit raycastHit)){
                if(!(raycastHit.collider.gameObject.GetComponent<Target>() == null)){
                    Vector3 distanceToTarget = raycastHit.point - transform.position;
                    Vector3 forceDirection   = distanceToTarget.normalized;

                    rigidbody.AddForce(forceDirection * forceSize, ForceMode.Impulse);
                }
            }
            ShotEventChannel.RaiseEvent(true);
        }
    }

    public void Shoot(Vector3 position)
    {
        Vector3 distanceToTarget = position - transform.position;
        Vector3 forceDirection   = distanceToTarget.normalized;

        rigidbody.AddForce(forceDirection * forceSize, ForceMode.Impulse);
    }
}
