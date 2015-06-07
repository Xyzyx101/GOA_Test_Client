using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
    public UDPTestServerConnection serverConnection;
    public PlayerController playerController;
    public Queue<Command> SendQueue {
        get {
            return sendQueue;
        }
    }
    public Queue<Command> ReceiveQueue {
        get {
            return receiveQueue;
        }
    }
    private Queue<Command> sendQueue = new Queue<Command>();
    private Queue<Command> receiveQueue = new Queue<Command>();

    private PlayerFactory playerFactory;
    public Transform[] spawnPoints;

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
        public const int SPAWN_LOCAL = 3;
        public const int SPAWN_REMOTE = 4;
    }

    void Awake() {
        playerFactory = GetComponent<PlayerFactory>();
    }

    void Start() {
        Command command = new Command() {
            playerId = 0,
            commandType = COMMANDTYPE.CONNECT,
            time = Time.time,
            positionX = 0f,
            positionY = 0f,
            positionZ = 0f,
            moveForward = 0f,
            moveSideways = 0f,
            jump = false,
            crouch = false
        };
        SendCommand(command);
    }

    // Update is called once per frame
    void Update() {
        FlushReceiveQueue();
    }

    public void SendCommand(Command command) {
        sendQueue.Enqueue(command);
    }

    public void ReceiveCommand(Command command) {
        receiveQueue.Enqueue(command);
    }

    public void SpawnLocal(int playerId) {
        localPlayer = playerFactory.SpawnLocalPlayer(playerId, spawnPoints[playerId].position.x, spawnPoints[playerId].position.z);
        localPlayer.setId(playerId);
        players.Add(playerId, localPlayer);
        playerController.Init();
    }

    public void SpawnRemote(int playerId) {
        Player remotePlayer = playerFactory.SpawnRemotePlayer(playerId, spawnPoints[playerId].position.x, spawnPoints[playerId].position.z);
        remotePlayer.setId(playerId);
        players.Add(playerId, remotePlayer);
    }

    public void DisconnectPlayer(int playerId) {
        players.Remove(playerId);
    }

    public Player GetLocalPlayer() {
        return localPlayer;
    }

    void FlushReceiveQueue() {
        Command command;
        Player player;
        while (receiveQueue.Count > 0) {
            command = (Command)receiveQueue.Dequeue();
            switch (command.commandType) {
                case COMMANDTYPE.SPAWN_LOCAL:
                    SpawnLocal(command.playerId);
                    break;
                case COMMANDTYPE.SPAWN_REMOTE:
                    SpawnRemote(command.playerId);
                    break;
                case COMMANDTYPE.MOVE:
                    player = players[command.playerId];
                    player.AddCommand(command);
                    break;
            }
        }
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
