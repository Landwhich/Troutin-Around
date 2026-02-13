using UnityEngine;
using UnityEngine.InputSystem;

public class fishingBehaviour : MonoBehaviour
{
    public GameObject fishingRod;
    public Animator fishingRodAnimator;

    public float lureThrowForce;

    public GameObject fishingLure;
    public Transform lureOrigin;
    public lureCollision lureCollision;

    Rigidbody lureRB;

    private bool toggleFishing;
    private bool lureIsCast;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        toggleFishing = false;
        lureIsCast = false;

        lureRB = fishingLure.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame && lureIsCast == false)
        {
            toggleFishing = !toggleFishing;
            
            if (toggleFishing == true)
            {
                Debug.Log("Fishing is enabled!");
            }
            else if (toggleFishing == false)
            {
                Debug.Log("Fishing is disabled!");
            }
        }


        if (toggleFishing == true) {
            // later: have fishing rod track mouse on activating fishing mode

            if (Mouse.current.leftButton.wasReleasedThisFrame && lureIsCast == false)
            {
                // cast fishing lure
                lureIsCast=true;
                Debug.Log("Lure is cast = " + lureIsCast);
                
                fishingRodAnimator.Play("FishingRodCast");  // animation which triggers throwLure()
            }

            if (Mouse.current.rightButton.IsPressed() == true && lureCollision.lureIsInWater == true)
            {
                // TO BE CHANGED TO LEFT BUTTON ONCE lureIsCast BOOLEANS ACTUALLY WORK

                // reel in fishing lure

                // now: button press to trigger lure to move back to fishingRodTip
                // later: raycast distance to rod tip, hold down to reel along water's surface

                Debug.Log("Reeling in lure!");

                lureCollision.reelLure();
            }
        }
    }

    public void throwLure()
    {
        // to be called by an event in the FishingRodCast animation used by FishingRodGroup
        lureRB.useGravity = true;
        lureRB.linearVelocity = Vector3.zero;

        fishingLure.transform.position = lureOrigin.position;
        fishingLure.SetActive(true);

        lureRB.AddForce(lureOrigin.forward * lureThrowForce, ForceMode.Impulse);

        Debug.Log("Lure has been thrown!");
    }
}
