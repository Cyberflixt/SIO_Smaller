using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UI_Combo : MonoBehaviour
{
    public static UI_Combo instance;

    [Category("Side bar")]
    public TMP_Text label_rank;
    public TMP_Text label_grade;
    public TMP_Text label_score;
    public RectTransform side_bar_parent;
    public RectTransform side_bar_bar;
    public Transform[] side_bar_popups;



    private static int popup_last_index = 0;
    private static float side_bar_offset = 0;
    private static int combo_score_total = 0;
    private static float combo_score_rank = 0;

    private struct Rank{
        public string letter;
        public string name;

        public float score;
        public float multiplier;
        public float decrease_speed;

        public Rank(string _letter, string _name, float _score, float _multiplier, float _decrease_speed){
            letter = _letter;
            name = _name;
            score = _score;
            multiplier = _multiplier;
            decrease_speed = _decrease_speed;
        }
    }
    private static Rank[] ranks = {
        //    Letter, Name, Score, Multiplier, Decrease_speed
        new Rank("S", "Silence",      0, 2,  1),
        new Rank("D", "Dreadful",     1, 1,  .5f),
        new Rank("C", "Cemetery",   400, 1,  1.2f),
        new Rank("B", "Bloodshed",  800, 2,  1.5f),
        new Rank("A", "Armageddon",1300, 3,  2f),
        new Rank("S", "SAVAGERY", 1900, 5,  2.5f),
        new Rank("Ω", "OMEGA",     3000, 10, 3f),
        new Rank("?", "HOLY FUCK", 4000, 30, 5f),
    };

    private static (Rank current, Rank next) GetCurrentRanks(){
        // Get rank corresponding to score;
        int i = 1;
        while (i < ranks.Length && combo_score_rank >= ranks[i].score){
            i++;
        }
        return (ranks[i-1], ranks[Math.Min(i, ranks.Length)]);
    }
    
    private static void AddScore(int score){
        combo_score_total += score;
        combo_score_rank += score;
        side_bar_offset = 1;
    }

    public static void AddCombo(string text, int score){
        AddScore(score);

        // Clone different popup
        popup_last_index = (popup_last_index + 1) % instance.side_bar_popups.Length;
        Transform original = instance.side_bar_popups[popup_last_index];
        Transform popup = Instantiate(original, original.parent);
        popup.gameObject.SetActive(true);

        // Get text label
        TMP_Text label = popup.GetChild(0).GetComponent<TMP_Text>();
        label.text = text;

        instance.StartCoroutine(instance.PopupLoop(popup));
    }

    private float EaseOutEaseInOut(float x){
        float peak = 0.05f;

        // 1st up ease out
        if (x < peak)
            return x/peak * (2 - x/peak);

        // 2nd down ease in
        float t = (x - peak)/(1 - peak);
        if (x < (peak+1)/2)
            return 1 - 4*t*t*t;
        
        // 3rd down ease out
        float v = -2 * t + 2;
        return v*v*v/2;
    }
    private float EaseOutStrong(float x){
        x = 1 - x;
        float b = x*x*x*x;
        return 1 - b*b*b;
    }

    private IEnumerator PopupLoop(Transform popup)
    {
        float popup_duration = 3;

        // Save start position
        RectTransform rect = popup.GetComponent<RectTransform>();

        int rotation_base = UnityEngine.Random.Range(-50, 50);

        // Loop until time passed
        float elapsedTime = 0f;
        while (elapsedTime < popup_duration)
        {
            elapsedTime += Time.deltaTime;

            // Get smooth time
            float time = elapsedTime / popup_duration;
            float smooth = EaseOutStrong(time);
            
            rect.localScale = Vector3.Lerp(Vector3.one * 2, Vector3.one, smooth);

            float rotation = (1-smooth) * rotation_base;
            rect.localEulerAngles = new Vector3(0, 0, rotation);

            yield return null;
        }

        // Destroy the item
        Destroy(popup.gameObject);
    }



    private Vector2 default_anchorMin = Vector2.zero;
    private Vector2 default_anchorMax = Vector2.zero;

    // Events
    void Awake(){
        // Singleton
        instance = this;

        // Hide originals
        foreach (Transform popup in side_bar_popups){
            popup.gameObject.SetActive(false);
        }

        // Save defaults
        default_anchorMin = side_bar_parent.anchorMin;
        default_anchorMax = side_bar_parent.anchorMax;
    }

    private void Update(){
        (Rank rank, Rank rank_next) = GetCurrentRanks();
        combo_score_rank -= Time.deltaTime * rank.decrease_speed * 100;
        if (combo_score_rank < 0) combo_score_rank = 0;

        // Combo bar
        float t = 1;
        if (rank.score > 0){
            t = 1 - (rank_next.score - combo_score_rank) / (rank_next.score - rank.score);
        }
        side_bar_bar.anchorMax = new Vector2(1, Mathf.Clamp01(t));

        // Texts
        instance.label_score.text = '+' + combo_score_total.ToString();
        instance.label_grade.text = rank.letter;
        instance.label_rank.text = rank.name;

        // Noise
        float noise_force = .01f;
        noise_force *= Mathf.Log(combo_score_rank / 200 + 1) / 2;
        
        float noiseV = 100 * Time.time;
        float noisex = (Mathf.PerlinNoise1D(noiseV) * 2 - 1) * noise_force;
        float noisey = (Mathf.PerlinNoise1D(noiseV * 2) * 2 - 1) * noise_force;

        // Offset
        float force = -.05f;
        Vector2 offset = new Vector2(side_bar_offset * force + noisex, noisey);
        side_bar_parent.anchorMin = default_anchorMin + offset;
        side_bar_parent.anchorMax = default_anchorMax + offset;
        side_bar_offset /= 1 + Math.Clamp(Time.unscaledDeltaTime * 5, 0, 1);
    }
}
