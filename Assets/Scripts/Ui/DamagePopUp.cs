using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopUp : MonoBehaviour
{
    public AnimationCurve opacityCurve;
    public AnimationCurve scaleCurve;
    public AnimationCurve yCurve;

    public TextMeshProUGUI label;
    [HideInInspector]
    public float duration = 1;
    [HideInInspector]
    public float scale = 1;
    [HideInInspector]
    public Vector3 origin;

    private Vector3 defaultSize = Vector3.one;
    private float start = 0;

    private void Awake()
    {
        start = Time.time;
        origin = transform.position;
        defaultSize = transform.localScale;
    }

    private void Update()
    {
        // Animate
        float t = (Time.time - start) / duration;

        label.color = new Color(1, 1, 1, opacityCurve.Evaluate(t));
        transform.localScale = defaultSize * scaleCurve.Evaluate(t) * scale;
        if (Camera.main != null){
            transform.forward = Camera.main.transform.forward;
            transform.position = origin + Vector3.up * yCurve.Evaluate(t) - transform.forward * .5f;
        } else {
            transform.position = origin + Vector3.up * yCurve.Evaluate(t);
        }

        // Destroy after done
        if (t > 1)
            Destroy(gameObject);
    }

    public static void Create(Vector3 position, string text, float duration = 1f, float scale = 1f){
        // Create
        Transform popup = Instantiate(Global.instance.damagePopup);
        DamagePopUp script = popup.GetComponent<DamagePopUp>();

        // Set text & position
        popup.transform.position = position;
        script.label.text = text;
        script.duration = duration;
        script.origin = position;
        script.scale = scale;
    }
}
