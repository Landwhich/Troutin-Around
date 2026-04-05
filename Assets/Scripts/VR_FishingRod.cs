using System.Transactions;
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
    private Rigidbody rodRB;

    private bool isHeld = false;
    private bool isCasting = false;

    public Bait_Collision baitCollision;
    public SpringJoint baitSpring;
    public float minDepth;
    public float maxDepth;
    public float depthSpeed;

    private float currentDepth = 0.5f;

    public GameObject fishGrabPrefab;
    public Transform hookTransform;
    public bool fishSpawned = false;
    private GameObject coughtFish;

    private void Start()
    {
        lureRB = fishingLure.GetComponent<Rigidbody>();
        rodRB = GetComponent<Rigidbody>();
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
        //Debug.Log("rod is being held");

    }

    void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
        //Debug.Log("Rod dropped");
        rodRB.isKinematic = false;
    }

    void Update()
    {
        if (!isHeld) return;
        
        float castValue = castTriggerAction.action.ReadValue<float>();
        bool castIsPressed = castValue > 0.8f;

        float reelValue = reelTriggerAction.action.ReadValue<float>();
        bool reelIsPressed = reelValue > 0.8f;

        if (castIsPressed && !isCasting && !lureCollision.lureIsInWater) { 
            //Debug.Log("Charging Cast");
            isCasting = true;
            startPos = lureOrigin.position;
        }
        //player is holding the trigger
        if (isCasting)
        {
            controllerVelocity = (lureOrigin.position - startPos) / Time.deltaTime;
            startPos = lureOrigin.position;
        }
        //player releases the trigger
        if(isCasting && !castIsPressed && !lureCollision.lureIsInWater)
        {
            //Debug.Log("released Cast");
            isCasting = false;
            throwLure(controllerVelocity);
        }
        //reel in bobby and lure
        if (reelIsPressed)
        {
            //Debug.Log("Reeling Lure");
            reelLure();
            baitHeight(-depthSpeed);
        }
        //adjust height of lure when in the water
        if (castIsPressed && lureCollision.lureIsInWater) 
        {
            baitHeight(depthSpeed);
        }
        //put fish on the lure when collision with fish happens
        if (baitCollision.caughtFish && !fishSpawned)
        {
            coughtFish = Instantiate(fishGrabPrefab);

            coughtFish.transform.SetParent(hookTransform);
            coughtFish.transform.localPosition = new Vector3(0,-10,0);
            coughtFish.transform.localRotation = Quaternion.Euler(-90f,0f,0f);
            FishGrabVR fishScript = coughtFish.GetComponent<FishGrabVR>();
            if (fishScript != null)
            {
                fishScript.fishingRod = this;
            }

            Rigidbody rb = coughtFish.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            fishSpawned = true;
        }

    }

    public void onFishRemove()
    {
        baitCollision.caughtFish = false;
        fishSpawned = false;
    }

    public void baitHeight(float speed)
    {
        currentDepth += speed * Time.deltaTime;
        currentDepth = Mathf.Clamp(currentDepth, minDepth, maxDepth);
        baitSpring.maxDistance = currentDepth;
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
        //Debug.Log("lure has been thrown");
    }

    public void reelLure()
    {
        if (lureCollision.lureIsInWater)
        {
            Vector3 direction = (lureOrigin.position - fishingLure.transform.position).normalized;
            lureRB.AddForce(direction* lureReelForce, ForceMode.Acceleration);
        
            float distance = Vector3.Distance(fishingLure.transform.position, lureOrigin.position);
            if (distance < 0.1f) {
                //Debug.Log("lure reeled in");

                lureRB.linearVelocity = Vector3.zero;
                lureRB.angularVelocity = Vector3.zero;
                lureRB.isKinematic=true;
                //lureRB.linearDamping = 1.0f;

                fishingLure.transform.position = lureOrigin.position;
                fishingLure.transform.SetParent(lureOrigin);
                lureCollision.lureIsInWater = false;
            }

        }

    }
}
