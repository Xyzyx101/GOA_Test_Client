using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

public class UDPTestServerConnection : MonoBehaviour {
    private GameManager gameManager;
    Socket sckCommunication;
    EndPoint epLocal, epRemote;
    string txtLocalIp;
    string txtServerIp;
    int txtLocalPort;
    int txtServerPort;
    int delay = 200;
    string txtgui;
    byte[] buffer;
    int numberBytes;
    System.Text.UTF8Encoding enc;
    private Queue<string> msgQueue = new Queue<string>();
    float updateDelta;

    private void Awake() {
        GameObject gameManagerObj = GameObject.FindGameObjectWithTag("GameManager");
        gameManager = gameManagerObj.GetComponent<GameManager>() as GameManager;

        txtLocalIp = GetLocalIP();
        txtLocalPort = new System.Random().Next(50000, 51000);

        txtServerIp = "127.0.0.1";
        txtServerPort = 27660;

        updateDelta = 0;

        enc = new System.Text.UTF8Encoding();
    }

    private string GetLocalIP() {

        //FIXME
        return "127.0.0.1";

        IPHostEntry host;
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList) {
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                return ip.ToString();
            }
        }
        return "192.168.2.11";
    }

    void Start() {
        numberBytes = 1024;
        Debug.Log("UDPmanager start...");

        sckCommunication = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        sckCommunication.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        txtgui = "port=" + txtLocalPort + ',' + txtLocalIp;
        // bind socket                        
        epLocal = new IPEndPoint(IPAddress.Parse(txtLocalIp), txtLocalPort);
        sckCommunication.Bind(epLocal);

        // connect to remote ip and port 
        epRemote = new IPEndPoint(IPAddress.Parse(txtServerIp), txtServerPort);

        //FIXME
        sckCommunication.Connect(epRemote);

        // starts to listen to an specific port
        Debug.Log("UDPmanager listening on port " + txtLocalPort);

        buffer = new byte[numberBytes];

        sckCommunication.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None,
                                          ref epRemote, new AsyncCallback(OperatorCallBack), buffer);
    }

    // Update is called once per frame
    void Update() {
        Queue<Command> sendQueue = gameManager.SendQueue;
        while (sendQueue.Count > 0) {
            SendCommand(sendQueue.Dequeue());
        }

        while (msgQueue.Count > 0) {
            Command command = DeserializeCommand(msgQueue.Dequeue());
            if (gameManager.GetLocalPlayer() != null && command.playerId == gameManager.GetLocalPlayer().getId()) {
                updateDelta = Time.realtimeSinceStartup - command.time;
            }
            gameManager.ReceiveQueue.Enqueue(command);
        }
    }

    private void OperatorCallBack(IAsyncResult ar) {
        try {
            int size = sckCommunication.EndReceiveFrom(ar, ref epRemote);

            // check if theres actually information
            if (size > 0) {
                // used to help us on getting the data
                byte[] aux = new byte[numberBytes];

                // gets the data
                aux = (byte[])ar.AsyncState;

                // converts from data[] to string

                string msg = enc.GetString(aux);
                msgQueue.Enqueue(msg);
            }

            // starts to listen 
            buffer = new byte[numberBytes];

            sckCommunication.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None,
                                              ref epRemote, new AsyncCallback(OperatorCallBack), buffer);
        } catch (Exception exp) {
            print(exp.ToString());
            txtgui += exp.ToString();
        }
    }

    private void SendCommand(Command command) {
        string commandString = SerializeCommand(command);
        byte[] msg = new byte[numberBytes];
        msg = enc.GetBytes(commandString);
        int size = enc.GetByteCount(commandString);
        sckCommunication.Send(msg, size, SocketFlags.None);
    }

    void OnGUI() {
        txtgui = txtLocalIp + ":" + txtLocalPort + " -> " + (updateDelta * 1000).ToString("F0") + "ms";
        GUI.Label(new Rect(25, 25, 400, 150), txtgui);
    }

    string SerializeCommand(Command command) {
        JSONObject j = new JSONObject(JSONObject.Type.OBJECT);
        j.AddField("PlayerId", command.playerId);
        j.AddField("CommandType", command.commandType);
        j.AddField("Time", command.time);
        j.AddField("PositionX", command.positionX);
        j.AddField("PositionY", command.positionY);
        j.AddField("PositionZ", command.positionZ);
        j.AddField("MoveForward", command.moveForward);
        j.AddField("MoveSideways", command.moveSideways);
        j.AddField("Jump", command.jump);
        j.AddField("Crouch", command.crouch);
        Debug.Log(j.Print());
        return j.Print();
    }

    Command DeserializeCommand(string jsonString) {
        JSONObject j = new JSONObject(jsonString);
        Command command = new Command() {
            playerId = int.Parse(j.GetField("PlayerId").ToString()),
            commandType = int.Parse(j.GetField("CommandType").ToString()),
            time = float.Parse(j.GetField("Time").ToString()),
            positionX = float.Parse(j.GetField("PositionX").ToString()),
            positionY = float.Parse(j.GetField("PositionY").ToString()),
            positionZ = float.Parse(j.GetField("PositionZ").ToString()),
            moveForward = float.Parse(j.GetField("MoveForward").ToString()),
            moveSideways = float.Parse(j.GetField("MoveSideways").ToString()),
            jump = bool.Parse(j.GetField("Jump").ToString()),
            crouch = bool.Parse(j.GetField("Crouch").ToString())
        };
        return command;
    }

    IEnumerator FakeServer(string commandString) {
        yield return StartCoroutine(FakeLag());
        gameManager.ReceiveCommand(DeserializeCommand(commandString));
    }

    IEnumerator FakeLag() {
        // Fake 80ms to 120ms of lag
        float randomLag = 0.08f + UnityEngine.Random.value * 0.04f;
        //Debug.Log("Lag:" + randomLag * 1000 + "ms");
        yield return new WaitForSeconds(randomLag);
    }
}