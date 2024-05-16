using UnityEngine.Assertions;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers
{
    /// <summary>
    /// Abstract base class from which all grab transformer behaviors derive.
    /// Instances of this class can be assigned to an <see cref="XRGrabInteractable"/> using the Inspector
    /// by setting the Starting Single Grab Transformers (<see cref="XRGrabInteractable.startingSingleGrabTransformers"/>) and
    /// Starting Multiple Grab Transformers (<see cref="XRGrabInteractable.startingMultipleGrabTransformers"/>).
    /// This serves as a serializable reference instead of using the runtime list of grab transformers
    /// which is not serialized.
    /// </summary>
    /// <seealso cref="IXRGrabTransformer"/>
    public abstract class XRBaseGrabTransformer : MonoBehaviour, IXRGrabTransformer
    {
        /// <summary>
        /// Options for the register mode of the grab transformer.
        /// </summary>
        /// <seealso cref="registrationMode"/>
        public enum RegistrationMode
        {
            /// <summary>
            /// Do not automatically register with the interactable. You will need to manually add it through code
            /// or by adding a reference to <see cref="XRGrabInteractable.startingSingleGrabTransformers"/>
            /// or <see cref="XRGrabInteractable.startingMultipleGrabTransformers"/>.
            /// </summary>
            None,

            /// <summary>
            /// Register as a grab transformer used when there is a single interactor selecting the object.
            /// </summary>
            Single,

            /// <summary>
            /// Register as a grab transformer used when there are multiple interactors selecting the object.
            /// </summary>
            Multiple,

            /// <summary>
            /// Register as a grab transformer used both when there are multiple interactors selecting the object
            /// or a single interactor selecting the object.
            /// </summary>
            SingleAndMultiple,
        }

        /// <inheritdoc />
        public virtual bool canProcess => isActiveAndEnabled;

        /// <summary>
        /// Controls how this grab transformer will be registered automatically at startup.
        /// </summary>
        /// <seealso cref="RegistrationMode"/>
        /// <remarks>
        /// This can be overridden in derived classes.
        /// <br />
        /// <example>
        /// If you want your derived class to register as a Multiple Grab Transformer:
        /// <code>
        /// protected override RegisterMode registrationMode => RegisterMode.Multiple;
        /// </code>
        /// </example>
        /// </remarks>
        protected virtual RegistrationMode registrationMode => RegistrationMode.Single;

        /// <summary>
        /// This function is called just before any of the Update methods is called the first time. See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Start()
        {
            if (TryGetComponent<XRGrabInteractable>(out var grabInteractable))
            {
                // If the user has explicitly added this grab transformer to either
                // of the starting lists, assume that automatic registration should be
                // skipped.
                if (grabInteractable.startingSingleGrabTransformers.Contains(this) ||
                    grabInteractable.startingMultipleGrabTransformers.Contains(this))
                {
                    return;
                }

                // If this grab transformer is already registered to either
                // of the runtime lists, assume that this was added as part of the default
                // grab transformer addition (or by code for some other reason) so automatic
                // registration should be skipped.
                for (var index = grabInteractable.singleGrabTransformersCount - 1; index >= 0; --index)
                {
                    if (ReferenceEquals(grabInteractable.GetSingleGrabTransformerAt(index), this))
                        return;
                }

                for (var index = grabInteractable.multipleGrabTransformersCount - 1; index >= 0; --index)
                {
                    if (ReferenceEquals(grabInteractable.GetMultipleGrabTransformerAt(index), this))
                        return;
                }

                switch (registrationMode)
                {
                    case RegistrationMode.None:
                        break;
                    case RegistrationMode.Single:
                        grabInteractable.AddSingleGrabTransformer(this);
                        break;
                    case RegistrationMode.Multiple:
                        grabInteractable.AddMultipleGrabTransformer(this);
                        break;
                    case RegistrationMode.SingleAndMultiple:
                        grabInteractable.AddSingleGrabTransformer(this);
                        grabInteractable.AddMultipleGrabTransformer(this);
                        break;
                    default:
                        Assert.IsTrue(false, $"Unhandled {nameof(RegistrationMode)}={registrationMode}");
                        break;
                }
            }
        }

        /// <summary>
        /// This function is called when the MonoBehaviour will be destroyed. See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDestroy()
        {
            if (TryGetComponent<XRGrabInteractable>(out var grabInteractable))
            {
                grabInteractable.RemoveSingleGrabTransformer(this);
                grabInteractable.RemoveMultipleGrabTransformer(this);
            }
        }

        /// <inheritdoc />
        public virtual void OnLink(XRGrabInteractable grabInteractable)
        {
        }

        /// <inheritdoc />
        public virtual void OnGrab(XRGrabInteractable grabInteractable)
        {
        }

        /// <inheritdoc />
        public virtual void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
        {
        }

        /// <inheritdoc />
        public abstract void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale);

        /// <inheritdoc />
        public virtual void OnUnlink(XRGrabInteractable grabInteractable)
        {
        }
    }
}
