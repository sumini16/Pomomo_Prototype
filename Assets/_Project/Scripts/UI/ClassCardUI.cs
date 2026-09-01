using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 직업 카드 하나의 표시를 담당합니다.
/// 무엇을 할지는 Bind로 주입받으므로, 카드는 선택 로직을 알지 못합니다(ItemSlotUI와 같은 방식).
/// </summary>
public class ClassCardUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selectedFrame;

    private Action<ClassData> onClick;

    public ClassData Data { get; private set; }

    public void Bind(ClassData data, Action<ClassData> clickAction)
    {
        Data = data;
        onClick = clickAction;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = data.icon != null;   // 아이콘이 없으면 흰 사각형이 남지 않도록
        }

        if (nameText != null) nameText.text = data.displayName;
        if (descriptionText != null) descriptionText.text = data.description;

        // 수치는 ClassData에서 직접 읽습니다.
        // 손으로 적어두면 밸런스를 바꿨을 때 설명만 옛 값으로 남습니다.
        if (statsText != null)
            statsText.text =
                $"체력\t{data.maxHealth}\n" +
                $"공격력\t{data.attackDamage}\n" +
                $"이동속도\t{data.moveSpeed:0.0}\n" +
                $"방어\t{data.defense}";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(Data));
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedFrame != null) selectedFrame.SetActive(selected);
    }
}