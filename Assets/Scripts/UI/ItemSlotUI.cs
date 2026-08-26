using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;

    public void Bind(ItemData item, int count)
    {
        nameText.text = item.displayName;
        countText.text = count.ToString();

        // 아이콘이 없는 아이템도 있을 수 있으므로 표시 여부를 함께 처리
        bool hasIcon = item.icon != null;
        iconImage.gameObject.SetActive(hasIcon);
        if (hasIcon)
            iconImage.sprite = item.icon;
    }
}