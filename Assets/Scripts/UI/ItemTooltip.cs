using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemTooltip : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Vector2 cursorOffset = new Vector2(16f, -16f);

    private RectTransform rect;
    private RectTransform canvasRect;
    private Canvas canvas;

    private void Awake()
    {
        rect = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
        canvasRect = (RectTransform)canvas.transform;

        gameObject.SetActive(false);
    }

    public void Show(ItemData item)
    {
        if (item == null) return;

        nameText.text = item.displayName;
        descriptionText.text = item.description;

        bool hasIcon = item.icon != null;
        iconImage.gameObject.SetActive(hasIcon);
        if (hasIcon)
            iconImage.sprite = item.icon;

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        FollowCursor();
    }

    public void Hide() => gameObject.SetActive(false);

    private void Update() => FollowCursor();

    private void FollowCursor()
    {
        if (Mouse.current == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();

        // Overlay 캔버스에서는 worldCamera가 null이며, 그대로 넘겨야 정상 동작합니다
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPos, canvas.worldCamera, out Vector2 local);

        rect.anchoredPosition = local + cursorOffset;
    }
}