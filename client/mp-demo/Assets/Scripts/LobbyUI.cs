using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
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
        if (NetworkManager.Instance != null && NetworkManager.Instance.currentRoomId != null)
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
                UpdateLobbyUI();
            }
        }
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
            if (isReady)
            {
                ColorUtility.TryParseHtmlString("#33B46E", out Color notReadyColor); 
                ColorUtility.TryParseHtmlString("#CCCC2D", out Color readyColor);
                readyButton.image.color = readyColor; 
            }
            else
            {
                 readyButton.image.color = Color.white;
            }
        }
        
        NetworkManager.Instance.SendReadyState(isReady);
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
    }

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

       if (skinPreviewImage != null)
       {
           Texture2D tex = skinRegistry.skins[currentSkinIndex];
           Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
           skinPreviewImage.sprite = sprite;
       }
       
       if (skinNameText != null)
       {
           skinNameText.text = $"Skin {currentSkinIndex + 1}";
       }
    }
}
