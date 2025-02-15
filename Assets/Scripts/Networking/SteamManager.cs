using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using TMPro;
using Steamworks.Data;
using System;
using UnityEngine.UI;
using Unity.Netcode;
using Netcode.Transports.Facepunch;
using UnityEngine.SceneManagement;

public class SteamManager : MonoBehaviour
{

    private const int playersMax = 4;
    public static Lobby? currentLobby;

    void OnEnable(){
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }
    void OnDisable(){
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result == Result.OK){
            lobby.SetPublic();
            lobby.SetJoinable(true);
            NetworkManager.Singleton.StartHost();
        }
    }

    private void OnLobbyEntered(Lobby lobby){
        currentLobby = lobby;
        MainMenuUI.OnLobbyJoined(lobby.Id.ToString());

        if (NetworkManager.Singleton.IsHost) return;
        NetworkManager.Singleton.gameObject.GetComponent<FacepunchTransport>().targetSteamId = lobby.Owner.Id;
        NetworkManager.Singleton.StartClient();
    }

    // Player is trying to join someone (using steam)
    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId id){
        await lobby.Join();
    }

    public void CopyLobbyId(){
        // Get & Copy lobby id
        if (currentLobby != null){
            string id = currentLobby.Value.Id.ToString();

            TextEditor textEditor = new TextEditor();
            textEditor.text = id;
            textEditor.SelectAll();
            textEditor.Copy();
        }
    }

    public void LeaveLobby(){
        currentLobby?.Leave();
        currentLobby = null;
        NetworkManager.Singleton.Shutdown();
        
        MainMenuUI.OnLobbyLeft();
    }

    public async void CreateLobby(){
        await SteamMatchmaking.CreateLobbyAsync(playersMax);
    }

    public static async void JoinLobbyID(ulong lobbyID){
        Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithSlotsAvailable(1).RequestAsync();

        foreach(Lobby lobby in lobbies){
            if (lobby.Id == lobbyID){
                await lobby.Join();
                return;
            }
        }
    }

    public void StartGameServer(){
        // Load scene for everyone
        if (NetworkManager.Singleton.IsHost){
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
        }
    }
}
