using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Slider : MonoBehaviour
{
    public delegate void OptionChangedDelegate();
    public OptionChangedDelegate OnOptionChanged;

    [SerializeField] private TextMeshProUGUI optionText; 
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton; 

    [SerializeField] private string[] options;

    private int currentIndex = 0;
    void Start()
    {
        UpdateOptionText();
        leftButton.onClick.AddListener(PreviousOption);
        rightButton.onClick.AddListener(NextOption);

    }

    private void UpdateOptionText()
    {
        optionText.text = options[currentIndex];
    }

    private void PreviousOption()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = options.Length - 1;
        }
        UpdateOptionText();
        OnOptionChanged?.Invoke();
    }

    private void NextOption()
    {
        currentIndex++;
        if (currentIndex >= options.Length)
        {
            currentIndex = 0;
        }
        UpdateOptionText();
        OnOptionChanged?.Invoke();
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }

    public void SetCurrentIndex(int index)
    {
        currentIndex = index;
        UpdateOptionText();
    }
}
