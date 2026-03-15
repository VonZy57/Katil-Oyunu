using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogOptionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI textComponent;
    private Color normalColor = new Color(0xFD / 255f, 0xFF / 255f, 0x00 / 255f); // #FDFF00
    private Color hoverColor = new Color(0xB8 / 255f, 0xB8 / 255f, 0x14 / 255f); // #B8B814

    void Awake()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.color = normalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (textComponent != null)
        {
            textComponent.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (textComponent != null)
        {
            textComponent.color = normalColor;
        }
    }
}
