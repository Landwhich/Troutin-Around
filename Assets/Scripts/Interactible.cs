using UnityEngine;

public class interactible : MonoBehaviour
{
    public string promptMessage = "press E";
    public virtual void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }
}
