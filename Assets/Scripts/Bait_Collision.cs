using UnityEngine;

public class Bait_Collision : MonoBehaviour
{
    public bool caughtFish = false;
    public string fishName = "";
    private void OnTriggerEnter(Collider collision)
    {
        if (caughtFish) return;

        if (collision.gameObject.CompareTag("Fish"))
        {
            CatchFish("Fish", "THE BAIT WORKED!");
        }
        else if (collision.gameObject.CompareTag("Trout"))
        {
            CatchFish("Trout", "TROUT");
        }
        else if (collision.gameObject.CompareTag("Bass"))
        {
            CatchFish("Bass", "BASS");
        }
        else if (collision.gameObject.CompareTag("Panfish"))
        {
            CatchFish("Panfish", "Panfish");
        }
        else if (collision.gameObject.CompareTag("Catfish"))
        {
            CatchFish("Catfish", "CatFish");
        }
    }
    void CatchFish(string name, string logMessage)
    {
        Debug.Log(logMessage);
        fishName = name;
        caughtFish = true;
    }
}
