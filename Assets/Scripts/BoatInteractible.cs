using UnityEngine;

public class BoatInteractible : interactible
{
    public Transform boatSeat;
    public GameObject player;

    private boatController boatController;

    private void Start()
    {
        boatController = GetComponent<boatController>();
    }

    public override void Interact()
    {

        player.transform.SetParent(boatSeat);
        player.GetComponent<PlayerController>().enabled = false;

        player.transform.position = boatSeat.position; 
        player.transform.rotation = boatSeat.rotation;

        boatController.isDriving = true;

        Debug.Log("YOU ARE NOW IN THE BOAT");
    }
}
