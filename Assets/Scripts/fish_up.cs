using UnityEngine;

public class FishBucketTrigger : MonoBehaviour
{
    public Transform fish_in_bucket;

    private float[] yPositions = { 27.412f, 27.49f, 27.73f, 27.921f, 28.146f };
    private int currentStep = 0;
    private bool playerInside = false;

    Vector3 startPosition = new Vector3(42.66f, 27.412f, 169.51f);

    void Start()
    {
        fish_in_bucket.position = startPosition;
        currentStep = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerInside)
        {
            playerInside = true;

            if (currentStep < yPositions.Length - 1)
            {
                currentStep++;

                Vector3 pos = fish_in_bucket.position;
                pos.y = yPositions[currentStep];
                fish_in_bucket.position = pos;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
}