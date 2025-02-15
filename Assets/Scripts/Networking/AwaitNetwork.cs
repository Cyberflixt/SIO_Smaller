using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// Waits for NetworkManager to load,
/// then switches to Main Menu's scene
/// </summary>
public class AwaitNetwork : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadMainScene());
    }

    IEnumerator LoadMainScene(){
        // Waits for NetworkManager to load
        yield return new WaitUntil(() => NetworkManager.Singleton != null);

        // Load main menu
        SceneManager.LoadScene("MainMenu");
    }
}
