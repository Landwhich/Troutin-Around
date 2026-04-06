using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Burst.Intrinsics.X86;

public class StartGame : MonoBehaviour
{
    public void onClick()
    {
        SceneManager.LoadScene("MK_SCENE", LoadSceneMode.Single);
    }

    public void VR_launch()
    {
        SceneManager.LoadScene("Cayden", LoadSceneMode.Single);
    }
}
