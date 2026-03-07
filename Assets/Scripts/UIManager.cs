using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace MultiUser
{
    public class UIManager : MonoBehaviour
    {
        VisualElement rootVisualElement;
        VisualElement panel;  
        Button hostButton;
        Button clientButton;
        Button serverButton;
        Label statusLabel;

        void OnEnable()
        {
            Debug.Log($"UIManager Enabled on: {SystemInfo.deviceUniqueIdentifier} | IsEditor: {Application.isEditor}");
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument component is MISSING on this GameObject!");
                return;
            }

            rootVisualElement = uiDocument.rootVisualElement;

            // Root should be transparent and non-blocking
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.alignItems = Align.FlexStart; 
            rootVisualElement.style.justifyContent = Justify.FlexStart;
            rootVisualElement.pickingMode = PickingMode.Position;
            rootVisualElement.style.backgroundColor = Color.clear;

            panel = new VisualElement();
            panel.style.flexDirection = FlexDirection.Column;
            panel.style.alignItems = Align.FlexStart;
            panel.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.85f); 
            panel.style.paddingTop = 10;
            panel.style.paddingBottom = 10;
            panel.style.paddingLeft = 10;
            panel.style.paddingRight = 10;
            panel.style.marginTop = 10;
            panel.style.marginLeft = 10;
            panel.style.width = 260;
            panel.style.borderTopLeftRadius = 6;
            panel.style.borderTopRightRadius = 6;
            panel.style.borderBottomLeftRadius = 6;
            panel.style.borderBottomRightRadius = 6;

            hostButton   = CreateButton("HostButton", "Host");
            clientButton = CreateButton("ClientButton", "Client");
            serverButton = CreateButton("ServerButton", "Server");
            statusLabel  = CreateLabel("StatusLabel", "Not Connected");

            panel.Add(hostButton);
            panel.Add(clientButton);
            panel.Add(serverButton);
            panel.Add(statusLabel);

            rootVisualElement.Clear();
            rootVisualElement.Add(panel);

            hostButton.clicked   += OnHostButtonClicked;
            clientButton.clicked += OnClientButtonClicked;
            serverButton.clicked += OnServerButtonClicked;

            var chatPanel = new VisualElement();
            chatPanel.style.position = Position.Absolute;
            chatPanel.style.bottom = 10; chatPanel.style.left = 10;
            chatPanel.style.width = 340; chatPanel.style.height = 220;
            chatPanel.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            chatPanel.style.paddingTop = chatPanel.style.paddingBottom =
            chatPanel.style.paddingLeft = chatPanel.style.paddingRight = 8;
            chatPanel.style.borderTopLeftRadius = chatPanel.style.borderTopRightRadius =
            chatPanel.style.borderBottomLeftRadius = chatPanel.style.borderBottomRightRadius = 6;

            var chatScroll = new ScrollView();
            chatScroll.pickingMode = PickingMode.Position;
            chatScroll.style.height = 150;

            var inputRow = new VisualElement();
            inputRow.style.flexDirection = FlexDirection.Row;
            inputRow.style.marginTop = 6;

            var chatField = new TextField();
            chatField.style.flexGrow = 1;
            chatField.style.color = Color.white;
            chatField.style.marginRight = 4;
            chatField.focusable = true;
            chatField.isDelayed = false;
            chatField.RegisterCallback<PointerDownEvent>(_ => chatField.Focus());
            chatField.pickingMode = PickingMode.Position;

            var sendBtn = CreateButton("SendChat", "Send");
            sendBtn.style.width = 60; sendBtn.style.marginBottom = 0;

            var declareBtn = CreateButton("DeclareWinner", "✓ Winner");
            declareBtn.style.width = 80; declareBtn.style.marginBottom = 0;
            declareBtn.style.backgroundColor = new Color(0.1f, 0.5f, 0.1f);

            chatPanel.pickingMode = PickingMode.Position; 
            chatScroll.pickingMode = PickingMode.Ignore; 
            inputRow.pickingMode = PickingMode.Position; 

            inputRow.Add(chatField);
            inputRow.Add(sendBtn);
            inputRow.Add(declareBtn);
            chatPanel.Add(chatScroll);
            chatPanel.Add(inputRow);
            rootVisualElement.Add(chatPanel);

            var chatManager = FindFirstObjectByType<ChatManager>();
            chatManager?.BindUI(chatScroll, chatField, sendBtn, declareBtn);

            chatField.RegisterCallback<FocusInEvent>(evt => {
                MultiUserPlayer.IsChatFocused = true;
            });

            chatField.RegisterCallback<FocusOutEvent>(evt => {
                MultiUserPlayer.IsChatFocused = false;
            });

            chatField.RegisterCallback<KeyDownEvent>(evt => {
                if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    var chatManager = FindFirstObjectByType<ChatManager>();
                    chatManager?.SendFromUI();
                    chatField.value = string.Empty;
                    chatField.Blur();
                    evt.StopPropagation();
                }
                if (evt.keyCode == KeyCode.Escape)
                {
                    chatField.Blur();
                    evt.StopPropagation();
                }
            });
        }

        void Update()
        {
            UpdateUI();
        }

        void OnDisable()
        {
            hostButton.clicked   -= OnHostButtonClicked;
            clientButton.clicked -= OnClientButtonClicked;
            serverButton.clicked -= OnServerButtonClicked;
        }

        void OnHostButtonClicked() => NetworkManager.Singleton.StartHost();
        void OnClientButtonClicked() => NetworkManager.Singleton.StartClient();
        void OnServerButtonClicked() => NetworkManager.Singleton.StartServer();

        private Button CreateButton(string name, string text)
        {
            var button = new Button();
            button.name = name;
            button.text = text;
            button.style.width = 240;
            button.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            button.style.color = Color.white;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.marginBottom = 6;
            button.style.height = 36;
            button.style.borderTopLeftRadius = 4;
            button.style.borderTopRightRadius = 4;
            button.style.borderBottomLeftRadius = 4;
            button.style.borderBottomRightRadius = 4;
            return button;
        }

        private Label CreateLabel(string name, string content)
        {
            var label = new Label();
            label.name = name;
            label.text = content;
            label.style.color = Color.white;
            label.style.fontSize = 14;
            label.style.marginTop = 4;
            return label;
        }

        void UpdateUI()
        {
            if (NetworkManager.Singleton == null)
            {
                SetStartButtons(false);
                SetStatusText("NetworkManager not found");
                return;
            }

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                SetStartButtons(true);
                SetStatusText("Not connected");
            }
            else
            {
                SetStartButtons(false);
                UpdateStatusLabels();
            }
        }

        void SetStartButtons(bool state)
        {
            hostButton.style.display   = state ? DisplayStyle.Flex : DisplayStyle.None;
            clientButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
            serverButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void SetStatusText(string text) => statusLabel.text = text;

        void UpdateStatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";
            string transport = "Transport: " + NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name;
            string modeText  = "Mode: " + mode;
            SetStatusText($"{transport}\n{modeText}");
        }
    }
}