using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractions : MonoBehaviour
{
    public float interactionRange = 10f;
    public Camera playerCam;
    public TextMeshProUGUI interactionText;

    private interactible currentInteractable;

    void Update()
    {
        Ray ray = playerCam.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2)
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            currentInteractable = hit.collider.GetComponent<interactible>();

            if (currentInteractable != null)
            {
                interactionText.gameObject.SetActive(true);
                interactionText.text = currentInteractable.promptMessage;

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    currentInteractable.Interact();
                }

                return;
            }
        }

        interactionText.gameObject.SetActive(false);
        currentInteractable = null;
    }
}
