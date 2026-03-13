using UnityEngine;
using UnityEngine.UI;

public class crosshairUI : MonoBehaviour
{
    public Image crosshairImg;
    public Color crosshairColor = Color.white;
    public Color interactiveColor = Color.black;

    public void setInteract(bool interact)
    {
        crosshairImg.color = interact ? interactiveColor : crosshairColor;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
