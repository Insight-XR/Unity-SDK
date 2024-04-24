using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
public class XROffsetGrab : XRGrabInteractable
{
    // Start is called before the first frame update
    void Start()
    {
        if (!attachTransform)
        {
            GameObject grab = new GameObject("Grab Pivot");
            grab.transform.SetParent(transform, false);
            attachTransform = grab.transform;
        }
    }

    protected override void OnSelectEntering(XRBaseInteractor Interact)
    {
        if (Interact is XRDirectInteractor)
        {
            attachTransform.position = Interact.transform.position;
            attachTransform.transform.rotation = Interact.transform.rotation;
        }
        
        base.OnSelectEntering(Interact);
    }
}
