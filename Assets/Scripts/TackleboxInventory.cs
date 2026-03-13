using UnityEngine;

public class TackleboxInventory : MonoBehaviour
{
    public GameObject inventoryUI;
    private bool playerNearby = false;

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            inventoryUI.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            inventoryUI.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}