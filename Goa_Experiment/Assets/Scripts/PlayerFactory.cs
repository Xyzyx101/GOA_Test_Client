using UnityEngine;
using System.Collections;

public class PlayerFactory : MonoBehaviour {
    UnityStandardAssets.Utility.SmoothFollow smoothFollowScript;
    public GameObject localPlayerPrefab;
    public GameObject remotePlayerPrefab;
    public Color[] remotePlayerColors;

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
        Player foo = localPlayer.GetComponent<Player>();
        return foo;
    }

    public Player SpawnRemotePlayer(int playerId, float posX, float posZ) {
        GameObject remotePlayer = (GameObject)Instantiate(remotePlayerPrefab, new Vector3(posX, 0, posZ), Quaternion.identity);
        SkinnedMeshRenderer[] renderers = remotePlayer.GetComponentsInChildren<SkinnedMeshRenderer>();
        Color color = NextColor();
        foreach (SkinnedMeshRenderer renderer in renderers) {
            renderer.material.SetColor("_Color",color);
        }
        return remotePlayer.GetComponent<Player>();
    }

    Color NextColor() {
        int color = ++colorIndex % remotePlayerColors.Length;
        return remotePlayerColors[color];
    }
}
