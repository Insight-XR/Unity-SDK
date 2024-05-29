using InsightXR.Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadScript : MonoBehaviour
{
    [SerializeField]
    private DataHandleLayer dataHandleLayer;

    Material mMaterial;
    MeshRenderer mMeshRenderer;

    [SerializeField]
    private Transform vrDevice;

    float[] mPoints;
    public int mHitCount;

    float mDelay;

    private void Awake()
    {
        vrDevice = FindObjectOfType<Camera>().transform;
    }

    void Start()
    {
        mDelay = 3;
        mMeshRenderer = GetComponent<MeshRenderer>();
        mMaterial = mMeshRenderer.material;
        mPoints = new float[1000 * 3];
    }

    void Update()
    {
        if (dataHandleLayer.replay)
        {
            GetComponent<QuadScript>().enabled = false;
            return; 
        }

        mDelay -= Time.deltaTime;
        if (mDelay <= 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(vrDevice.position, vrDevice.forward, out hit, 100f))
            {
                addHitPoint(hit.textureCoord.x * 4 - 2, hit.textureCoord.y * 4 - 2);
                mDelay = .1f;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint cp in collision.contacts)
        {
            Debug.Log("Contact with object " + cp.otherCollider.gameObject.name);

            Vector3 StartOfRay = cp.point - cp.normal;
            Vector3 RayDir = cp.normal;

            Ray ray = new Ray(StartOfRay, RayDir);
            RaycastHit hit;

            bool hitit = Physics.Raycast(ray, out hit, 10f, LayerMask.GetMask("HeatMapLayer"));

            if (hitit)
            {
                Debug.Log("Hit Object " + hit.collider.gameObject.name);
                Debug.Log("Hit Texture coordinates = " + hit.textureCoord.x + "," + hit.textureCoord.y);
                addHitPoint(hit.textureCoord.x * 4 - 2, hit.textureCoord.y * 4 - 2);
            }
        }
    }

    public void addHitPoint(float xp, float yp)
    {
        int index = mHitCount * 3;
        mPoints[index] = xp;
        mPoints[index + 1] = yp;
        mPoints[index + 2] = Random.Range(1f, 3f);

        mHitCount++;
        mHitCount %= 1000;

        mMaterial.SetFloatArray("_Hits", mPoints);
        mMaterial.SetInt("_HitCount", mHitCount);
    }
}
