using UnityEngine;
using System.Collections;

public class PlayerFactory : MonoBehaviour {
    UnityStandardAssets.Utility.SmoothFollow smoothFollowScript;
    public GameObject localPlayerPrefab;
    public GameObject remotePlayerPrefab;
    public Color[] playerColors;

    private int colorIndex = -1;

    void Awake() {
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        smoothFollowScript = mainCamera.GetComponent<UnityStandardAssets.Utility.SmoothFollow>();
    }
	
    public Player SpawnLocalPlayer(int playerId, float posX, float posZ) {
        GameObject localPlayer = (GameObject)Instantiate(localPlayerPrefab, new Vector3(posX, 0, posZ), Quaternion.identity);
        Transform[] children = localPlayer.GetComponentsInChildren<Transform>();
        foreach (Transform t in children) {
            if (t.name == "CameraTarget") {
                smoothFollowScript.setCameraTarget(t);
                break;
            }
        }
        SkinnedMeshRenderer[] renderers = localPlayer.GetComponentsInChildren<SkinnedMeshRenderer>();
        Color color = PlayerColor(playerId);
        foreach (SkinnedMeshRenderer renderer in renderers) {
            renderer.material.SetColor("_Color", color);
        }
        return  localPlayer.GetComponent<Player>();
    }

    public Player SpawnRemotePlayer(int playerId, float posX, float posZ) {
        GameObject remotePlayer = (GameObject)Instantiate(remotePlayerPrefab, new Vector3(posX, 0, posZ), Quaternion.identity);
        SkinnedMeshRenderer[] renderers = remotePlayer.GetComponentsInChildren<SkinnedMeshRenderer>();
        Color color = PlayerColor(playerId);
        foreach (SkinnedMeshRenderer renderer in renderers) {
            renderer.material.SetColor("_Color",color);
        }
        return remotePlayer.GetComponent<Player>();
    }

    Color PlayerColor(int playerId) {
        int color = playerId % playerColors.Length;
        return playerColors[color];
    }
}
