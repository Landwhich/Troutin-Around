using UnityEngine;

public class lureCollision : MonoBehaviour
{
    public GameObject fishingLure;
    public Transform lureOrigin;

    public bool lureIsInWater;
    public float lureReelSpeed;

    public GameObject mackerel;

    Rigidbody lureRB;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lureRB = fishingLure.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Water")
        {
            Debug.Log("The lure collided with water!");
            
            lureRB.useGravity = false;
            lureRB.linearVelocity = Vector3.zero;
            lureRB.angularVelocity = Vector3.zero;
            lureIsInWater = true;
        }
    }

    public void reelLure()
    {
        mackerel.SetActive(true);

        if (fishingLure.transform.position != lureOrigin.transform.position)
        {
            fishingLure.transform.position = Vector3.MoveTowards(fishingLure.transform.position, lureOrigin.transform.position,
                                                                    lureReelSpeed * Time.deltaTime);
        }
        else if (fishingLure.transform.position == lureOrigin.transform.position)
        {
            lureIsInWater = false;
        }
    }
}
