using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;

    private ItemData item;
    private ItemTooltip tooltip;

    public void Bind(ItemData data, int count, ItemTooltip sharedTooltip)
    {
        item = data;
        tooltip = sharedTooltip;

        bool hasIcon = data.icon != null;
        iconImage.gameObject.SetActive(hasIcon);
        if (hasIcon)
            iconImage.sprite = data.icon;

        // 1개일 때는 숫자를 숨기는 것이 RPG 관례
        countText.text = count > 1 ? count.ToString() : string.Empty;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"슬롯 진입  tooltip={(tooltip == null ? "null" : "OK")}");
        tooltip?.Show(item);
    }
    public void OnPointerExit(PointerEventData eventData) => tooltip?.Hide();
}