using UnityEngine;

public class BoatInteractible : interactible
{
    public Transform boatSeat;
    public GameObject player;

    public float exitNudgeDistance = 1.5f;
    public float exitNudgeSpeed = 3f;

    public ParticleSystem wakeParticles;
    public ParticleSystem splashParticles;

    private boatController boatController;
    private CharacterController characterController;

    private void Start()
    {
        boatController    = GetComponent<boatController>();
        characterController = player.GetComponent<CharacterController>();

        if (wakeParticles  != null) wakeParticles.Stop();
        if (splashParticles != null) splashParticles.Stop();
    }

    public override void Interact()
    {
        if (!boatController.isDriving)
        {
            EnterBoat();
        }
        else
        {
            ExitBoat();
        }
    }

    private void EnterBoat()
    {
        player.transform.SetParent(boatSeat);
        player.transform.position = boatSeat.position;
        player.transform.rotation = boatSeat.rotation;

        player.GetComponent<PlayerController>().enabled = false;

        if (characterController != null)
            characterController.enabled = false;

        boatController.isDriving = true;

        if (wakeParticles  != null) wakeParticles.Play();
        if (splashParticles != null) splashParticles.Play();

        Debug.Log("YOU ARE NOW IN THE BOAT");
    }

    private void ExitBoat()
    {
        player.transform.SetParent(null);

        Vector3 exitDirection = -transform.right;
        Vector3 exitPosition  = boatSeat.position + exitDirection * exitNudgeDistance;

        exitPosition.y = boatSeat.position.y;
        player.transform.position = exitPosition;

        if (characterController != null)
            characterController.enabled = true;

        player.GetComponent<PlayerController>().enabled = true;

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = exitDirection * exitNudgeSpeed;
        }

        boatController.isDriving = false;

        if (wakeParticles  != null) wakeParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (splashParticles != null) splashParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        Debug.Log("YOU ARE OUT OF THE BOAT");
    }
}