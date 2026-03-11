using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Burst.Intrinsics.X86;

public class StartGame : MonoBehaviour
{
    public void onClick()
    {
        SceneManager.LoadScene(1);
    }

    public void VR_launch()
    {
        SceneManager.LoadScene(2);
    }
}
