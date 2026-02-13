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
        if (Keyboard.current.fKey.wasPressedThisFrame)
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
            // have fishing rod track mouse


            if (Mouse.current.leftButton.wasReleasedThisFrame && lureIsCast != true)
            {
                // cast fishing lure
                lureIsCast=true;

                fishingRodAnimator.Play("FishingRodCast");
            }

            if (Mouse.current.leftButton.wasPressedThisFrame && lureCollision.lureIsInWater == true)
            {
                // reel in fishing lure

                // raycast distance to rod tip


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
