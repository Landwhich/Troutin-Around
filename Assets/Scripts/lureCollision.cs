using UnityEngine;

public class lureCollision : MonoBehaviour
{
    public bool lureIsInWater;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Water")
        {
            lureIsInWater = true;
        }
    }
}
