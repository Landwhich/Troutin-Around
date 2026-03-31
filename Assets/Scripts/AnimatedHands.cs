using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class AnimatedHands : MonoBehaviour
{

    public InputActionProperty triggerValue;
    public InputActionProperty gripValue;

    public Animator Hands;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float trigger = triggerValue.action.ReadValue<float>();
        float grip = gripValue.action.ReadValue<float>();

        Hands.SetFloat("Trigger", trigger);
        Hands.SetFloat("Grip", grip);
    }
}
