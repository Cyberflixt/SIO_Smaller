using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Interact : MonoBehaviour
{
    // References
    public RectTransform holder;
    public TMP_Text label_key;
    public TMP_Text label_value;

    // Private
    private static UI_Interact instance;
    private static string current_text = "whopper";
    private int animate_token = 0;

    void Awake(){
        instance = this;
        Hide();
    }

    public static void Hide(){
        if (current_text != ""){
            instance.holder.gameObject.SetActive(false);
            current_text = "";
        }
    }

    public static void Show(string key, string value){
        if (current_text != value){
            current_text = value;
            instance.holder.gameObject.SetActive(true);
            instance.label_key.text = key;
            instance.label_value.text = value;

            instance.StartCoroutine(instance.LoopAnimate());
        }
    }

    private IEnumerator LoopAnimate()
    {
        animate_token++;
        int token = animate_token;

        float offset = .5f;
        float scale = .1f;

        // Loop until time passed (or token changed)
        float elapsedTime = 0f;
        while (elapsedTime < 1 && animate_token == token)
        {
            elapsedTime += Time.deltaTime;

            // Get smooth time
            float time = elapsedTime;
            float t = 1-time;
            t = t*t*t;

            holder.localScale = Vector3.one + Vector3.one * scale * t;
            holder.anchorMin = new Vector2(0, 0 - t * offset);
            holder.anchorMax = new Vector2(1, 1 - t * offset);

            yield return null;
        }

        // Animation wasn't cancelled?
        if (animate_token == token){
            // Final properties
            holder.localScale = Vector3.one;
            holder.anchorMin = new Vector2(0,0);
            holder.anchorMax = new Vector2(1,1);
        }
    }
}
