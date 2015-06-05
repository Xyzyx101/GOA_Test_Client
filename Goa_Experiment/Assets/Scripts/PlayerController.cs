using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;


public class PlayerController : MonoBehaviour {
    private GameManager gameManager;
    private Player localPlayer;
    private Transform m_Cam;                  // A reference to the main camera in the scenes transform
    private Vector3 m_CamForward;             // The current forward direction of the camera
    private Vector3 m_Move;
    private bool m_Jump;                      // the world-relative desired move direction, calculated from the camForward and user input.
    private bool m_Crouch;

    private bool playerInitialized = false;

    public void Init() {
        playerInitialized = true;
        GameObject gameManagerObj = GameObject.FindGameObjectWithTag("GameManager");
        gameManager = gameManagerObj.GetComponent<GameManager>() as GameManager;
        localPlayer = gameManager.GetLocalPlayer();
    }

    private void Start() {
        // get the transform of the main camera
        if (Camera.main != null) {
            m_Cam = Camera.main.transform;
        } else {
            Debug.LogWarning(
                "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
            // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
        }

        // get the third person character ( this should never be null due to require component )
        //m_Character = GetComponent<ThirdPersonCharacter>();
    }


    private void Update() {
        if (!playerInitialized) return;
        if (!m_Jump) {
            m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
        }
    }


    // Fixed update is called in sync with physics
    private void FixedUpdate() {
        if (!playerInitialized) return;
        // read inputs
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        m_Crouch = Input.GetKey(KeyCode.C);

        // calculate move direction to pass to character
        if (m_Cam != null) {
            // calculate camera relative direction to move:
            m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            m_Move = v * m_CamForward + h * m_Cam.right;
        } else {
            // we use world-relative directions in the case of no main camera
            m_Move = v * Vector3.forward + h * Vector3.right;
        }
#if !MOBILE_INPUT
        // walk speed multiplier
        if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif

        // pass all parameters to the character control script
        sendPlayerMoveCommand();
        m_Jump = false;
    }

    private void sendPlayerMoveCommand() {
        Command command = new Command() {
            playerId = localPlayer.getId(),
            commandType = GameManager.COMMANDTYPE.MOVE,
            positionX = localPlayer.transform.position.x,
            positionY = localPlayer.transform.position.y,
            positionZ = localPlayer.transform.position.z,
            time = Time.realtimeSinceStartup,
            moveForward = m_Move.z,
            moveSideways = m_Move.x,
            jump = m_Jump,
            crouch = m_Crouch
        };
        gameManager.SendCommand(command);
    }
}
