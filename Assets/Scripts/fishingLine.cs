using UnityEngine;

public class fishingLine : MonoBehaviour
{
    public Transform origin;
    public Transform lure;

    private LineRenderer lineRenderer;
    private Transform[] lineSegments;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // for loop will be used to select the lineSegments array
        /*
        lineRenderer.positionCount = lineSegments.Length;
        for (int i = 0; i < lineSegments.Length; i++)
        {
            lineRenderer.SetPosition(i, lineSegments[i].position);
        }
        */
        lineRenderer.SetPosition(0, origin.position);
        lineRenderer.SetPosition(1, lure.position);
    }
}
