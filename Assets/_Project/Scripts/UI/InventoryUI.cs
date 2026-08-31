using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 소지품 창.
/// - 평소에는 I 키로 여닫습니다.
/// - 상점이 열리면 우측으로 밀려나 '판매 창'이 됩니다.
/// - 상단 분류 탭으로 보여줄 아이템을 걸러냅니다.
///
/// 슬롯을 그리는 코드는 하나뿐이고, 눌렀을 때의 동작만 상점이 주입합니다.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    /// <summary>분류 탭 하나. 인스펙터에서 버튼과 분류를 짝지어 등록합니다.</summary>
    [Serializable]
    public class CategoryTab
    {
        public Button button;

        [Tooltip("체크하면 '전체' 탭이 됩니다. 아래 Category 값은 무시됩니다.")]
        public bool isAllTab;

        public ItemCategory category;

        [Tooltip("선택했을 때 좌측 표시기에 나타날 이름")]
        public string label = "전체";
    }

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private ItemSlotUI slotPrefab;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private TextMeshProUGUI emptyText;
    [SerializeField] private ItemTooltip tooltip;

    [Header("제목")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private string normalTitle = "소지품";
    [SerializeField] private string tradeTitle = "소지품  (클릭하면 판매)";

    [Header("분류 탭")]
    [SerializeField] private CategoryTab[] tabs;

    [Tooltip("지금 어떤 분류를 보고 있는지 좌측에 표시합니다. 누르는 버튼이 아닙니다.")]
    [SerializeField] private TextMeshProUGUI selectedLabelText;
    [SerializeField] private Color selectedTabColor = Color.white;
    [SerializeField] private Color normalTabColor = new Color(1f, 1f, 1f, 0.35f);

    [Header("격자")]
    [Tooltip("항상 깔아둘 칸 수. 아이템이 없는 칸은 빈 칸으로 남습니다.")]
    [SerializeField] private int slotCount = 42;

    [Header("배치")]
    [Tooltip("평소 위치")]
    [SerializeField] private Vector2 normalPosition = Vector2.zero;
    [Tooltip("상점이 열렸을 때 우측으로 밀려나는 위치")]
    [SerializeField] private Vector2 dockedPosition = new Vector2(310f, 0f);

    public bool IsOpen { get; private set; }

    // 거래 모드일 때만 설정됩니다. null이면 평소 인벤토리입니다.
    private Action<ItemData> slotClickAction;
    private Func<ItemData, int> priceProvider;

    private bool InTradeMode => slotClickAction != null;

    // 분류 필터. showAll이 true면 category는 무시합니다.
    private bool showAll = true;
    private ItemCategory currentCategory;

    // 매번 만들고 지우지 않고 한 번 만들어 재사용합니다.
    // 갱신마다 Destroy/Instantiate를 반복하면 쓰레기가 계속 쌓입니다.
    private readonly List<ItemSlotUI> slots = new List<ItemSlotUI>();

    private void Awake()
    {
        if (panelRect == null) panelRect = panelRoot.GetComponent<RectTransform>();
        panelRoot.SetActive(false);

        BuildSlots();

        // 각 탭 버튼이 자기 분류를 들고 있게 합니다.
        // foreach 변수는 반복마다 새로 만들어지므로 클로저에 안전하게 담깁니다.
        foreach (CategoryTab tab in tabs)
        {
            if (tab.button == null) continue;
            tab.button.onClick.AddListener(() => SelectTab(tab));
        }

        // 시작 상태는 '전체' 탭. 없으면 첫 번째 탭을 씁니다.
        CategoryTab initial = System.Array.Find(tabs, tab => tab.isAllTab);
        if (initial == null && tabs.Length > 0) initial = tabs[0];
        if (initial != null) SelectTab(initial, refresh: false);
        else ApplyTabColors();
    }

    private void OnEnable() => inventory.OnInventoryChanged += RefreshIfOpen;
    private void OnDisable() => inventory.OnInventoryChanged -= RefreshIfOpen;

    private void Update()
    {
        if (Keyboard.current == null) return;

        // 상점이 열려 있는 동안에는 I 키로 닫지 못하게 합니다.
        // 판매 창이 사라지면 상점만 덩그러니 남기 때문입니다.
        if (InTradeMode) return;

        if (Keyboard.current.iKey.wasPressedThisFrame)
            Toggle();
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        IsOpen = true;
        panelRoot.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        IsOpen = false;
        panelRoot.SetActive(false);
        tooltip.Hide();
    }

    /// <summary>상점이 열릴 때 호출합니다. 슬롯을 누르면 넘겨준 동작이 실행됩니다.</summary>
    public void EnterTradeMode(Action<ItemData> onSlotClicked, Func<ItemData, int> priceOf)
    {
        slotClickAction = onSlotClicked;
        priceProvider = priceOf;

        panelRect.anchoredPosition = dockedPosition;
        if (titleText != null) titleText.text = tradeTitle;

        Open();
    }

    /// <summary>상점이 닫힐 때 호출합니다. 평소 인벤토리로 되돌립니다.</summary>
    public void ExitTradeMode()
    {
        slotClickAction = null;
        priceProvider = null;

        panelRect.anchoredPosition = normalPosition;
        if (titleText != null) titleText.text = normalTitle;

        Close();
    }

    private void SelectTab(CategoryTab tab, bool refresh = true)
    {
        showAll = tab.isAllTab;
        currentCategory = tab.category;

        // 좌측 표시기에 방금 고른 분류를 띄웁니다.
        if (selectedLabelText != null)
            selectedLabelText.text = tab.label;

        ApplyTabColors();

        if (!refresh) return;

        tooltip.Hide();      // 탭을 바꾸면 슬롯이 사라지므로 툴팁도 같이 내립니다
        Refresh();
    }

    private void ApplyTabColors()
    {
        foreach (CategoryTab tab in tabs)
        {
            if (tab.button == null) continue;

            bool selected = tab.isAllTab ? showAll
                                         : (!showAll && tab.category == currentCategory);

            Image image = tab.button.GetComponent<Image>();
            if (image != null)
                image.color = selected ? selectedTabColor : normalTabColor;
        }
    }

    private void RefreshIfOpen()
    {
        if (IsOpen) Refresh();
    }

    /// <summary>빈 격자를 미리 깔아둡니다. 실행 중에 칸 수는 변하지 않습니다.</summary>
    private void BuildSlots()
    {
        // 에디터에서 미리 넣어둔 자식이 있으면 정리합니다.
        for (int i = slotContainer.childCount - 1; i >= 0; i--)
            Destroy(slotContainer.GetChild(i).gameObject);

        slots.Clear();

        for (int i = 0; i < slotCount; i++)
        {
            ItemSlotUI slot = Instantiate(slotPrefab, slotContainer);
            slot.Clear();
            slots.Add(slot);
        }
    }

    private void Refresh()
    {
        int index = 0;

        foreach (var pair in inventory.Items)
        {
            // 분류 필터. '전체' 탭이면 전부 통과시킵니다.
            if (!showAll && pair.Key.category != currentCategory) continue;

            if (index >= slots.Count) break;   // 칸이 모자라면 나머지는 그리지 않습니다

            int price = priceProvider != null ? priceProvider(pair.Key) : 0;
            slots[index].Bind(pair.Key, pair.Value, tooltip, slotClickAction, price);

            index++;
        }

        // 남은 칸은 빈 칸으로 되돌립니다. 격자는 그대로 유지됩니다.
        for (int i = index; i < slots.Count; i++)
            slots[i].Clear();

        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(index == 0);
            emptyText.text = showAll ? "비어 있습니다" : "이 분류에 아이템이 없습니다";
        }
    }
}