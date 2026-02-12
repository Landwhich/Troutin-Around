using UnityEngine;

public class ComputeTestRunner : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture target;

    int kernel;

    void Start()
    {
        kernel = computeShader.FindKernel("CSMain");

        target = new RenderTexture(Screen.width, Screen.height, 0);
        target.enableRandomWrite = true;
        target.Create();

        computeShader.SetTexture(kernel, "Result", target);
        computeShader.Dispatch(kernel, Screen.width / 8, Screen.height / 8, 1);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(target, dest);
    }
}
