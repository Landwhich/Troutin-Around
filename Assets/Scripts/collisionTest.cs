using UnityEngine;

public class collisionTest : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Water")
        {
            Debug.Log("The cube collider with water!");
        }
    }
}
