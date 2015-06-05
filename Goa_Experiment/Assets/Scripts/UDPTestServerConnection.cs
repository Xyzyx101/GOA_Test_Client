using UnityEngine;
using System.Collections;

public class UDPTestServerConnection : MonoBehaviour {
    private GameManager gameManager;

    private void Awake() {
        GameObject gameManagerObj = GameObject.FindGameObjectWithTag("GameManager");
        gameManager = gameManagerObj.GetComponent<GameManager>() as GameManager;
    }

    private void Update() {
        // Get commands from server

    }

    public void SerializeAndSend(Command command) {
        string serializedCommand = SerializeCommand(command);
        StartCoroutine(FakeServer(serializedCommand));
    }

    string SerializeCommand(Command command) {
        JSONObject j = new JSONObject(JSONObject.Type.OBJECT);
        j.AddField("playerId", command.playerId);
        j.AddField("commandType", command.commandType);
        j.AddField("time", command.time);
        j.AddField("positionX", command.positionX);
        j.AddField("positionY", command.positionY);
        j.AddField("positionZ", command.positionZ);
        j.AddField("moveForward", command.moveForward);
        j.AddField("moveSideways", command.moveSideways);
        j.AddField("jump", command.jump);
        j.AddField("crouch", command.crouch);
        return j.Print();
    }

    Command DeserializeCommand(string jsonString) {
        JSONObject j = new JSONObject(jsonString);
        Command command = new Command() {
            playerId = int.Parse(j.GetField("playerId").ToString()),
            commandType = int.Parse(j.GetField("commandType").ToString()),
            positionX = float.Parse(j.GetField("positionX").ToString()),
            positionY = float.Parse(j.GetField("positionY").ToString()),
            positionZ = float.Parse(j.GetField("positionZ").ToString()),
            moveForward = float.Parse(j.GetField("moveForward").ToString()),
            moveSideways = float.Parse(j.GetField("moveSideways").ToString()),
            jump = bool.Parse(j.GetField("jump").ToString()),
            crouch = bool.Parse(j.GetField("crouch").ToString())
        };
        return command;
    }

    IEnumerator FakeServer(string commandString) {
        yield return StartCoroutine(FakeLag());
        gameManager.ReceiveCommand(DeserializeCommand(commandString));
    }

    IEnumerator FakeLag() {
        // Fake 80ms to 120ms of lag
        float randomLag = 0.08f + Random.value * 0.04f;
        Debug.Log("Lag:" + randomLag * 1000 + "ms");
        yield return new WaitForSeconds(randomLag);
    }
}
