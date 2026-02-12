using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 12.0f;

    public PlayerController player;
    public CharacterController controller;
    public Transform cameraTransform;

    public Vector3 CurrentMove { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveInput = Keyboard.current != null ? new Vector2 (
                (Keyboard.current.aKey.isPressed ? -1 : 0) + (Keyboard.current.dKey.isPressed ?  1 : 0),
                (Keyboard.current.wKey.isPressed ?  1 : 0) + (Keyboard.current.sKey.isPressed ? -1 : 0)
            ) : Vector2.zero;
        Vector3 move = cameraTransform.right * moveInput.x + cameraTransform.forward * moveInput.y;
        move.y = 0;
        CurrentMove = move * speed;
    }

    void FixedUpdate() {
        controller.Move(CurrentMove * Time.fixedDeltaTime);
    }
}