using System;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ChatManager : MonoBehaviour
{
    public CanvasGroup Gui_parent;
    public TMP_InputField Gui_input;
    public Transform Gui_list;
    public GameObject Gui_label;

    private bool opened = false;
    private float lastMessageTime = -10f;
    private float fadeOutDuration = 1f;


    #region Utilities

    // Server message: someone left the lobby
    private void LobbyMemberLeave(Lobby lobby, Friend friend){
        CreateMessage(friend.Name + " left the lobby.");
    }

    // Server message: someone joined the lobby
    private void LobbyMemberJoined(Lobby lobby, Friend friend){
        CreateMessage(friend.Name + " joined the lobby!");
    }

    // Server message: you joined the lobby (you the player)
    private void LobbyEntered(Lobby lobby){
        CreateMessage("You joined the lobby");
    }

    // [Someone]: Sent a message
    private void ChatSent(Lobby lobby, Friend friend, string message)
    {
        CreateMessage("["+friend.Name+"]: "+message);
    }

    /// UI: Create message with a given text
    private void CreateMessage(string text){
        GameObject obj = Instantiate(Gui_label, Gui_list);
        TMP_Text label = obj.GetComponent<TMP_Text>();
        label.text = text;
        obj.SetActive(true);

        lastMessageTime = Time.time + 4f;
    }

    // Try to send chat message
    private void SendChatMessage(string text){
        // Text is not empty?
        if (!string.IsNullOrEmpty(text)){
            // Send message to lobby
            SteamManager.currentLobby?.SendChatString(text);
        }
    }

    public void SetChatVisible(bool visible){
        opened = visible;
        if (opened){
            // Show chat

            // Clear text input, select input
            Gui_input.text = "";
            EventSystem.current.SetSelectedGameObject(Gui_input.gameObject);
        } else {
            // Hide chat

            // Clear text input, deselect input
            string message = Gui_input.text;
            Gui_input.text = "";
            EventSystem.current.SetSelectedGameObject(null);

            // Try to send message
            SendChatMessage(message);
        }
    }


    #endregion
    #region Events


    void Start(){
        // Keep chat in all scenes
        DontDestroyOnLoad(gameObject);

        // Call function when chat key is pressed
        InputExt.actions["Chat"].started += ChatKeyPressed;
    }

    void ChatKeyPressed(InputAction.CallbackContext context){
        // Toggle chat when key is pressed
        SetChatVisible(!opened);
    }

    void OnEnable(){
        SetChatVisible(opened);
        
        // Connect events
        SteamMatchmaking.OnChatMessage       += ChatSent;
        SteamMatchmaking.OnLobbyEntered      += LobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += LobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave  += LobbyMemberLeave;
    }
    void OnDisable(){
        // Disconnect events
        SteamMatchmaking.OnChatMessage       -= ChatSent;
        SteamMatchmaking.OnLobbyEntered      -= LobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= LobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave  -= LobbyMemberLeave;
    }

    // Update every frame
    void Update(){
        // Set UI opacity (fade out when inactive)
        if (opened){
            lastMessageTime = Time.time + fadeOutDuration;
        }
        Gui_parent.alpha = Mathf.Clamp01((lastMessageTime - Time.time) / fadeOutDuration);
    }

    #endregion
}
