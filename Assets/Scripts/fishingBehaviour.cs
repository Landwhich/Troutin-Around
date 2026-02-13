using UnityEngine;
using UnityEngine.InputSystem;

public class fishingBehaviour : MonoBehaviour
{
    public GameObject fishingRod;
    public GameObject fishingLure;
    public lureCollision lureCollision;

    private bool toggleFishing;
    private bool lureIsCast;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        toggleFishing = false;
        lureIsCast = false;
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

            }

            if (Mouse.current.leftButton.wasPressedThisFrame && lureCollision.lureIsInWater == true)
            {
                // reel in fishing lure

                // raycast distance to rod tip


            }
        }
    }



}
