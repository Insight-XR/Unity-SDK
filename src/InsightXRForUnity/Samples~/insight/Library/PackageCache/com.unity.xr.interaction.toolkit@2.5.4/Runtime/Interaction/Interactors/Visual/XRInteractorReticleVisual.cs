using System;
using Unity.XR.CoreUtils;
using Unity.Collections;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Interactor helper object that draws a targeting <see cref="reticlePrefab"/> over a ray casted point in front of the Interactor.
    /// </summary>
    /// <remarks>
    /// When attached to an <see cref="XRRayInteractor"/>, the <see cref="XRRayInteractor.TryGetCurrentRaycast"/> 
    /// method will be used instead of the internal ray cast function of this behavior.
    /// </remarks>
    [AddComponentMenu("XR/Visual/XR Interactor Reticle Visual", 11)]
    [DisallowMultipleComponent]
    [HelpURL(XRHelpURLConstants.k_XRInteractorReticleVisual)]
    public class XRInteractorReticleVisual : MonoBehaviour
    {
        const int k_MaxRaycastHits = 10;

        [SerializeField]
        float m_MaxRaycastDistance = 10f;
        /// <summary>
        /// The max distance to Raycast from this Interactor.
        /// </summary>
        public float maxRaycastDistance
        {
            get => m_MaxRaycastDistance;
            set => m_MaxRaycastDistance = value;
        }

        [SerializeField]
        GameObject m_ReticlePrefab;
        /// <summary>
        /// Prefab which Unity draws over Raycast destination.
        /// </summary>
        public GameObject reticlePrefab
        {
            get => m_ReticlePrefab;
            set
            {
                m_ReticlePrefab = value;
                SetupReticlePrefab();
            }
        }

        [SerializeField]
        float m_PrefabScalingFactor = 1f;
        /// <summary>
        /// Amount to scale prefab (before applying distance scaling).
        /// </summary>
        public float prefabScalingFactor
        {
            get => m_PrefabScalingFactor;
            set => m_PrefabScalingFactor = value;
        }

        [SerializeField]
        bool m_UndoDistanceScaling = true;
        /// <summary>
        /// Whether Unity undoes the apparent scale of the prefab by distance.
        /// </summary>
        public bool undoDistanceScaling
        {
            get => m_UndoDistanceScaling;
            set => m_UndoDistanceScaling = value;
        }

        [SerializeField]
        bool m_AlignPrefabWithSurfaceNormal = true;
        /// <summary>
        /// Whether Unity aligns y-axis of the prefab to the ray casted surface normal. On non-horizontal surfaces this
        /// will use the xrOrigin.up to align the z-axis of the prefab. On horizontal surfaces this will use the interactor
        /// forward vector to align the z-axis of the prefab.
        /// </summary>
        /// <remarks>
        /// If xrOrigin is null it will default to Vector3.up to align the z-axis of the prefab.
        /// </remarks>
        public bool alignPrefabWithSurfaceNormal
        {
            get => m_AlignPrefabWithSurfaceNormal;
            set => m_AlignPrefabWithSurfaceNormal = value;
        }

        [SerializeField]
        float m_EndpointSmoothingTime = 0.02f;
        /// <summary>
        /// Smoothing time for endpoint.
        /// </summary>
        public float endpointSmoothingTime
        {
            get => m_EndpointSmoothingTime;
            set => m_EndpointSmoothingTime = value;
        }

        [SerializeField]
        bool m_DrawWhileSelecting;
        /// <summary>
        /// Whether Unity draws the <see cref="reticlePrefab"/> while selecting an Interactable.
        /// </summary>
        public bool drawWhileSelecting
        {
            get => m_DrawWhileSelecting;
            set => m_DrawWhileSelecting = value;
        }

        [SerializeField]
        bool m_DrawOnNoHit;        
        /// <summary>
        /// Whether Unity draws the <see cref="reticlePrefab"/> when there is no hit. If <see langword="true"/>, Unity will draw the <see cref="reticlePrefab"/>
        /// at the last point of a <see cref="XRRayInteractor"/>.
        /// </summary>
        public bool drawOnNoHit
        {
            get => m_DrawOnNoHit;
            set => m_DrawOnNoHit = value;
        }

        [SerializeField]
        LayerMask m_RaycastMask = -1;
        /// <summary>
        /// Layer mask for ray cast.
        /// </summary>
        public LayerMask raycastMask
        {
            get => m_RaycastMask;
            set => m_RaycastMask = value;
        }

        bool m_ReticleActive;
        /// <summary>
        /// Whether the reticle is currently active.
        /// </summary>
        public bool reticleActive
        {
            get => m_ReticleActive;
            set
            {
                m_ReticleActive = value;
                if (m_ReticleInstance != null)
                    m_ReticleInstance.SetActive(value);
            }
        }

        NativeArray<Vector3> m_InteractorLinePoints;

        XROrigin m_XROrigin;
        GameObject m_ReticleInstance;
        XRBaseInteractor m_Interactor;
        Vector3 m_TargetEndPoint;
        Vector3 m_TargetEndNormal;
        PhysicsScene m_LocalPhysicsScene;
        bool m_HasRaycastHit;
        
        /// <summary>
        /// Reusable array of ray cast hits.
        /// </summary>
        readonly RaycastHit[] m_RaycastHits = new RaycastHit[k_MaxRaycastHits];

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            m_LocalPhysicsScene = gameObject.scene.GetPhysicsScene();

            if (TryGetComponent(out m_Interactor))
            {
                m_Interactor.selectEntered.AddListener(OnSelectEntered);
            }

            FindXROrigin();
            SetupReticlePrefab();
            reticleActive = false;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            reticleActive = false;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Update()
        {
            if (m_Interactor != null && UpdateReticleTarget())
                ActivateReticleAtTarget();
            else
                reticleActive = false;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDestroy()
        {
            if (m_InteractorLinePoints.IsCreated)
            {
                m_InteractorLinePoints.Dispose();
            }

            if (m_Interactor != null)
            {
                m_Interactor.selectEntered.RemoveListener(OnSelectEntered);
            }
        }
        
        void FindXROrigin()
        {
            if (m_XROrigin == null)
                ComponentLocatorUtility<XROrigin>.TryFindComponent(out m_XROrigin);
        }

        void SetupReticlePrefab()
        {
            if (m_ReticleInstance != null)
                Destroy(m_ReticleInstance);

            if (m_ReticlePrefab != null)
                m_ReticleInstance = Instantiate(m_ReticlePrefab);
        }

        static RaycastHit FindClosestHit(RaycastHit[] hits, int hitCount)
        {
            var index = 0;
            var distance = float.MaxValue;
            for (var i = 0; i < hitCount; ++i)
            {
                if (hits[i].distance < distance)
                {
                    distance = hits[i].distance;
                    index = i;
                }
            }

            return hits[index];
        }

        bool TryGetRaycastPoint(ref Vector3 raycastPos, ref Vector3 raycastNormal)
        {
            var raycastHit = false;

            // Raycast against physics
            var hitCount = m_LocalPhysicsScene.Raycast(m_Interactor.attachTransform.position, m_Interactor.attachTransform.forward,
                m_RaycastHits, m_MaxRaycastDistance, m_RaycastMask);
            if (hitCount != 0)
            {
                var closestHit = FindClosestHit(m_RaycastHits, hitCount);
                raycastPos = closestHit.point;
                raycastNormal = closestHit.normal;
                raycastHit = true;
            }

            return raycastHit;
        }

        bool UpdateReticleTarget()
        {
            if (!m_DrawWhileSelecting && m_Interactor.hasSelection)
                return false;

            if (m_Interactor.disableVisualsWhenBlockedInGroup && m_Interactor.IsBlockedByInteractionWithinGroup())
                return false;

            var hasRaycastHit = false;
            var raycastPos = Vector3.zero;
            var raycastNormal = Vector3.zero;

            if (m_Interactor is XRRayInteractor rayInteractor)
            {
                if (rayInteractor.TryGetCurrentRaycast(out var raycastHit, out _, out var uiRaycastHit, out _, out var isUIHitClosest))
                {
                    if (isUIHitClosest)
                    {
                        Debug.Assert(uiRaycastHit.HasValue, this);
                        var hit = uiRaycastHit.Value;
                        raycastPos = hit.worldPosition;
                        raycastNormal = hit.worldNormal;
                        // If the raycast hits the back of a UI canvas, ensure the normal refers to back of the UI canvas
                        // instead of the front facing world normal of the UI canvas
                        var isHittingBackOfCanvas = Vector3.Dot(rayInteractor.rayOriginTransform.forward, raycastNormal) > 0.0f;
                        if (isHittingBackOfCanvas)
                            raycastNormal *= -1;
                        hasRaycastHit = true;
                    }
                    else if (raycastHit.HasValue)
                    {
                        var hit = raycastHit.Value;
                        raycastPos = hit.point;
                        raycastNormal = hit.normal;
                        hasRaycastHit = true;
                    }
                }
                else if (m_DrawOnNoHit && rayInteractor.GetLinePoints(ref m_InteractorLinePoints, out _))
                {
                    raycastPos = m_InteractorLinePoints != null && m_InteractorLinePoints.Length > 0
                        ? m_InteractorLinePoints[m_InteractorLinePoints.Length - 1]
                        : Vector3.zero;
                }
            }
            else if (TryGetRaycastPoint(ref raycastPos, ref raycastNormal))
            {
                hasRaycastHit = true;
            }

            m_HasRaycastHit = hasRaycastHit;
            
            if (hasRaycastHit || m_DrawOnNoHit)
            {
                // Smooth target
                var velocity = Vector3.zero;
                m_TargetEndPoint = Vector3.SmoothDamp(m_TargetEndPoint, raycastPos, ref velocity, m_EndpointSmoothingTime);
                m_TargetEndNormal = Vector3.SmoothDamp(m_TargetEndNormal, raycastNormal, ref velocity, m_EndpointSmoothingTime);
                return true;
            }
            return false;
        }

        void ActivateReticleAtTarget()
        {
            if (m_ReticleInstance != null)
            {
                m_ReticleInstance.transform.position = m_TargetEndPoint;

                // Attempt to align reticle's Z axis with the XR Origin's up vector.  
                var relativeUpVector = (m_XROrigin != null && m_XROrigin.Origin != null)? m_XROrigin.Origin.transform.up : Vector3.up;

                if (m_AlignPrefabWithSurfaceNormal && m_HasRaycastHit)
                {
                    var vectorToProject = relativeUpVector;
                    
                    // If surface normal is directly up indicating a horizontal surface, align the reticle's Z axis with
                    // the direction of the interactor's raycast direction. Multiple by dot product to flip reticle when
                    // on the underside of a horizontal surface.
                    var targetNormalProjectedVectorDotProduct = Vector3.Dot(m_TargetEndNormal, vectorToProject);
                    if (Mathf.Approximately(Mathf.Abs(targetNormalProjectedVectorDotProduct), 1f)) 
                        vectorToProject = m_Interactor.transform.forward * targetNormalProjectedVectorDotProduct;

                    // Calculate the projected forward vector on the target normal
                    var forwardVector = Vector3.ProjectOnPlane(vectorToProject, m_TargetEndNormal);
                    if(forwardVector != Vector3.zero)
                        m_ReticleInstance.transform.rotation = Quaternion.LookRotation(forwardVector, m_TargetEndNormal);
                }
                else
                {
                    m_ReticleInstance.transform.rotation = Quaternion.LookRotation(relativeUpVector, (m_Interactor.attachTransform.position - m_TargetEndPoint).normalized);
                }

                var scaleFactor = m_PrefabScalingFactor;
                if (m_UndoDistanceScaling)
                    scaleFactor *= Vector3.Distance(m_Interactor.attachTransform.position, m_TargetEndPoint);
                m_ReticleInstance.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                reticleActive = true;
            }
        }

        void OnSelectEntered(SelectEnterEventArgs args)
        {
            reticleActive = false;
        }
    }
}