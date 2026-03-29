using UnityEngine;


//this script is similar to the lureCollision file for the M&K version, just changed to work for VR
public class VR_LureCollision : MonoBehaviour
{
    //public GameObject fishingLure;
    //public Transform lureOrigin;

    public bool lureIsInWater;
    public float waterDragSpeed;

    Rigidbody lureRB;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lureRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (lureIsInWater)
        {
            lureRB.linearVelocity = Vector3.Lerp(lureRB.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 2.0f);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Water")
        {
            Debug.Log("The lure collided with water!");
            lureIsInWater = true;

            lureRB.linearVelocity = Vector3.zero;
            lureRB.linearDamping = waterDragSpeed;
        }
    }
}
