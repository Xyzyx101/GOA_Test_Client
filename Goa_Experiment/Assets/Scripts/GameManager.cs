using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
    public UDPTestServerConnection serverConnection;
    public PlayerController playerController;
    private Queue<Command> sendQueue = new Queue<Command>();
    private Queue<Command> receiveQueue = new Queue<Command>();

    private PlayerFactory playerFactory;
    public Transform[] spawnPoints;

    private int nextSpawnIndex = -1;

    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    private Player localPlayer;
    private int localPlayerId;
    private Player[] otherPlayers;

    //FIXME
    private int dummyServerId; //replace this with the id from the server

    public struct COMMANDTYPE {
        public const int MOVE = 0;
        public const int CONNECT = 1;
        public const int DISCONNECT = 2;
    }

    void Awake() {
        playerFactory = GetComponent<PlayerFactory>();
    }

    void Start() {
        int spawnIndex = NextSpawnIndex();
        localPlayer = playerFactory.SpawnLocalPlayer(spawnIndex, spawnPoints[spawnIndex].position.x, spawnPoints[spawnIndex].position.z);
        localPlayerId = ++dummyServerId;
        localPlayer.setId(localPlayerId);
        players.Add(localPlayerId, localPlayer);
        ConnectPlayer(++dummyServerId);
        ConnectPlayer(++dummyServerId);
        playerController.Init();
    }

    // Update is called once per frame
    void Update() {
        FlushQueue();
    }

    public void SendCommand(Command command) {
        sendQueue.Enqueue(command);
    }

    public void ReceiveCommand(Command command) {
        receiveQueue.Enqueue(command);
    }

    public void ConnectPlayer(int playerId) {
        int spawnIndex = NextSpawnIndex();
        Player remotePlayer = playerFactory.SpawnRemotePlayer(spawnIndex, spawnPoints[spawnIndex].position.x, spawnPoints[spawnIndex].position.z);
        players.Add(playerId, remotePlayer);
    }

    public void DisconnectPlayer(int playerId) {
        players.Remove(playerId);
    }

    public Player GetLocalPlayer() {
        return localPlayer;
    }

    void FlushQueue() {
        Command command;
        Player player;
        while (receiveQueue.Count > 0) {
            command = (Command)receiveQueue.Dequeue();
            player = players[command.playerId];
            player.AddCommand(command);
        }
        while (sendQueue.Count > 0) {
            command = (Command)sendQueue.Dequeue();
            serverConnection.SerializeAndSend(command);
        }
    }

    int NextSpawnIndex() {
        return nextSpawnIndex = ++nextSpawnIndex % spawnPoints.Length;
    }
}

public struct Command {
    public int playerId;
    public int commandType;
    public float time;
    public float positionX;
    public float positionY;
    public float positionZ;
    public float moveForward;
    public float moveSideways;
    public bool jump;
    public bool crouch;
}
