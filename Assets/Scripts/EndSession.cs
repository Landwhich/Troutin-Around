using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSession : NetworkBehaviour
{
    public static EndSession Instance;

    void Awake() => Instance = this;

    public void HostEndSession() {
        if (!IsHost) return;
        NotifySessionEndedClientRpc("The host has ended the session.");
    }

    [Rpc(SendTo.ClientsAndHost)]
    void NotifySessionEndedClientRpc(string reason)
    {
        Debug.Log($"[EndSession] Session ended: {reason}");
        ChatManager.Instance?.DeclareWinner(""); 

        Invoke(nameof(ShutdownAndReturnToMenu), 2.0f);
    }

    void ShutdownAndReturnToMenu()
    {
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
            NetworkManager.Singleton.Shutdown();
    }


    public void ClientLeave()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null &&
            (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient))
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}