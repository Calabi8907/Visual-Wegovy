using UnityEngine;
using TMPro; // Use UnityEngine.UI if using standard UI Text
using UnityEngine.UI;

public class SliderTextUpdater : MonoBehaviour
{
    public TextMeshProUGUI textElement;
    public Slider slider;

    // This method must be public and take a float to show as a "Dynamic" option
    public void UpdateText()
    {
        textElement.text = slider.value.ToString();
    }
}

