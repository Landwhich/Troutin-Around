using UnityEngine;
using UnityEngine.InputSystem;

public class boatController : MonoBehaviour
{
    public float speed = 500f;
    public float turnSpeed = 200f;

    public bool isDriving = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        if (!isDriving) return;

        float move = 0f;
        float turn = 0f;

        if (Keyboard.current.wKey.isPressed)
            move = 1f;

        if (Keyboard.current.sKey.isPressed)
            move = -1f;

        if (Keyboard.current.aKey.isPressed)
            turn = -1f;

        if (Keyboard.current.dKey.isPressed)
            turn = 1f;

        transform.Translate(Vector3.left * move * speed * Time.deltaTime);
        transform.Rotate(Vector3.up * turn * turnSpeed * Time.deltaTime);
    }
}
