using UnityEngine;
using Colyseus;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colyseus.Schema;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    
    public string serverUrl = "ws://localhost:2567";
    public string roomName = "my_room";
    public GameObject playerPrefab; 

    private ColyseusClient client;
    private ColyseusRoom<MyRoomState> room;
    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    private void Awake() 
    { 
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        } 
        Instance = this; 
        DontDestroyOnLoad(gameObject); 
    }

    private async void Start()
    {
        client = new ColyseusClient(serverUrl);
        try
        {
            room = await client.JoinOrCreate<MyRoomState>(roomName);
            Debug.Log($"Connected with ID: {room.SessionId}");


            // Listen for player additions using Callbacks strategy (required for SDK 0.16+)
            room.OnStateChange += OnStateChange;
            
            var events = Colyseus.Schema.Callbacks.Get(room);
            events.OnAdd(state => state.players, (key, player) => OnPlayerAdded(key, player));
            events.OnRemove(state => state.players, (key, player) => OnPlayerRemoved(key, player));
        }
        catch (System.Exception e) { Debug.LogError(e.Message); }
    }

    private void OnPlayerAdded(string id, Player player)
    {
        Debug.Log($"🎮 Player added: {id}");
        Vector3 pos = new Vector3(player.x, player.y, player.z);
        GameObject obj = Instantiate(playerPrefab, pos, Quaternion.identity);
        
        bool isLocal = id == room.SessionId;
        
        NetworkPlayer np = obj.AddComponent<NetworkPlayer>();
        np.Initialize(player, isLocal);

        if (!isLocal)
        {
            var camera = obj.GetComponentInChildren<Camera>();
            if (camera)
            {
                camera.gameObject.SetActive(false);
            }
            
             var audioListener = obj.GetComponentInChildren<AudioListener>();
            if (audioListener) audioListener.enabled = false;

            obj.name = $"RemotePlayer_{id}";
        }
        else
        {
            obj.name = "LocalPlayer";
        }

        players.Add(id, obj);
    }

    private void OnPlayerRemoved(string id, Player player)
    {
        if (players.ContainsKey(id))
        {
            Destroy(players[id]);
            players.Remove(id);
        }
    }

    private void OnStateChange(MyRoomState state, bool isFirstState)
    {
        // Occurs when the room state is updated
    }

    public void SendPlayerUpdate(Vector3 pos, float rotY, Vector3 vel, float aX, float aY, bool g, bool j, Vector2 camRot)
    {
        if (room == null) return;
        
        room.Send("playerUpdate", new {
            x = pos.x, y = pos.y, z = pos.z,
            rotationY = rotY,
            velocityX = vel.x, velocityY = vel.y, velocityZ = vel.z,
            animInputX = aX, animInputY = aY,
            isGrounded = g, isJumping = j,
            cameraRotationX = camRot.x, cameraRotationY = camRot.y
        });
    }
}