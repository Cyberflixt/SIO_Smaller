using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks.Data;

public class MainMenuUI : MonoBehaviour
{
    public TMP_InputField lobbyIDInputField;
    public TMP_Text labelLobbyID;
    public GameObject UI_Lobby;
    public GameObject UI_MainMenu;

    public static MainMenuUI instance = null;

    private void Start(){
        instance = this;

        // Is player in a lobby?
        if (SteamManager.currentLobby != null){
            OnLobbyJoined(SteamManager.currentLobby.Value.Id.ToString());
        } else {
            OnLobbyLeft();
        }
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game");
        Application.Quit();
    }

    public void JoinLobbyTextboxID(){
        // Is text a number?
        if (ulong.TryParse(lobbyIDInputField.text, out ulong lobbyID))
            SteamManager.JoinLobbyID(lobbyID);
    }
    
    public static void OnLobbyJoined(string id){
        if (instance){
            // Enable lobby UI
            instance.labelLobbyID.text = id;

            instance.UI_Lobby.SetActive(true);
            instance.UI_MainMenu.SetActive(false);
        }
    }
    public static void OnLobbyLeft(){
        if (instance){
            // Disable lobby UI
            instance.UI_Lobby.SetActive(false);
            instance.UI_MainMenu.SetActive(true);
        }
    }
}
