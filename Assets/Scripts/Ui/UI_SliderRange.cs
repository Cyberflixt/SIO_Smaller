using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SliderRange : MonoBehaviour
{
    [SerializeField] private Slider slider;
    private TextMeshProUGUI valueText;
    public delegate void OptionChangedDelegate();
    public OptionChangedDelegate OnOptionChanged;

    void Awake()
    {
        valueText = gameObject.GetComponentInChildren<TextMeshProUGUI>();
    }
    void Start()
    {
        UpdateText(slider.value);
        slider.onValueChanged.AddListener(UpdateText);
    }

    void UpdateText(float value)
    {
        OnOptionChanged?.Invoke();
        valueText.text = value.ToString("0.0"); 
    }

    public float GetValue() {
        return slider.value;
    }
    public void SetValue(float v) {
        slider.value = v;
    }

}
