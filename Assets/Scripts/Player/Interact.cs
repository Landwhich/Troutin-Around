using UnityEngine;
using UnityEngine.InputSystem;

interface IInteractable{
    public void Interact();
}

public class Interact : MonoBehaviour
{

    public Camera playerCamera;
    public float InteractRange = 5.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        if (Keyboard.current.eKey.wasPressedThisFrame) {
            Ray r = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(r, out RaycastHit hitInfo, InteractRange)){
                if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
                    interactObj.Interact();
                }
            }
        }
    }
}
