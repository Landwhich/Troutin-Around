using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VR_FishingRod : MonoBehaviour
{

    public XRGrabInteractable grabInteractable;
    public InputActionProperty castTriggerAction;
    public InputActionProperty reelTriggerAction;

    public float lureThrowForce;
    public float lureReelForce;
    public float maxThrowForce;

    private Vector3 startPos;
    private Vector3 controllerVelocity;

    public GameObject fishingLure;
    public Transform lureOrigin;
    public VR_LureCollision lureCollision;
    private Rigidbody lureRB;

    private bool isHeld = false;
    private bool isCasting = false;

    private bool wasPressed = false;

    private void Start()
    {
        lureRB = fishingLure.GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        if (grabInteractable != null) { 
            grabInteractable = GetComponent<XRGrabInteractable>();
        }
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        castTriggerAction.action.Enable();
        reelTriggerAction.action.Enable();
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
        castTriggerAction.action.Disable();
        reelTriggerAction.action.Disable();
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isHeld = true;
        Debug.Log("rod is being held");

    }

    void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
        Debug.Log("Rod dropped");
    }

    void Update()
    {
        if (!isHeld) return;
        
        float castValue = castTriggerAction.action.ReadValue<float>();
        bool castIsPressed = castValue > 0.8f;

        float reelValue = reelTriggerAction.action.ReadValue<float>();
        bool reelIsPressed = reelValue > 0.8f;

        if (castIsPressed && !isCasting && !lureCollision.lureIsInWater) { 
            Debug.Log("Charging Cast");

            isCasting = true;

            startPos = lureOrigin.position;
            //throwLure();
        }

        if (isCasting)
        {
            controllerVelocity = (lureOrigin.position - startPos) / Time.deltaTime;
            startPos = lureOrigin.position;
        }

        if(isCasting && !castIsPressed && !lureCollision.lureIsInWater)
        {
            Debug.Log("released Cast");
            isCasting = false;
            throwLure(controllerVelocity);
        }

        if (reelIsPressed && lureCollision.lureIsInWater)
        {
            Debug.Log("Reeling Lure");
            reelLure();
        }
    }

    public void throwLure(Vector3 velocity)
    {
        fishingLure.transform.SetParent(null);

        lureRB.useGravity = true;
        lureRB.isKinematic = false;
        lureRB.linearVelocity = Vector3.zero;

        fishingLure.transform.position = lureOrigin.position;
        fishingLure.SetActive(true);

        
        Vector3 throwForce = Vector3.ClampMagnitude(velocity * lureThrowForce, maxThrowForce);
        lureRB.AddForce(throwForce, ForceMode.Impulse);
        
        Debug.Log("lure has been thrown");
    }

    public void reelLure()
    {
        if (lureCollision.lureIsInWater)
        {
            Vector3 direction = (lureOrigin.position - fishingLure.transform.position).normalized;

            lureRB.AddForce(direction* lureReelForce, ForceMode.Acceleration);
        
            float distance = Vector3.Distance(fishingLure.transform.position, lureOrigin.position);
            if (distance < 0.5f) {
                Debug.Log("lure reeled in");

                lureRB.linearVelocity = Vector3.zero;
                lureRB.angularVelocity = Vector3.zero;
                lureRB.isKinematic=true;

                fishingLure.transform.position = lureOrigin.position;

                fishingLure.transform.SetParent(lureOrigin);

                lureCollision.lureIsInWater = false;
            }

        }

    }
}
