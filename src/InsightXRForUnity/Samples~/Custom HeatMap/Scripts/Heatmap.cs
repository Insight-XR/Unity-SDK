using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Heatmap : MonoBehaviour
{
    public Texture baseTexture;
    public Material meshMaterial;
    public GameObject meshGameobject;
    public Shader UVShader;
    public Mesh meshToDraw;
    public Shader ilsandMarkerShader;
    public Shader fixIlsandEdgesShader;
    public static Vector3 mouseWorldPosition;

   [SerializeField] private Camera mainC;
    private RenderTexture markedIlsandes;
    private CommandBuffer cb_markingIlsdands;
    private int numberOfFrames;

    private HeatmapTexture albedo;
    public List<GameObject> hittedGameObject;

    void Start()
    {
        mainC = Camera.main;
        if (mainC == null) mainC = this.GetComponent<Camera>();
        if (mainC == null) mainC = GameObject.FindObjectOfType<Camera>();

        TextureAndMaterial();
    }

    private void TextureAndMaterial()
    {
        markedIlsandes = new RenderTexture(baseTexture.width, baseTexture.height, 0, RenderTextureFormat.R8);
        albedo = new HeatmapTexture(Color.white, baseTexture.width, baseTexture.height, "_MainTex", UVShader, meshToDraw, fixIlsandEdgesShader, markedIlsandes, baseTexture);

        meshMaterial.SetTexture(albedo.id, albedo.runTimeTexture);

        cb_markingIlsdands = new CommandBuffer();
        cb_markingIlsdands.name = "markingIlsnads";

        cb_markingIlsdands.SetRenderTarget(markedIlsandes);
        Material mIlsandMarker = new Material(ilsandMarkerShader);
        cb_markingIlsdands.DrawMesh(meshToDraw, Matrix4x4.identity, mIlsandMarker);

        mainC.RemoveAllCommandBuffers();
        mainC.AddCommandBuffer(CameraEvent.AfterDepthTexture, cb_markingIlsdands);

        albedo.SetActiveTexture(mainC);
    }

    private void Update()
    {
        if (numberOfFrames > 2) mainC.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, cb_markingIlsdands);

        numberOfFrames++;

        albedo.UpdateShaderParameters(meshGameobject.transform.localToWorldMatrix);

        RaycastHit hit;
        Ray ray = new Ray(mainC.transform.position, mainC.transform.forward);
        Vector4 mwp = Vector3.positiveInfinity;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.tag == "PaintObject")
            {
                if (hit.collider.gameObject != meshGameobject)
                {
                    meshGameobject = hit.collider.gameObject;
                    meshMaterial = meshGameobject.GetComponent<Renderer>().material;
                    if (!hittedGameObject.Contains(meshGameobject))
                    {
                        hittedGameObject.Add(meshGameobject);
                        Texture defaultTexture = meshMaterial.mainTexture;
                        StartCoroutine(RevertPaint(meshMaterial, defaultTexture, meshGameobject));
                    }
                    if (meshMaterial.mainTexture != null)
                    {
                        baseTexture = meshMaterial.mainTexture;
                    }
                    meshToDraw = meshGameobject.GetComponent<MeshFilter>().mesh;
                    TextureAndMaterial();
                }

                mwp = hit.point;
            }
        }

        mwp.w = Input.GetMouseButton(0) ? 1 : 0;

        mouseWorldPosition = mwp;
        Shader.SetGlobalVector("_Mouse", mwp);
    }

    Color GetHeatmapColor(float value, float min, float max)
    {
        value = Mathf.Clamp((value - min) / (max - min), 0, 1);
        Color coldColor = Color.blue;
        Color hotColor = Color.red;

        return Color.Lerp(coldColor, hotColor, value);
    }
    IEnumerator RevertPaint(Material mat, Texture baseTexture, GameObject meshGameObject)
    {
        yield return new WaitForSeconds(5);
        mat.SetTexture("_MainTex", baseTexture);
        hittedGameObject.Remove(meshGameObject);
    }
}

[System.Serializable]
public class HeatmapTexture
{
    public string id;
    public RenderTexture runTimeTexture;
    public RenderTexture paintedTexture;

    public CommandBuffer cb;

    private Material mPaintInUV;
    private Material mFixedEdges;
    private RenderTexture fixedIlsands;

    public HeatmapTexture(Color clearColor, int width, int height, string id, Shader sPaintInUV, Mesh mToDraw, Shader fixIlsandEdgesShader, RenderTexture markedIlsandes, Texture initialTexture)
    {
        this.id = id;

        runTimeTexture = new RenderTexture(width, height, 0)
        {
            anisoLevel = 0,
            useMipMap = false,
            filterMode = FilterMode.Bilinear
        };

        paintedTexture = new RenderTexture(width, height, 0)
        {
            anisoLevel = 0,
            useMipMap = false,
            filterMode = FilterMode.Bilinear
        };

        Graphics.Blit(initialTexture, runTimeTexture);
        Graphics.Blit(initialTexture, paintedTexture);

        fixedIlsands = new RenderTexture(paintedTexture.descriptor);

        mPaintInUV = new Material(sPaintInUV);
        mPaintInUV.SetTexture("_MainTex", paintedTexture);

        mFixedEdges = new Material(fixIlsandEdgesShader);
        mFixedEdges.SetTexture("_IlsandMap", markedIlsandes);
        mFixedEdges.SetTexture("_MainTex", paintedTexture);

        cb = new CommandBuffer();
        cb.name = "TexturePainting" + id;

        cb.SetRenderTarget(runTimeTexture);
        cb.DrawMesh(mToDraw, Matrix4x4.identity, mPaintInUV);

        cb.Blit(runTimeTexture, fixedIlsands, mFixedEdges);
        cb.Blit(fixedIlsands, runTimeTexture);
        cb.Blit(runTimeTexture, paintedTexture);
    }

    public void SetActiveTexture(Camera mainC)
    {
        mainC.AddCommandBuffer(CameraEvent.AfterDepthTexture, cb);
    }

    public void SetInactiveTexture(Camera mainC)
    {
        mainC.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, cb);
    }

    public void UpdateShaderParameters(Matrix4x4 localToWorld)
    {
        mPaintInUV.SetMatrix("mesh_Object2World", localToWorld);
    }
}
