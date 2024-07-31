using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InsightDesk
{
    public class SliderManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool pointerDown { get; private set; }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pointerDown = false;
        }
    }
}