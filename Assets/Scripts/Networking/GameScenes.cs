using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;

public class GameScenes : NetworkBehaviour
{
    [SerializeField] private Transform playerCharacter;

    void Start(){
        SceneManager.sceneLoaded += SceneLoadedLocal;

        // TESTING MODE
        Scene scene = SceneManager.GetActiveScene();
        Transform mapData = GetMapData(scene);
        if (mapData){
            Debug.Log("Debug mode: Started game in gameplay scene.");

            // Gameplay map
            if (SteamManager.currentLobby == null){
                // Solo
                StartMapSolo(mapData);
            }
        }
    }
    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneLoadedMultiplayer;
    }

    private Transform SpawnCharacter(Transform mapData){
        Transform character = Instantiate(playerCharacter);
        character.position = mapData.Find("Spawn").position;
        return character;
    }
    public static Transform GetMapData(Scene scene){
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (GameObject root in roots){
            if (root.name == "MapData"){
                return root.transform;
            }
        }
        return null;
    }
    public static Transform GetMapData(){
        Scene scene = SceneManager.GetActiveScene();
        return GetMapData(scene);
    }

    private void SceneLoadedMultiplayer(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Scene scene = SceneManager.GetActiveScene();
        Transform mapData = GetMapData(scene);

        // Multiplayer
        if (IsHost && mapData){
            // Spawn character for each player
            foreach (ulong id in clientsCompleted){
                Transform newChar = SpawnCharacter(mapData);
                NetworkObject netObj = newChar.GetComponent<NetworkObject>();
                netObj.SpawnAsPlayerObject(id, true);
            }
        }
    }

    private void StartMapSolo(Transform mapData){
        // Solo
        Debug.Log("Map started in solo mode.");
        SpawnCharacter(mapData);
    }

    private void SceneLoadedLocal(Scene scene, LoadSceneMode loadSceneMode){
        Transform mapData = GetMapData(scene);
        if (mapData){
            // Gameplay map
            if (SteamManager.currentLobby == null){
                // Solo
                StartMapSolo(mapData);
            } else {
                // Multiplayer
            }
        }
    }
}
