using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadScript : MonoBehaviour
{
    Material mMaterial;
    MeshRenderer mMeshRenderer;

    float[] mPoints;
    int mHitCount;

    float mDelay;


    void Start()
    {
        mDelay = 3;

        mMeshRenderer = GetComponent<MeshRenderer>();
        mMaterial = mMeshRenderer.material;

        mPoints = new float[32 * 3]; //32 point 

    }

    void Update()
    {


    }

    /*private void OnCollisionEnter(Collision collision)
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
            Destroy(cp.otherCollider.gameObject);
        }
    }*/
    /*public void checkray(Vector3 hitray,Vector3 normal)
    {

        Vector3 StartOfRay = hitray - normal;
        Vector3 RayDir = normal;

        Ray ray = new Ray(StartOfRay, RayDir);
        RaycastHit hit;

        bool hitit = Physics.Raycast(ray, out hit, 10f, LayerMask.GetMask("HeatMapLayer"));

        if (hitit)
        {
            Debug.Log("Hit Object " + hit.collider.gameObject.name);
            Debug.Log("Hit Texture coordinates = " + hit.textureCoord.x + "," + hit.textureCoord.y);
            addHitPoint(hit.textureCoord.x * 4 - 2, hit.textureCoord.y * 4 - 2);
        }
        else
       {
           Debug.Log("error");
       }
    }*/
    public Vector2 CalculateTextureCoordinateAtPoint(Vector3 point)
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = mesh.uv;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 vertex1 = vertices[triangles[i]];
            Vector3 vertex2 = vertices[triangles[i + 1]];
            Vector3 vertex3 = vertices[triangles[i + 2]];

            if (IsPointInTriangle(point, vertex1, vertex2, vertex3))
            {
                // Calculate the barycentric coordinates of the point
                float u, v, w;
                Barycentric(point, vertex1, vertex2, vertex3, out u, out v, out w);

                // Interpolate UV coordinates
                Vector2 uv1 = uvs[triangles[i]];
                Vector2 uv2 = uvs[triangles[i + 1]];
                Vector2 uv3 = uvs[triangles[i + 2]];

                Vector2 interpolatedUV = uv1 * u + uv2 * v + uv3 * w;
                Debug.Log(interpolatedUV);
                addHitPoint(interpolatedUV.x * 4 - 2, interpolatedUV.y * 4 - 2);
                return interpolatedUV;
            }
        }

        return Vector2.zero; // Return zero if the point is not on any triangle
    }

    private bool IsPointInTriangle(Vector3 point, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        // Compute vectors
        Vector3 v0 = vertex3 - vertex1;
        Vector3 v1 = vertex2 - vertex1;
        Vector3 v2 = point - vertex1;

        // Compute dot products
        float dot00 = Vector3.Dot(v0, v0);
        float dot01 = Vector3.Dot(v0, v1);
        float dot02 = Vector3.Dot(v0, v2);
        float dot11 = Vector3.Dot(v1, v1);
        float dot12 = Vector3.Dot(v1, v2);

        // Compute barycentric coordinates
        float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        // Check if point is in triangle
        return (u >= 0) && (v >= 0) && (u + v <= 1);
    }

    private void Barycentric(Vector3 p, Vector3 a, Vector3 b, Vector3 c, out float u, out float v, out float w)
    {
        Vector3 v0 = b - a;
        Vector3 v1 = c - a;
        Vector3 v2 = p - a;

        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);

        float denom = d00 * d11 - d01 * d01;
        v = (d11 * d20 - d01 * d21) / denom;
        w = (d00 * d21 - d01 * d20) / denom;
        u = 1.0f - v - w;
    }
    public void addHitPoint(float xp, float yp)
    {
        mPoints[mHitCount * 3] = xp;
        mPoints[mHitCount * 3 + 1] = yp;
        mPoints[mHitCount * 3 + 2] = Random.Range(1f, 3f);

        mHitCount++;
        mHitCount %= 32;

        mMaterial.SetFloatArray("_Hits", mPoints);
        mMaterial.SetInt("_HitCount", mHitCount);

    }

}
