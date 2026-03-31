using UnityEngine;

public class mackerelBehaviour : MonoBehaviour
{
    public GameObject fishingLure;
    public Transform lureOrigin;

    public GameObject mackerel;
    public Transform mackerelTip;

    Rigidbody mackerelRB;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mackerelRB = GetComponent<Rigidbody>();
        transform.Rotate(0, 90, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void attachMackerel()
    {
        mackerelTip.transform.position = Vector3.MoveTowards(mackerelTip.transform.position, fishingLure.transform.position, 30 * Time.deltaTime);
    }

}
