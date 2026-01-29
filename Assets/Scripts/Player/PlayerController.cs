using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 12.0f;
    public float mouseSensitivity = 100.0f;

    public CharacterController controller;
    public Transform cameraTransform;

    Vector2 rotation = new Vector2(0.0f, 0.0f);

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
                (Keyboard.current.aKey.isPressed ? -1 : 0) + (Keyboard.current.dKey.isPressed ? 1 : 0),
                (Keyboard.current.wKey.isPressed ? 1 : 0) + (Keyboard.current.sKey.isPressed ? -1 : 0)
            ) : Vector2.zero;

        Vector3 move = cameraTransform.right * moveInput.x + cameraTransform.forward * moveInput.y;
        move.y = 0;
        controller.Move(move*speed*Time.deltaTime);

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        rotation.y -= mouseY;
        rotation.x += mouseX;
        rotation.y = Mathf.Clamp(rotation.y, -80f, 80f);
        // rotation.x = Mathf.Clamp(rotation.x, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(rotation.y, rotation.x, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
