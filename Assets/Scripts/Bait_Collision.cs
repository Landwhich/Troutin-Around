using UnityEngine;

public class Bait_Collision : MonoBehaviour
{
    public bool caughtFish = false;
    //GameObject coughtFishObject;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Fish")
        {
            //Debug.Log("THE BAIT WORKED!");
            caughtFish = true;
        }
    }
}
