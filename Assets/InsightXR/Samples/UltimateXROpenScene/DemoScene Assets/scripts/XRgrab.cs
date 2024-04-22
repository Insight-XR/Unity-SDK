using UnityEngine.XR.Interaction.Toolkit;
 
public class XRgrab : XRGrabInteractable
{
    protected override void OnSelectEntered(XRBaseInteractor interactor)
    {
        SetParentToXRRig();
        base.OnSelectEntered(interactor);
    }
 
    protected override void OnSelectExited(XRBaseInteractor interactor)
    {
        SetParentToWorld();
        base.OnSelectExited(interactor);
    }
 
    public void SetParentToXRRig()
    {
        transform.SetParent(selectingInteractor.transform);
    }
 
    public void SetParentToWorld()
    {
        transform.SetParent(null);
    }
}