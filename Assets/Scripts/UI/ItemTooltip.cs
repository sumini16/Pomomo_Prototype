using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTooltip : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void Show(ItemData item)
    {
        nameText.text = item.displayName;
        descriptionText.text = item.description;

        bool hasIcon = item.icon != null;
        iconImage.gameObject.SetActive(hasIcon);

        if (hasIcon)
            iconImage.sprite = item.icon;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}