using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.Toolkit.UI
{
    public abstract partial class UIInputModule
    {
        /// <summary>
        /// Calls the methods in its invocation list after the input module collects a list of type <see cref="RaycastResult"/>, but before the results are used.
        /// Note that not all fields of the event data are still valid or up to date at this point in the UI event processing.
        /// This event can be used to read, modify, or reorder results.
        /// After the event, the first result in the list with a non-null GameObject will be used.
        /// </summary>
        public event Action<PointerEventData, List<RaycastResult>> finalizeRaycastResults;

        /// <summary>
        /// This occurs when a UI pointer enters an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> pointerEnter;

        /// <summary>
        /// This occurs when a UI pointer exits an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> pointerExit;

        /// <summary>
        /// This occurs when a select button down occurs while a UI pointer is hovering an element.
        /// This event is executed using ExecuteEvents.ExecuteHierarchy when sent to the target element.
        /// </summary>
        public event Action<GameObject, PointerEventData> pointerDown;

        /// <summary>
        /// This occurs when a select button up occurs while a UI pointer is hovering an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> pointerUp;

        /// <summary>
        /// This occurs when a select button click occurs while a UI pointer is hovering an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> pointerClick;

        #if UNITY_2021_1_OR_NEWER || PACKAGE_DOCS_GENERATION
        /// <summary>
        /// This occurs while a UI pointer is moving over elements.
        /// </summary>
        /// <remarks>
        /// This may induce performance penalties due to the frequency in which this event gets called
        /// and should be used with that consideration in mind.
        /// Only invoked in Unity 2021.1 and newer.
        /// </remarks>
        public event Action<GameObject, PointerEventData> pointerMove;
        #endif

        /// <summary>
        /// This occurs when a potential drag occurs on an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> initializePotentialDrag;

        /// <summary>
        /// This occurs when a drag first occurs on an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> beginDrag;

        /// <summary>
        /// This occurs every frame while dragging an element.
        /// </summary>
        public event Action<GameObject, PointerEventData> drag;

        /// <summary>
        /// This occurs on the last frame an element is dragged.
        /// </summary>
        public event Action<GameObject, PointerEventData> endDrag;

        /// <summary>
        /// This occurs when a dragged element is dropped on a drop handler.
        /// </summary>
        public event Action<GameObject, PointerEventData> drop;

        /// <summary>
        /// This occurs when an element is scrolled
        /// This event is executed using ExecuteEvents.ExecuteHierarchy when sent to the target element.
        /// </summary>
        public event Action<GameObject, PointerEventData> scroll;

        /// <summary>
        /// This occurs on update for the currently selected object.
        /// </summary>
        public event Action<GameObject, BaseEventData> updateSelected;

        /// <summary>
        /// This occurs when the move axis is activated.
        /// </summary>
        public event Action<GameObject, AxisEventData> move;

        /// <summary>
        /// This occurs when the submit button is pressed.
        /// </summary>
        public event Action<GameObject, BaseEventData> submit;

        /// <summary>
        /// This occurs when the cancel button is pressed.
        /// </summary>
        public event Action<GameObject, BaseEventData> cancel;
    }

    #region Hover
    /// <summary>
    /// <see cref="UnityEvent"/> that Unity invokes when an Interactor initiates hovering over a new UI element.
    /// </summary>
    [Serializable]
    public sealed class UIHoverEnterEvent : UnityEvent<UIHoverEventArgs>
    {
    }

    /// <summary>
    /// <see cref="UnityEvent"/> that Unity invokes when an Interactor ends hovering over a UI element.
    /// </summary>
    [Serializable]
    public sealed class UIHoverExitEvent : UnityEvent<UIHoverEventArgs>
    {
    }

    /// <summary>
    /// Arguments passed to the <see cref="UnityEvent"/> that Unity invokes when an Interactor is hovering over a UI element.
    /// </summary>
    public class UIHoverEventArgs
    {
        /// <summary>
        /// The <see cref="IUIInteractor"/> that is hovering.
        /// </summary>
        public IUIInteractor interactorObject { get; set; }

        /// <summary>
        /// The <see cref="TrackedDeviceModel"/> corresponding to the controller or hand
        /// interacting with the UI element that is being hovered over.
        /// </summary>
        public TrackedDeviceModel deviceModel { get; set; }

        /// <summary>
        /// The UI element that is being hovered over.
        /// </summary>
        public GameObject uiObject { get; set; }
    }
    #endregion
}
