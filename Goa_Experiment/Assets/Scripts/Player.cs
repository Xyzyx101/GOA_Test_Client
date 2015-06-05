using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(ThirdPersonCharacter))]
public class Player : MonoBehaviour {

    private ThirdPersonCharacter character;
    private int id;
    private Queue<Command> commands;
    // Use this for initialization
    void Awake() {
        commands = new Queue<Command>(64);
        character = GetComponent<ThirdPersonCharacter>();
    }

    // Update is called once per frame
    void Update() {
        FlushCommandQueue();
    }

    public void AddCommand(Command command) {
        commands.Enqueue(command);
    }

    public void setId(int _id) {
        id = _id;
    }

    public int getId() {
        return id;
    }

    void FlushCommandQueue() {
        Command command;
        while (commands.Count > 0) {
            command = (Command)commands.Dequeue();
            ExecuteCommand(command);
        }
    }

    void ExecuteCommand(Command command) {
        switch (command.commandType) {
            case GameManager.COMMANDTYPE.MOVE:
                //transform.position = new Vector3(command.positionX, command.positionY, command.positionZ);
                character.Move(new Vector3(command.moveSideways, 0, command.moveForward), command.crouch, command.jump);
                break;
        }
    }
}
