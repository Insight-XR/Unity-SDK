using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit
{
    /// <summary>
    /// Enum used to determine how the socket should scale the interactable.
    /// </summary>
    /// <seealso cref="XRSocketInteractor.socketScaleMode"/>
    public enum SocketScaleMode
    {
        /// <summary>
        /// The interactable will not be scaled when attached to the socket.
        /// </summary>
        None,

        /// <summary>
        /// The interactable will be scaled to a fixed size when attached to the socket.
        /// The actual size is defined by the <see cref="XRSocketInteractor.fixedScale"/> value.
        /// </summary>
        Fixed,

        /// <summary>
        /// The interactable will be scaled to fit the size of the socket when attached.
        /// The scaling is dynamic, computed using the interactable's bounds, with a target size defined by <see cref="XRSocketInteractor.targetBoundsSize"/>.
        /// </summary>
        StretchedToFitSize,
    }


    /// <summary>
    /// Interactor used for holding interactables via a socket. This component is not designed to be attached to a controller
    /// (thus does not derive from <see cref="XRBaseControllerInteractor"/>) and instead will always attempt to select an interactable that it is
    /// hovering over.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/XR Socket Interactor", 11)]
    [HelpURL(XRHelpURLConstants.k_XRSocketInteractor)]
    public partial class XRSocketInteractor : XRBaseInteractor
    {
        [SerializeField]
        bool m_ShowInteractableHoverMeshes = true;
        /// <summary>
        /// Whether this socket should show a mesh at socket's attach point for Interactables that it is hovering over.
        /// </summary>
        /// <remarks>
        /// The interactable's attach transform must not change parent Transform while selected
        /// for the position and rotation of the hover mesh to be correctly calculated.
        /// </remarks>
        public bool showInteractableHoverMeshes
        {
            get => m_ShowInteractableHoverMeshes;
            set => m_ShowInteractableHoverMeshes = value;
        }

        [SerializeField]
        Material m_InteractableHoverMeshMaterial;
        /// <summary>
        /// Material used for rendering interactable meshes on hover
        /// (a default material will be created if none is supplied).
        /// </summary>
        public Material interactableHoverMeshMaterial
        {
            get => m_InteractableHoverMeshMaterial;
            set => m_InteractableHoverMeshMaterial = value;
        }

        [SerializeField]
        Material m_InteractableCantHoverMeshMaterial;
        /// <summary>
        /// Material used for rendering interactable meshes on hover when there is already a selected object in the socket
        /// (a default material will be created if none is supplied).
        /// </summary>
        public Material interactableCantHoverMeshMaterial
        {
            get => m_InteractableCantHoverMeshMaterial;
            set => m_InteractableCantHoverMeshMaterial = value;
        }

        [SerializeField]
        bool m_SocketActive = true;
        /// <summary>
        /// Whether socket interaction is enabled.
        /// </summary>
        public bool socketActive
        {
            get => m_SocketActive;
            set
            {
                m_SocketActive = value;
                m_SocketGrabTransformer.canProcess = value && isActiveAndEnabled;
            }
        }

        [SerializeField]
        float m_InteractableHoverScale = 1f;
        /// <summary>
        /// Scale at which to render hovered Interactable.
        /// </summary>
        public float interactableHoverScale
        {
            get => m_InteractableHoverScale;
            set => m_InteractableHoverScale = value;
        }

        [SerializeField]
        float m_RecycleDelayTime = 1f;
        /// <summary>
        /// Sets the amount of time the socket will refuse hovers after an object is removed.
        /// </summary>
        /// <remarks>
        /// Does nothing if <see cref="hoverSocketSnapping"/> is enabled to prevent snap flickering.
        /// </remarks>
        public float recycleDelayTime
        {
            get => m_RecycleDelayTime;
            set => m_RecycleDelayTime = value;
        }

        float m_LastRemoveTime = -1f;

        [SerializeField]
        bool m_HoverSocketSnapping;

        /// <summary>
        /// Determines if the interactable should snap to the socket's attach transform when hovering.
        /// Note this will cause z-fighting with the hover mesh visuals, so it is recommended to disable <see cref="showInteractableHoverMeshes"/> if this is active.
        /// If enabled, hover recycle delay functionality is disabled to prevent snap flickering.
        /// </summary>
        public bool hoverSocketSnapping
        {
            get => m_HoverSocketSnapping;
            set => m_HoverSocketSnapping = value;
        }

        [SerializeField]
        float m_SocketSnappingRadius = 0.1f;

        /// <summary>
        /// When socket snapping is enabled, this is the radius within which the interactable will snap to the socket's attach transform while hovering.
        /// </summary>
        public float socketSnappingRadius
        {
            get => m_SocketSnappingRadius;
            set
            {
                m_SocketSnappingRadius = value;
                m_SocketGrabTransformer.socketSnappingRadius = value;
            }
        }

        [SerializeField]
        SocketScaleMode m_SocketScaleMode = SocketScaleMode.None;

        /// <summary>
        /// Scale mode used to calculate the scale factor applied to the interactable when hovering.
        /// </summary>
        /// <seealso cref="SocketScaleMode"/>
        public SocketScaleMode socketScaleMode
        {
            get => m_SocketScaleMode;
            set
            {
                m_SocketScaleMode = value;
                m_SocketGrabTransformer.scaleMode = value;
            }
        }

        [SerializeField]
        Vector3 m_FixedScale = Vector3.one;

        /// <summary>
        /// Scale factor applied to the interactable when scale mode is set to <see cref="SocketScaleMode.Fixed"/>.
        /// </summary>
        /// <seealso cref="socketScaleMode"/>
        public Vector3 fixedScale
        {
            get => m_FixedScale;
            set
            {
                m_FixedScale = value;
                m_SocketGrabTransformer.fixedScale = value;
            }
        }

        [SerializeField]
        Vector3 m_TargetBoundsSize = Vector3.one;

        /// <summary>
        /// Bounds size used to calculate the scale factor applied to the interactable when scale mode is set to <see cref="SocketScaleMode.StretchedToFitSize"/>.
        /// </summary>
        /// <seealso cref="socketScaleMode"/>
        public Vector3 targetBoundsSize
        {
            get => m_TargetBoundsSize;
            set
            {
                m_TargetBoundsSize = value;
                m_SocketGrabTransformer.targetBoundsSize = value;
            }
        }

        /// <summary>
        /// The set of Interactables that this Interactor could possibly interact with this frame.
        /// This list is not sorted by priority.
        /// </summary>
        /// <seealso cref="IXRInteractor.GetValidTargets"/>
        protected List<IXRInteractable> unsortedValidTargets { get; } = new List<IXRInteractable>();

        /// <summary>
        /// The set of Colliders that stayed in touch with this Interactor on fixed updated.
        /// This list will be populated by colliders in OnTriggerStay.
        /// </summary>
        readonly HashSet<Collider> m_StayedColliders = new HashSet<Collider>();

        readonly TriggerContactMonitor m_TriggerContactMonitor = new TriggerContactMonitor();

        readonly Dictionary<IXRInteractable, ValueTuple<MeshFilter, Renderer>[]> m_MeshFilterCache = new Dictionary<IXRInteractable, ValueTuple<MeshFilter, Renderer>[]>();

        /// <summary>
        /// Reusable list of type <see cref="MeshFilter"/> to reduce allocations.
        /// </summary>
        static readonly List<MeshFilter> s_MeshFilters = new List<MeshFilter>();

        /// <summary>
        /// Reusable value of <see cref="WaitForFixedUpdate"/> to reduce allocations.
        /// </summary>
        static readonly WaitForFixedUpdate s_WaitForFixedUpdate = new WaitForFixedUpdate();

        /// <summary>
        /// Reference to Coroutine that updates the trigger contact monitor with the current
        /// stayed colliders.
        /// </summary>
        IEnumerator m_UpdateCollidersAfterTriggerStay;

        readonly XRSocketGrabTransformer m_SocketGrabTransformer = new XRSocketGrabTransformer();

        readonly HashSetList<XRGrabInteractable> m_InteractablesWithSocketTransformer = new HashSetList<XRGrabInteractable>();

        /// <summary>
        /// Maximum number of interactables this interactor can socket.
        /// Used for hover socket snapping evaluation.
        /// </summary>
        protected virtual int socketSnappingLimit => 1;

        /// <summary>
        /// Determines if when snapping to a socket, any existing sockets should be ejected.
        /// </summary>
        protected virtual bool ejectExistingSocketsWhenSnapping => true;
        
        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnValidate()
        {
            SyncTransformerParams();
        }

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();
            m_TriggerContactMonitor.interactionManager = interactionManager;
            m_UpdateCollidersAfterTriggerStay = UpdateCollidersAfterOnTriggerStay();

            SyncTransformerParams();
            CreateDefaultHoverMaterials();
        }

        /// <inheritdoc />
        protected override void OnEnable()
        {
            base.OnEnable();
            m_TriggerContactMonitor.contactAdded += OnContactAdded;
            m_TriggerContactMonitor.contactRemoved += OnContactRemoved;
            m_SocketGrabTransformer.canProcess = m_SocketActive;
            SyncTransformerParams();
            ResetCollidersAndValidTargets();
            StartCoroutine(m_UpdateCollidersAfterTriggerStay);
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();
            m_SocketGrabTransformer.canProcess = false;
            m_TriggerContactMonitor.contactAdded -= OnContactAdded;
            m_TriggerContactMonitor.contactRemoved -= OnContactRemoved;
            ResetCollidersAndValidTargets();
            StopCoroutine(m_UpdateCollidersAfterTriggerStay);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        /// <param name="other">The other <see cref="Collider"/> involved in this collision.</param>
        protected void OnTriggerEnter(Collider other)
        {
            m_TriggerContactMonitor.AddCollider(other);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        /// <param name="other">The other <see cref="Collider"/> involved in this collision.</param>
        protected void OnTriggerStay(Collider other)
        {
            m_StayedColliders.Add(other);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        /// <param name="other">The other <see cref="Collider"/> involved in this collision.</param>
        protected void OnTriggerExit(Collider other)
        {
            m_TriggerContactMonitor.RemoveCollider(other);
        }

        /// <summary>
        /// This coroutine functions like a LateFixedUpdate method that executes after OnTriggerXXX.
        /// </summary>
        /// <returns>Returns enumerator for coroutine.</returns>
        IEnumerator UpdateCollidersAfterOnTriggerStay()
        {
            while (true)
            {
                // Wait until the end of the physics cycle so that OnTriggerXXX can get called.
                // See https://docs.unity3d.com/Manual/ExecutionOrder.html
                yield return s_WaitForFixedUpdate;

                m_TriggerContactMonitor.UpdateStayedColliders(m_StayedColliders);
            }
            // ReSharper disable once IteratorNeverReturns -- stopped when behavior is destroyed.
        }

        /// <inheritdoc />
        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
            {
                // Clear stayed Colliders at the beginning of the physics cycle before
                // the OnTriggerStay method populates this list.
                // Then the UpdateCollidersAfterOnTriggerStay coroutine will use this list to remove Colliders
                // that no longer stay in this frame after previously entered and add any stayed Colliders
                // that are not currently tracked by the TriggerContactMonitor.
                m_StayedColliders.Clear();
            }
            else if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                // An explicit check for isHoverRecycleAllowed is done since an interactable may have been deselected
                // after this socket was updated by the manager, such as when a later Interactor takes the selection
                // from this socket. The recycle delay time could cause the hover to be effectively disabled.
                if (m_ShowInteractableHoverMeshes && hasHover && isHoverRecycleAllowed)
                    DrawHoveredInteractables();
            }
        }

        /// <summary>
        /// Creates the default hover materials
        /// for <see cref="interactableHoverMeshMaterial"/> and <see cref="interactableCantHoverMeshMaterial"/> if necessary.
        /// </summary>
        protected virtual void CreateDefaultHoverMaterials()
        {
            if (m_InteractableHoverMeshMaterial != null && m_InteractableCantHoverMeshMaterial != null)
                return;

            var shaderName = GraphicsSettings.currentRenderPipeline ? "Universal Render Pipeline/Lit" : "Standard";
            var defaultShader = Shader.Find(shaderName);

            if (defaultShader == null)
            {
                Debug.LogWarning("Failed to create default materials for Socket Interactor," +
                    $" was unable to find \"{shaderName}\" Shader. Make sure the shader is included into the game build.", this);
                return;
            }

            if (m_InteractableHoverMeshMaterial == null)
            {
                m_InteractableHoverMeshMaterial = new Material(defaultShader);
                SetMaterialFade(m_InteractableHoverMeshMaterial, new Color(0f, 0f, 1f, 0.6f));
            }

            if (m_InteractableCantHoverMeshMaterial == null)
            {
                m_InteractableCantHoverMeshMaterial = new Material(defaultShader);
                SetMaterialFade(m_InteractableCantHoverMeshMaterial, new Color(1f, 0f, 0f, 0.6f));
            }
        }

        /// <summary>
        /// Sets Standard <paramref name="material"/> with Fade rendering mode
        /// and set <paramref name="color"/> as the main color.
        /// </summary>
        /// <param name="material">The <see cref="Material"/> whose properties will be set.</param>
        /// <param name="color">The main color to set.</param>
        static void SetMaterialFade(Material material, Color color)
        {
            material.SetOverrideTag("RenderType", "Transparent");

            // In a Scripted Render Pipeline (URP/HDRP), we need to set the surface mode to 1 for transparent.
            var isSRP = GraphicsSettings.currentRenderPipeline != null;
            if (isSRP)
                material.SetFloat(ShaderPropertyLookup.surface, 1f);

            material.SetFloat(ShaderPropertyLookup.mode, 2f);
            material.SetInt(ShaderPropertyLookup.srcBlend, (int)BlendMode.SrcAlpha);
            material.SetInt(ShaderPropertyLookup.dstBlend, (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt(ShaderPropertyLookup.zWrite, 0);
            // ReSharper disable StringLiteralTypo
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            // ReSharper restore StringLiteralTypo
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetColor(isSRP ? ShaderPropertyLookup.baseColor : ShaderPropertyLookup.color, color);
        }

        /// <inheritdoc />
        protected override void OnHoverEntering(HoverEnterEventArgs args)
        {
            base.OnHoverEntering(args);

            // Avoid the performance cost of GetComponents if we don't need to show the hover meshes.
            if (!m_ShowInteractableHoverMeshes)
                return;

            var interactable = args.interactableObject;
            s_MeshFilters.Clear();
            interactable.transform.GetComponentsInChildren(true, s_MeshFilters);
            if (s_MeshFilters.Count == 0)
                return;

            var interactableTuples = new ValueTuple<MeshFilter, Renderer>[s_MeshFilters.Count];
            for (var i = 0; i < s_MeshFilters.Count; ++i)
            {
                var meshFilter = s_MeshFilters[i];
                interactableTuples[i] = (meshFilter, meshFilter.GetComponent<Renderer>());
            }
            m_MeshFilterCache.Add(interactable, interactableTuples);
        }

        /// <inheritdoc />
        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);

            if (!CanHoverSnap(args.interactableObject))
                return;

            if (args.interactableObject is XRGrabInteractable grabInteractable)
                StartSocketSnapping(grabInteractable);
        }

        /// <summary>
        /// Determines whether the specified <see cref="IXRInteractable"/> object can hover snap.
        /// </summary>
        /// <param name="interactable">The <see cref="IXRInteractable"/> object to check for hover snap capability.</param>
        /// <returns>Returns <see langword="true"/> if hover socket snapping is enabled and the interactable has no selection or is selecting; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This method checks whether hover socket snapping is allowed and whether the specified interactable has no current selection or is in the process of selecting.
        /// </remarks>
        protected virtual bool CanHoverSnap(IXRInteractable interactable)
        {
            return m_HoverSocketSnapping && (!hasSelection || IsSelecting(interactable));
        }

        /// <inheritdoc />
        protected override void OnHoverExiting(HoverExitEventArgs args)
        {
            base.OnHoverExiting(args);

            var interactable = args.interactableObject;
            m_MeshFilterCache.Remove(interactable);

            if (interactable is XRGrabInteractable grabInteractable)
                EndSocketSnapping(grabInteractable);
        }

        /// <inheritdoc />
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            if (args.interactableObject is XRGrabInteractable grabInteractable)
                StartSocketSnapping(grabInteractable);
        }

        /// <inheritdoc />
        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            base.OnSelectExiting(args);
            m_LastRemoveTime = Time.time;
        }

        /// <inheritdoc />
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            if (args.interactableObject is XRGrabInteractable grabInteractable)
                EndSocketSnapping(grabInteractable);
        }

        Matrix4x4 GetHoverMeshMatrix(IXRInteractable interactable, MeshFilter meshFilter, float hoverScale)
        {
            var interactableAttachTransform = interactable.GetAttachTransform(this);

            var grabInteractable = interactable as XRGrabInteractable;

            // Get the "static" pose of the interactable's attach transform in world space.
            // While the XR Grab Interactable is selected, the Attach Transform pose may have been modified
            // by user code, and we assume it will be restored back to the initial captured pose.
            // When Use Dynamic Attach is enabled, we can instead rely on using the dedicated GameObject for this interactor.
            Pose interactableAttachPose;
            if (grabInteractable != null && !grabInteractable.useDynamicAttach &&
                grabInteractable.isSelected &&
                interactableAttachTransform != interactable.transform &&
                interactableAttachTransform.IsChildOf(interactable.transform))
            {
                // The interactable's attach transform must not change parent Transform while selected
                // for the pose to be calculated correctly. This transforms the captured pose in local space
                // into the current pose in world space. If the pose of the attach transform was not modified
                // after being selected, this will be the same value as calculated in the else statement.
                var localAttachPose = grabInteractable.GetLocalAttachPoseOnSelect(grabInteractable.firstInteractorSelecting);
                var attachTransformParent = interactableAttachTransform.parent;
                interactableAttachPose =
                    new Pose(attachTransformParent.TransformPoint(localAttachPose.position),
                        attachTransformParent.rotation * localAttachPose.rotation);
            }
            else
            {
                interactableAttachPose = new Pose(interactableAttachTransform.position, interactableAttachTransform.rotation);
            }

            var attachOffset = meshFilter.transform.position - interactableAttachPose.position;
            var interactableLocalPosition = InverseTransformDirection(interactableAttachPose, attachOffset) * hoverScale;
            var interactableLocalRotation = Quaternion.Inverse(Quaternion.Inverse(meshFilter.transform.rotation) * interactableAttachPose.rotation);

            Vector3 position;
            Quaternion rotation;

            var interactorAttachTransform = GetAttachTransform(interactable);
            var interactorAttachPose = new Pose(interactorAttachTransform.position, interactorAttachTransform.rotation);
            if (grabInteractable == null || grabInteractable.trackRotation)
            {
                position = interactorAttachPose.rotation * interactableLocalPosition + interactorAttachPose.position;
                rotation = interactorAttachPose.rotation * interactableLocalRotation;
            }
            else
            {
                position = interactableAttachPose.rotation * interactableLocalPosition + interactorAttachPose.position;
                rotation = meshFilter.transform.rotation;
            }

            // Rare case that Track Position is disabled
            if (grabInteractable != null && !grabInteractable.trackPosition)
                position = meshFilter.transform.position;

            var scale = meshFilter.transform.lossyScale * hoverScale;

            return Matrix4x4.TRS(position, rotation, scale);
        }

        /// <summary>
        /// Transforms a direction from world space to local space. The opposite of <c>Transform.TransformDirection</c>,
        /// but using a world Pose instead of a Transform.
        /// </summary>
        /// <param name="pose">The world space position and rotation of the Transform.</param>
        /// <param name="direction">The direction to transform.</param>
        /// <returns>Returns the transformed direction.</returns>
        /// <remarks>
        /// This operation is unaffected by scale.
        /// <br/>
        /// You should use <c>Transform.InverseTransformPoint</c> equivalent if the vector represents a position in space rather than a direction.
        /// </remarks>
        static Vector3 InverseTransformDirection(Pose pose, Vector3 direction)
        {
            return Quaternion.Inverse(pose.rotation) * direction;
        }

        /// <summary>
        /// Unity calls this method automatically in order to draw the Interactables that are currently being hovered over.
        /// </summary>
        /// <seealso cref="GetHoveredInteractableMaterial"/>
        protected virtual void DrawHoveredInteractables()
        {
            if (!m_ShowInteractableHoverMeshes || m_InteractableHoverScale <= 0f)
                return;

            var mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            foreach (var interactable in interactablesHovered)
            {
                if (interactable == null)
                    continue;

                if (IsSelecting(interactable))
                    continue;

                if (!m_MeshFilterCache.TryGetValue(interactable, out var interactableTuples))
                    continue;

                if (interactableTuples == null || interactableTuples.Length == 0)
                    continue;

                var materialToDrawWith = GetHoveredInteractableMaterial(interactable);
                if (materialToDrawWith == null)
                    continue;

                foreach (var tuple in interactableTuples)
                {
                    var meshFilter = tuple.Item1;
                    var meshRenderer = tuple.Item2;
                    if (!ShouldDrawHoverMesh(meshFilter, meshRenderer, mainCamera))
                        continue;

                    var matrix = GetHoverMeshMatrix(interactable, meshFilter, m_InteractableHoverScale);
                    var sharedMesh = meshFilter.sharedMesh;
                    for (var submeshIndex = 0; submeshIndex < sharedMesh.subMeshCount; ++submeshIndex)
                    {
                        Graphics.DrawMesh(
                            sharedMesh,
                            matrix,
                            materialToDrawWith,
                            gameObject.layer,
                            null, // Draw mesh in all cameras (default value)
                            submeshIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the material used to draw the given hovered Interactable.
        /// </summary>
        /// <param name="interactable">The hovered Interactable to get the material for.</param>
        /// <returns>Returns the material Unity should use to draw the given hovered Interactable.</returns>
        protected virtual Material GetHoveredInteractableMaterial(IXRHoverInteractable interactable)
        {
            return hasSelection ? m_InteractableCantHoverMeshMaterial : m_InteractableHoverMeshMaterial;
        }

        /// <inheritdoc />
        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            targets.Clear();

            if (!isActiveAndEnabled)
                return;

            var filter = targetFilter;
            if (filter != null && filter.canProcess)
                filter.Process(this, unsortedValidTargets, targets);
            else
                SortingHelpers.SortByDistanceToInteractor(this, unsortedValidTargets, targets);
        }

        /// <inheritdoc />
        public override bool isHoverActive => base.isHoverActive && m_SocketActive;

        /// <inheritdoc />
        public override bool isSelectActive => base.isSelectActive && m_SocketActive;

        /// <inheritdoc />
        public override XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride => XRBaseInteractable.MovementType.Instantaneous;

        /// <inheritdoc />
        public override bool CanHover(IXRHoverInteractable interactable)
        {
            return base.CanHover(interactable) && isHoverRecycleAllowed;
        }

        bool isHoverRecycleAllowed => m_HoverSocketSnapping || (m_LastRemoveTime < 0f || m_RecycleDelayTime <= 0f || (Time.time > m_LastRemoveTime + m_RecycleDelayTime));

        /// <inheritdoc />
        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            return base.CanSelect(interactable) &&
                ((!hasSelection && !interactable.isSelected) ||
                    (IsSelecting(interactable) && interactable.interactorsSelecting.Count == 1));
        }

        /// <summary>
        /// Unity calls this method automatically in order to determine whether the mesh should be drawn.
        /// </summary>
        /// <param name="meshFilter">The <see cref="MeshFilter"/> which will be drawn when returning <see langword="true"/>.</param>
        /// <param name="meshRenderer">The <see cref="Renderer"/> on the same <see cref="GameObject"/> as the <paramref name="meshFilter"/>.</param>
        /// <param name="mainCamera">The Main Camera.</param>
        /// <returns>Returns <see langword="true"/> if the mesh should be drawn. Otherwise, returns <see langword="false"/>.</returns>
        /// <seealso cref="DrawHoveredInteractables"/>
        protected virtual bool ShouldDrawHoverMesh(MeshFilter meshFilter, Renderer meshRenderer, Camera mainCamera)
        {
            // Graphics.DrawMesh will automatically handle camera culling of the hover mesh using
            // the GameObject layer of this socket that we pass as the argument value.
            // However, we also check here to skip drawing the hover mesh if the mesh of the interactable
            // itself isn't also drawn by the main camera. For the typical scene with one camera,
            // this means that for the hover mesh to be rendered, the camera should have a culling mask
            // which overlaps with both the GameObject layer of this socket and the GameObject layer of the interactable.
            var cullingMask = mainCamera.cullingMask;
            return meshFilter != null && (cullingMask & (1 << meshFilter.gameObject.layer)) != 0 && meshRenderer != null && meshRenderer.enabled;
        }

        /// <inheritdoc />
        protected override void OnRegistered(InteractorRegisteredEventArgs args)
        {
            base.OnRegistered(args);
            args.manager.interactableRegistered += OnInteractableRegistered;
            args.manager.interactableUnregistered += OnInteractableUnregistered;

            // Attempt to resolve any colliders that entered this trigger while this was not subscribed,
            // and filter out any targets that were unregistered while this was not subscribed.
            m_TriggerContactMonitor.interactionManager = args.manager;
            m_TriggerContactMonitor.ResolveUnassociatedColliders();
            XRInteractionManager.RemoveAllUnregistered(args.manager, unsortedValidTargets);
        }

        /// <inheritdoc />
        protected override void OnUnregistered(InteractorUnregisteredEventArgs args)
        {
            base.OnUnregistered(args);
            args.manager.interactableRegistered -= OnInteractableRegistered;
            args.manager.interactableUnregistered -= OnInteractableUnregistered;
        }

        void OnInteractableRegistered(InteractableRegisteredEventArgs args)
        {
            m_TriggerContactMonitor.ResolveUnassociatedColliders(args.interactableObject);
            if (m_TriggerContactMonitor.IsContacting(args.interactableObject) && !unsortedValidTargets.Contains(args.interactableObject))
                unsortedValidTargets.Add(args.interactableObject);
        }

        void OnInteractableUnregistered(InteractableUnregisteredEventArgs args)
        {
            unsortedValidTargets.Remove(args.interactableObject);
        }

        void OnContactAdded(IXRInteractable interactable)
        {
            if (!unsortedValidTargets.Contains(interactable))
                unsortedValidTargets.Add(interactable);
        }

        void OnContactRemoved(IXRInteractable interactable)
        {
            unsortedValidTargets.Remove(interactable);
        }

        /// <summary>
        /// Clears current valid targets and stayed colliders.
        /// </summary>
        void ResetCollidersAndValidTargets()
        {
            unsortedValidTargets.Clear();
            m_StayedColliders.Clear();
            m_TriggerContactMonitor.UpdateStayedColliders(m_StayedColliders);
        }

        /// <summary>
        /// Initiates socket snapping for a specified <see cref="XRGrabInteractable"/> object.
        /// </summary>
        /// <param name="grabInteractable">The <see cref="XRGrabInteractable"/> object to initiate socket snapping for.</param>
        /// <returns>Returns <see langword="true"/> if the operation is successful; false if the socket snapping has already started for the interactable or if the number of interactables with socket transformer exceeds the socket limit.</returns>
        /// <remarks>
        /// If the socket snapping has already started for the interactable, or if the number of interactables with socket transformer exceeds the socket limit, the method does nothing.
        /// Otherwise, it adds the specified grab interactable to the socket grab transformer and adds it to the global and local interactables with socket transformer lists.
        /// </remarks>
        /// <seealso cref="EndSocketSnapping"/>
        protected virtual bool StartSocketSnapping(XRGrabInteractable grabInteractable)
        {
            // If we've already started socket snapping this interactable, do nothing
            var interactablesSocketedCount = m_InteractablesWithSocketTransformer.Count;
            if (interactablesSocketedCount >= socketSnappingLimit ||
                m_InteractablesWithSocketTransformer.Contains(grabInteractable))
                return false;

            if (interactablesSocketedCount > 0 && ejectExistingSocketsWhenSnapping)
            {
                // Be sure to eject any existing grab interactable from the snap grab socket
                foreach (var interactable in m_InteractablesWithSocketTransformer.AsList())
                {
                    interactable.RemoveSingleGrabTransformer(m_SocketGrabTransformer);
                }
                m_InteractablesWithSocketTransformer.Clear();
            }

            grabInteractable.AddSingleGrabTransformer(m_SocketGrabTransformer);
            m_InteractablesWithSocketTransformer.Add(grabInteractable);
            return true;
        }

        /// <summary>
        /// Ends socket snapping for a specified <see cref="XRGrabInteractable"/> object.
        /// </summary>
        /// <param name="grabInteractable">The <see cref="XRGrabInteractable"/> object to end socket snapping for.</param>
        /// <returns>Returns <see langword="true"/> if the specified grab interactable was found and removed; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// Removes the specified grab interactable from the local interactables with socket transformer list and removes it from the socket grab transformer.
        /// </remarks>
        /// <seealso cref="StartSocketSnapping"/>
        protected virtual bool EndSocketSnapping(XRGrabInteractable grabInteractable)
        {
            grabInteractable.RemoveSingleGrabTransformer(m_SocketGrabTransformer);
            return m_InteractablesWithSocketTransformer.Remove(grabInteractable);
        }

        void SyncTransformerParams()
        {
            m_SocketGrabTransformer.socketInteractor = this;
            m_SocketGrabTransformer.socketSnappingRadius = socketSnappingRadius;
            m_SocketGrabTransformer.scaleMode = socketScaleMode;
            m_SocketGrabTransformer.fixedScale = fixedScale;
            m_SocketGrabTransformer.targetBoundsSize = targetBoundsSize;
        }

        struct ShaderPropertyLookup
        {
            public static readonly int surface = Shader.PropertyToID("_Surface");
            public static readonly int mode = Shader.PropertyToID("_Mode");
            public static readonly int srcBlend = Shader.PropertyToID("_SrcBlend");
            public static readonly int dstBlend = Shader.PropertyToID("_DstBlend");
            public static readonly int zWrite = Shader.PropertyToID("_ZWrite");
            public static readonly int baseColor = Shader.PropertyToID("_BaseColor");
            public static readonly int color = Shader.PropertyToID("_Color"); // Legacy
        }
    }
}
