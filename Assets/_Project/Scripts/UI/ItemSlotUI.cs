using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 아이템 한 칸. 무엇을 표시할지만 알고, 눌렸을 때 무엇을 할지는 알지 못합니다.
/// 클릭 동작을 밖에서 주입받으므로 인벤토리·구매·판매에 같은 프리팹을 씁니다.
/// (DialogueUI가 수락/거절 동작을 넘겨받는 것과 같은 방식)
/// </summary>
public class ItemSlotUI : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;

    [Tooltip("가격 표시용. 값이 0이면 자동으로 숨겨집니다.")]
    [SerializeField] private TextMeshProUGUI priceText;

    [Tooltip("아이템 이름. 목록형(상점) 프리팹에서만 씁니다. 격자형은 비워두세요.")]
    [SerializeField] private TextMeshProUGUI nameText;

    private ItemData item;
    private ItemTooltip tooltip;
    private Action<ItemData> onClick;

    public void Bind(ItemData data, int count, ItemTooltip sharedTooltip,
                     Action<ItemData> clickAction = null, int price = 0)
    {
        item = data;
        tooltip = sharedTooltip;
        onClick = clickAction;

        bool hasIcon = data.icon != null;
        iconImage.gameObject.SetActive(hasIcon);
        if (hasIcon) iconImage.sprite = data.icon;

        // 1개일 때 숫자를 숨기는 것이 RPG 관례
        countText.text = count > 1 ? count.ToString() : string.Empty;

        if (nameText != null) nameText.text = data.displayName;

        if (priceText != null)
        {
            bool showPrice = price > 0;
            priceText.gameObject.SetActive(showPrice);
            if (showPrice) priceText.text = price.ToString("N0");
        }
    }

    /// <summary>빈 칸으로 만듭니다. 격자를 유지하기 위해 오브젝트는 남겨둡니다.</summary>
    public void Clear()
    {
        item = null;
        onClick = null;

        iconImage.gameObject.SetActive(false);
        countText.text = string.Empty;

        if (priceText != null) priceText.gameObject.SetActive(false);
        if (nameText != null) nameText.text = string.Empty;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null) return;      // 빈 칸에는 툴팁을 띄우지 않습니다
        tooltip?.Show(item);
    }

    public void OnPointerExit(PointerEventData eventData) => tooltip?.Hide();

    // 빈 칸은 onClick이 null이라 눌러도 아무 일도 일어나지 않습니다.
    public void OnPointerClick(PointerEventData eventData) => onClick?.Invoke(item);
}