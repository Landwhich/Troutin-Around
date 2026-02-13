using UnityEngine;

public class lureCollision : MonoBehaviour
{
    public bool lureIsInWater;
    public GameObject fishingLure;

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
        if (collision.gameObject.CompareTag("Water"))
        {
            Debug.Log("The lure collider with water!");
            
            lureRB.useGravity = false;
            lureRB.linearVelocity = Vector3.zero;
            lureRB.angularVelocity = Vector3.zero;
            lureIsInWater = true;
        }
    }
}
