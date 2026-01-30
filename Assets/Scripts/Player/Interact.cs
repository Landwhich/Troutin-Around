using UnityEngine;
using UnityEngine.InputSystem;

interface IInteractable {
    public void Interact();
    public void Grab();
}

public class Interact : MonoBehaviour
{
    private GameObject objectBuffer = null;
    private Vector3 positionBuffer;
    private Quaternion rotationBuffer;
    public Camera playerCamera;
    public float InteractRange = 5.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        if (Keyboard.current.eKey.wasPressedThisFrame) {
            Ray r = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(r, out RaycastHit hit, InteractRange)){
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
                    interactObj.Interact();
                }
            }
        }
        if (Keyboard.current.qKey.wasPressedThisFrame) {
            Ray r = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(r, out RaycastHit hit, InteractRange)){
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
                    interactObj.Grab();
                }
            }
        }
        if (Mouse.current.middleButton.wasPressedThisFrame) {
            Ray r = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(r, out RaycastHit hit, InteractRange)){
                if (hit.collider.CompareTag("Copyable")) {
                    objectBuffer = hit.collider.gameObject;
                    positionBuffer.y = objectBuffer.transform.position.y;
                    rotationBuffer = objectBuffer.transform.rotation;
                    Debug.Log("Test Copy");
                }
            }
        }
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Ray r = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(r, out RaycastHit hit, InteractRange)){
                if (hit.collider.gameObject.TryGetComponent(out IInteractable interactObj)) {
                    Destroy(hit.collider.gameObject);
                }
            }
        }
        if (Mouse.current.rightButton.wasPressedThisFrame && objectBuffer != null) {
            Ray r = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(r, out RaycastHit hit, InteractRange)){
                Instantiate(objectBuffer, new Vector3(hit.point.x, positionBuffer.y, hit.point.z), rotationBuffer);
            }
        }
    }
}
