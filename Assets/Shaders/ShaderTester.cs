using UnityEngine;

public class ComputeTestRunner : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture target;

    // int kernel;

    void Start()
    {
        // kernel = computeShader.FindKernel("CSMain");

        // target = new RenderTexture(Screen.width, Screen.height, 0);
        target = new RenderTexture(256, 256, 24);
        target.enableRandomWrite = true;
        target.Create();

        computeShader.SetTexture(0, "Result", target);
        computeShader.Dispatch(0, target.width / 8, target.height / 8, 1);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if(target == null)
        {
            target = new RenderTexture(256, 256, 24);
            target.enableRandomWrite = true;
            target.Create();
        }

        computeShader.SetTexture(0, "Result", target);
        computeShader.Dispatch(0, target.width / 8, target.height / 8, 1);
        Graphics.Blit(target, dest);
    }
}
