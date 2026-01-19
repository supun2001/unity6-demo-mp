using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    #region Class Variables
    [Header("Menu UI")]
    public GameObject menuPanel;
    public TMP_InputField joinCodeInput;
    public Button createButton;
    public Button joinButton;

    [Header("HUD UI")]
    public GameObject hudPanel;
    public TextMeshProUGUI roomIdText;
    public Button readyButton;
    public TextMeshProUGUI readyButtonText;
    public TextMeshProUGUI playerCountText;
    public TextMeshProUGUI playerListText;
    private bool isReady = false;

    [Header("General")]
    public Camera lobbyCamera;

    [Header("Skin Selection")]
    public SkinRegistry skinRegistry;
    public Image skinPreviewImage; 
    public TextMeshProUGUI skinNameText; 
    private int currentSkinIndex = 0;
    #endregion

    #region Class Methods
    private void Start()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("LobbyUI: Menu Panel is not assigned in the Inspector!");
        }

        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("LobbyUI: HUD Panel is not assigned in the Inspector!");
        }
        
        if (lobbyCamera != null) lobbyCamera.gameObject.SetActive(true);

        if (PlayerPrefs.HasKey("SelectedSkin"))
        {
            currentSkinIndex = PlayerPrefs.GetInt("SelectedSkin");
        }
        UpdateSkinUI();
    }

    private void Update()
    {
        if (NetworkManager.Instance != null && !string.IsNullOrEmpty(NetworkManager.Instance.currentRoomId))
        {
            if (menuPanel.activeSelf)
            {
                SwitchToHUD();
            }
            
            if (hudPanel.activeSelf)
            {
                if (roomIdText != null && roomIdText.text != $"Room Code: {NetworkManager.Instance.currentRoomId}")
                {
                    roomIdText.text = $"Room Code: {NetworkManager.Instance.currentRoomId}";
                }
            }
            // Allow ESC to leave ONLY when Game Started (HUD is hidden) and NOT in Menu
            else if (!menuPanel.activeSelf) 
            {
                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    LeaveRoom();
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.Room != null)
        {
             NetworkManager.Instance.Room.OnStateChange -= OnLobbyStateChange;
        }
        
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnPlayerAddedEvent -= OnPlayerListChanged;
            NetworkManager.Instance.OnPlayerRemovedEvent -= OnPlayerListChanged;
        }
    }
    #endregion

    public async void LeaveRoom()
    {
        if (NetworkManager.Instance != null)
        {
            await NetworkManager.Instance.LeaveGame();
        }
        
        SwitchToMenu();
    }

    private void SwitchToMenu()
    {
        menuPanel.SetActive(true);
        hudPanel.SetActive(false);
        
        if (lobbyCamera != null) lobbyCamera.gameObject.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Unsubscribe from events to prevent duplicates
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnPlayerAddedEvent -= OnPlayerListChanged;
            NetworkManager.Instance.OnPlayerRemovedEvent -= OnPlayerListChanged;
        }

        // Reset Ready State
        isReady = false;
        if (readyButtonText != null) readyButtonText.text = "READY";
        if (readyButton != null) 
        {
             ColorUtility.TryParseHtmlString("#33B46E", out Color notReadyColor);
             readyButton.image.color = notReadyColor;
        }
        
        if (joinButton != null) joinButton.interactable = true;
        if (createButton != null) createButton.interactable = true;
    }

    
    #region Switch UI to HUD
    private void SwitchToHUD()
    {
        menuPanel.SetActive(false);
        hudPanel.SetActive(true);
        
        if (roomIdText != null)
        {
            roomIdText.text = $"{NetworkManager.Instance.currentRoomId}";
            Debug.Log($"Setting Room Code Text to: {roomIdText.text}");
        }
        else
        {
            Debug.LogError("LobbyUI: Room ID Text (TMP) is not assigned in the Inspector!");
        }
        
        if (lobbyCamera != null) lobbyCamera.gameObject.SetActive(false);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Subscribe to state changes for UI updates
        if (NetworkManager.Instance != null)
        {
            if (NetworkManager.Instance.Room != null)
            {
                NetworkManager.Instance.Room.OnStateChange += OnLobbyStateChange;
            }
            
            NetworkManager.Instance.OnPlayerAddedEvent += OnPlayerListChanged;
            NetworkManager.Instance.OnPlayerRemovedEvent += OnPlayerListChanged;

            // Force initial update
            UpdateLobbyUI();

            // Disable player input while in lobby (waiting to ready up)
            SetLocalPlayerInput(false);
        }
    }
    #endregion

    #region Button Clicks
    private void OnPlayerListChanged(string id, Player player)
    {
        UpdateLobbyUI();

        // If the added player is the local player, enforce the input state
        if (NetworkManager.Instance != null && NetworkManager.Instance.Room != null)
        {
            if (id == NetworkManager.Instance.Room.SessionId)
            {
                // If HUD is active, rely on Ready state. If HUD is off (game started), enable movement.
                bool shouldMove = !hudPanel.activeSelf || isReady;
                SetLocalPlayerInput(shouldMove);
                
                // Also update cursor state if this is the initial spawn
                if (shouldMove)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
    }

    private void OnLobbyStateChange(MyRoomState state, bool isFirstState)
    {
        UpdateLobbyUI();
    }

    public async void OnCreateClicked()
    {
        if (createButton != null) createButton.interactable = false;
        await NetworkManager.Instance.CreateGame();
        
        // Sync Skin Immediately on Join
        SaveAndSyncSkin();
        
        if (createButton != null) createButton.interactable = true;
    }

    public async void OnJoinClicked()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.Room != null)
        {
            Debug.LogWarning("LobbyUI: Already in a room. Ignoring Join request.");
            return;
        }

        string code = joinCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code)) return;

        if (joinButton != null) joinButton.interactable = false;
        await NetworkManager.Instance.JoinGame(code);

        // Sync Skin Immediately on Join
        SaveAndSyncSkin();

        if (joinButton != null) joinButton.interactable = true;
    }

    public void OnReadyClicked()
    {
        isReady = !isReady;
        
        // Update Visuals
        if (readyButtonText != null)
        {
            readyButtonText.text = isReady ? "READY!" : "READY";
        }

        if (readyButton != null)
        {
            ColorUtility.TryParseHtmlString("#33B46E", out Color notReadyColor); 
            ColorUtility.TryParseHtmlString("#CCCC2D", out Color readyColor);

            if (isReady)
            {
                readyButton.image.color = readyColor; 
            }
            else
            {
                 readyButton.image.color = notReadyColor;
            }
        }
        
        NetworkManager.Instance.SendReadyState(isReady);
        
        // Toggle input and cursor based on Ready state
        SetLocalPlayerInput(isReady);
        
        if (isReady)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void UpdateLobbyUI()
    {
        if (NetworkManager.Instance == null || NetworkManager.Instance.Room == null || NetworkManager.Instance.Room.State == null) return;

        var players = NetworkManager.Instance.Room.State.players;
        if (players == null) return;

        // Update Player Count
        if (playerCountText != null)
        {
            playerCountText.text = $"Players: {players.Count}/4";
        }

        // Update Player List
        if (playerListText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int i = 1;
            foreach (var key in players.Keys)
            {
                Player player = (Player)players[key];
                string status = player.isReady ? "[READY]" : "[WAITING]";
                string prefix = (key == NetworkManager.Instance.Room.SessionId) ? " (You)" : "";
                sb.AppendLine($"{i}. Player{prefix} {status}");
                i++;
            }
            playerListText.text = sb.ToString();
        }
    }

    public void OnGameStarted()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        
        // Lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Ensure movement is enabled
        SetLocalPlayerInput(true);
    }

    private void SetLocalPlayerInput(bool enabled)
    {
        GameObject localPlayer = GameObject.Find("LocalPlayer");
        if (localPlayer != null)
        {
            var input = localPlayer.GetComponent<PlayerLocomotionInput>();
            if (input != null)
            {
                input.InputEnabled = enabled;
            }
        }
    }




    // Skin Selection
    public void OnNextSkinClicked()
    {
        if (skinRegistry == null || skinRegistry.skins.Length == 0) return;

        currentSkinIndex = (currentSkinIndex + 1) % skinRegistry.skins.Length;
        SaveAndSyncSkin();
    }

    public void OnPrevSkinClicked()
    {
        if (skinRegistry == null || skinRegistry.skins.Length == 0) return;

        currentSkinIndex--;
        if (currentSkinIndex < 0) currentSkinIndex = skinRegistry.skins.Length - 1;
        SaveAndSyncSkin();
    }

    private void SaveAndSyncSkin()
    {
        PlayerPrefs.SetInt("SelectedSkin", currentSkinIndex);
        PlayerPrefs.Save();
       
       UpdateSkinUI();

       if (NetworkManager.Instance != null && NetworkManager.Instance.Room != null)
       {
           NetworkManager.Instance.Room.Send("setSkin", currentSkinIndex);
       }
    }

    private void UpdateSkinUI()
    {
        if (skinRegistry == null || skinRegistry.skins.Length == 0) return;

       if (currentSkinIndex >= skinRegistry.skins.Length) currentSkinIndex = 0;

       // Get the current skin entry
       var skinEntry = skinRegistry.skins[currentSkinIndex];

       if (skinPreviewImage != null)
       {
           // Use the distinct UI preview sprite if available, otherwise fallback or leave null
           if (skinEntry.uiPreview != null)
           {
               skinPreviewImage.sprite = skinEntry.uiPreview;
           }
       }
       
       if (skinNameText != null)
       {
           // Use the custom name if set, otherwise default to "Skin X"
           if (!string.IsNullOrEmpty(skinEntry.skinName))
           {
               skinNameText.text = skinEntry.skinName;
           }
           else
           {
               skinNameText.text = $"Skin {currentSkinIndex + 1}";
           }
       }
    }
    #endregion
}
