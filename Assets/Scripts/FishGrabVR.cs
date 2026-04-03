using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FishGrabVR : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private XRGrabInteractable grab;
    //public GameObject parentObj;
    public VR_FishingRod fishingRod;
    public Transform fishMouth;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        if (grab == null)
        {
            Debug.LogError("XRGrabInteractable missing on fish!");
        }
    }

    void OnEnable()
    {
        grab.firstSelectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        grab.firstSelectEntered.RemoveListener(OnGrab);
        grab.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        transform.SetParent(null,true);
        fishingRod.onFishRemove();
        //Debug.Log("FISH GRABBED FROM ROD");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
