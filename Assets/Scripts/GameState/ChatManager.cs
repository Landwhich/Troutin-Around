using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance;

    private ScrollView chatScrollView;
    private TextField chatInput;
    private Button sendButton;
    private Button declareWinnerButton; 

    private List<string> messageHistory = new();

    public System.Action<string> OnWinnerDeclared;

    void Awake() => Instance = this;

    public void BindUI(ScrollView scroll, TextField input, Button send, Button declareWinner)
    {
        chatScrollView   = scroll;
        chatInput        = input;
        sendButton       = send;
        declareWinnerButton = declareWinner;

        sendButton.clicked += SendChatMessage;

        // if (declareWinnerButton != null)
        //     declareWinnerButton.clicked += TryDeclareSelectedWinner;

        if (declareWinnerButton != null)
            declareWinnerButton.style.display = DisplayStyle.None;
    }

    public override void OnNetworkSpawn()
    {
        // Only host can declare a winner
        if (IsHost && declareWinnerButton != null)
            declareWinnerButton.style.display = DisplayStyle.Flex;
    }

    void SendChatMessage()
    {
        if (chatInput == null) return;
        string text = chatInput.value.Trim();
        if (string.IsNullOrEmpty(text)) return;

        chatInput.value = string.Empty;
        chatInput.Focus();

        string playerName = $"Player {NetworkManager.Singleton.LocalClientId}";
        SendChatServerRpc(playerName, text);
    }

    [Rpc(SendTo.Server)]
    void SendChatServerRpc(string playerName, string message, RpcParams rpcParams = default)
    {
        string formatted = $"<b>{playerName}:</b> {message}";
        BroadcastMessageClientRpc(formatted);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void BroadcastMessageClientRpc(string formattedMessage)
    {
        messageHistory.Add(formattedMessage);
        AppendToChat(formattedMessage);
    }

    void AppendToChat(string text)
    {
        if (chatScrollView == null) return;
        var label = new Label(text);
        label.style.color = Color.white;
        label.style.fontSize = 13;
        label.style.marginBottom = 2;
        label.style.whiteSpace = WhiteSpace.Normal;
        chatScrollView.Add(label);

        chatScrollView.schedule.Execute(() =>
            chatScrollView.scrollOffset = new Vector2(0, float.MaxValue)
        ).ExecuteLater(50);
    }
    private string selectedPlayerName = null;
    void TryDeclareSelectedWinner()
    {
        if (!IsHost || string.IsNullOrEmpty(selectedPlayerName)) return;
        DeclareWinnerServerRpc(selectedPlayerName);
    }

    public void DeclareWinner(string playerName)
    {
        if (!IsHost) return;
        DeclareWinnerServerRpc(playerName);
    }

    [Rpc(SendTo.Server)]
    void DeclareWinnerServerRpc(string winnerName)
    {
        AnnounceWinnerClientRpc(winnerName);
    }

    [Rpc(SendTo.ClientsAndHost)]
    void AnnounceWinnerClientRpc(string winnerName)
    {
        string announcement = $"🏆 <b>{winnerName} guessed correctly and wins!</b> 🏆";
        AppendToChat(announcement);
        OnWinnerDeclared?.Invoke(winnerName);
    }

    public void SelectChatEntry(string playerName)
    {
        if (!IsHost) return;
        selectedPlayerName = playerName;
        Debug.Log($"[ChatManager] Host selected: {playerName} as potential winner.");
    }

    public void SendFromUI()
    {
        SendChatMessage();
    }
}