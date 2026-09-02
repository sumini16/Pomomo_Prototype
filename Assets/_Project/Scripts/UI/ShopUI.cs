using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 상점 창. 구매 / 판매 두 탭을 목록으로 보여주고, 담은 뒤 한 번에 확정합니다.
///
/// 목록에서 누르면 곧바로 거래되지 않고 '장바구니'에 담깁니다.
/// 확정 버튼을 누르기 전까지 지갑과 인벤토리는 건드리지 않으므로,
/// 합계와 예상 잔액을 미리 보여줄 수 있고 되돌리기도 쉽습니다.
///
/// 거래 판정은 Shop이 하고, 이 클래스는 결과를 문구로 바꿔 보여주기만 합니다.
/// </summary>
public class ShopUI : MonoBehaviour
{
    private enum Tab { Buy, Sell }

    [SerializeField] private GameObject panelRoot;

    [Header("목록")]
    [SerializeField] private Transform rowContainer;
    [Tooltip("목록형 행 프리팹 (아이콘 + 이름 + 가격)")]
    [SerializeField] private ItemSlotUI rowPrefab;
    [SerializeField] private TextMeshProUGUI emptyText;

    [Header("장바구니")]
    [SerializeField] private Transform cartContainer;
    [Tooltip("장바구니 칸 프리팹. 인벤토리 격자용과 같은 것을 씁니다.")]
    [SerializeField] private ItemSlotUI cartSlotPrefab;
    [SerializeField] private int cartSlotCount = 8;

    [Header("탭")]
    [SerializeField] private Button buyTabButton;
    [SerializeField] private Button sellTabButton;
    [SerializeField] private Color selectedTabColor = Color.white;
    [SerializeField] private Color normalTabColor = new Color(1f, 1f, 1f, 0.35f);

    [Header("하단 정산")]
    [SerializeField] private TextMeshProUGUI totalLabelText;
    [SerializeField] private TextMeshProUGUI totalValueText;
    [SerializeField] private TextMeshProUGUI afterLabelText;
    [SerializeField] private TextMeshProUGUI afterValueText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    [SerializeField] private Button clearButton;

    [Header("표시")]
    [SerializeField] private ItemTooltip tooltip;
    [SerializeField] private TextMeshProUGUI shopNameText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Button closeButton;
    [SerializeField] private float feedbackDuration = 2f;

    [Header("연동")]
    [SerializeField] private InventoryUI inventoryUI;

    private static readonly Color SuccessColor = new Color(0.55f, 0.85f, 0.6f);
    private static readonly Color FailColor = new Color(0.95f, 0.5f, 0.5f);
    private static readonly Color NormalValueColor = Color.white;

    private Shop shop;
    private PlayerProgress progress;
    private Tab currentTab = Tab.Buy;
    private float feedbackHideTime;

    /// <summary>아직 확정되지 않은 주문. 확정 전에는 아무것도 바뀌지 않습니다.</summary>
    private readonly Dictionary<ItemData, int> cart = new Dictionary<ItemData, int>();
    private readonly List<ItemSlotUI> cartSlots = new List<ItemSlotUI>();

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        // 연결이 빠지면 열 때 예외가 나는 대신, 시작 시 한 번 알리고 멈춥니다.
        if (panelRoot == null || rowContainer == null || cartContainer == null
            || rowPrefab == null || cartSlotPrefab == null || inventoryUI == null)
        {
            Debug.LogError("[ShopUI] 필수 참조가 비어 있습니다. 인스펙터를 확인하세요.", this);
            enabled = false;
            return;
        }

        panelRoot.SetActive(false);

        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (buyTabButton != null) buyTabButton.onClick.AddListener(() => SelectTab(Tab.Buy));
        if (sellTabButton != null) sellTabButton.onClick.AddListener(() => SelectTab(Tab.Sell));
        if (confirmButton != null) confirmButton.onClick.AddListener(Confirm);
        if (clearButton != null) clearButton.onClick.AddListener(ClearCart);

        if (feedbackText != null) feedbackText.text = string.Empty;

        BuildCartSlots();
    }

    private void OnDisable()
    {
        // 씬 전환 등으로 Close를 거치지 않고 사라지면 UIState가 켜진 채 남아
        // 플레이어 입력이 영영 막힙니다. 여기서 반드시 정리합니다.
        if (IsOpen) Close();
    }

    public void Open(Shop target, PlayerProgress player, string shopName)
    {
        // 이미 열려 있는 상태에서 다시 열면 이벤트가 중복 구독됩니다.
        if (IsOpen) Close();

        shop = target;
        progress = player;

        panelRoot.SetActive(true);
        UIState.SetModal(true);

        progress.Wallet.OnGoldChanged += HandleGoldChanged;
        progress.Inventory.OnInventoryChanged += RefreshAll;

        if (shopNameText != null) shopNameText.text = shopName;

        cart.Clear();
        SelectTab(Tab.Buy);   // 열 때는 항상 구매 탭부터

        inventoryUI.EnterTradeMode(AddToCart, item => item.sellPrice);
    }

    public void Close()
    {
        if (progress != null)
        {
            progress.Wallet.OnGoldChanged -= HandleGoldChanged;
            progress.Inventory.OnInventoryChanged -= RefreshAll;
        }

        cart.Clear();

        if (inventoryUI != null) inventoryUI.ExitTradeMode();
        if (tooltip != null) tooltip.Hide();

        if (panelRoot != null) panelRoot.SetActive(false);
        UIState.SetModal(false);

        shop = null;
        progress = null;
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
            return;
        }

        if (feedbackHideTime > 0f && Time.time > feedbackHideTime)
        {
            if (feedbackText != null) feedbackText.text = string.Empty;
            feedbackHideTime = 0f;
        }
    }

    // ────────────────────────────── 탭

    private void SelectTab(Tab tab)
    {
        currentTab = tab;

        // 구매와 판매를 섞어 담을 수는 없으므로 탭을 바꾸면 비웁니다.
        cart.Clear();

        ApplyTabColors();
        ApplyTabLabels();

        if (tooltip != null) tooltip.Hide();

        RefreshAll();
    }

    private void ApplyTabColors()
    {
        SetTabColor(buyTabButton, currentTab == Tab.Buy);
        SetTabColor(sellTabButton, currentTab == Tab.Sell);
    }

    private void SetTabColor(Button button, bool selected)
    {
        if (button == null) return;

        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = selected ? selectedTabColor : normalTabColor;
    }

    private void ApplyTabLabels()
    {
        bool buying = currentTab == Tab.Buy;

        if (totalLabelText != null) totalLabelText.text = buying ? "구매 금액" : "판매 금액";
        if (afterLabelText != null) afterLabelText.text = buying ? "구매 후 잔액" : "판매 후 잔액";
        if (confirmButtonText != null) confirmButtonText.text = buying ? "구매" : "판매";
    }

    // ────────────────────────────── 장바구니

    private void BuildCartSlots()
    {
        ClearChildren(cartContainer);
        cartSlots.Clear();

        for (int i = 0; i < cartSlotCount; i++)
        {
            ItemSlotUI slot = Instantiate(cartSlotPrefab, cartContainer);
            slot.Clear();
            cartSlots.Add(slot);
        }
    }

    /// <summary>
    /// Destroy는 프레임 끝에 처리되므로, 지운 직후 새로 만들면 그 프레임 동안 두 벌이 공존합니다.
    /// Layout Group이 둘 다 계산해 목록이 순간 늘어나 보이므로 먼저 숨깁니다.
    /// </summary>
    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    /// <summary>목록에서 누르면 여기로 옵니다. 아직 거래는 일어나지 않습니다.</summary>
    private void AddToCart(ItemData item)
    {
        if (item == null) return;

        // 인벤토리 목록도 같은 콜백을 쓰기 때문에, 구매 탭에서 내 물건을 눌러
        // '구매' 주문에 담기는 일이 생깁니다. 여기서 한 번 걸러냅니다.
        if (currentTab == Tab.Buy && !ShopSells(item))
        {
            SetFeedback("이 상점에서 팔지 않는 물건입니다.", false);
            return;
        }

        int inCart = cart.TryGetValue(item, out int n) ? n : 0;

        if (currentTab == Tab.Sell)
        {
            // 가진 것보다 많이 담을 수는 없습니다.
            if (inCart >= progress.Inventory.GetCount(item))
            {
                SetFeedback("더 담을 수 없습니다.", false);
                return;
            }
        }

        cart[item] = inCart + 1;
        RefreshCart();
    }

    private bool ShopSells(ItemData item)
    {
        if (shop == null || item == null) return false;

        foreach (ItemData sold in shop.ItemsForSale)
        {
            if (sold == item) return true;
        }

        return false;
    }

    private void RemoveFromCart(ItemData item)
    {
        if (item == null || !cart.ContainsKey(item)) return;

        cart[item]--;
        if (cart[item] <= 0) cart.Remove(item);

        RefreshCart();
    }

    private void ClearCart()
    {
        cart.Clear();
        RefreshCart();
    }

    private int CartTotal()
    {
        int total = 0;

        foreach (KeyValuePair<ItemData, int> pair in cart)
        {
            int unit = currentTab == Tab.Buy ? pair.Key.buyPrice : pair.Key.sellPrice;
            total += unit * pair.Value;
        }

        return total;
    }

    /// <summary>담은 것을 실제 거래로 확정합니다.</summary>
    private void Confirm()
    {
        if (cart.Count == 0)
        {
            SetFeedback("담은 물건이 없습니다.", false);
            return;
        }

        int done = 0;
        TradeResult lastFail = TradeResult.Success;

        // 거래가 인벤토리 이벤트를 일으켜 cart를 건드릴 수 있으므로 복사본을 순회합니다.
        List<KeyValuePair<ItemData, int>> order = new List<KeyValuePair<ItemData, int>>(cart);

        foreach (KeyValuePair<ItemData, int> pair in order)
        {
            for (int i = 0; i < pair.Value; i++)
            {
                TradeResult result = currentTab == Tab.Buy
                    ? shop.TryBuy(progress, pair.Key)
                    : shop.TrySell(progress, pair.Key);

                if (result == TradeResult.Success) { done++; continue; }

                lastFail = result;
                break;   // 이 품목은 더 진행하지 않습니다
            }
        }

        cart.Clear();

        if (done == 0)
        {
            SetFeedback(FailMessage(lastFail), false);
        }
        else if (lastFail != TradeResult.Success)
        {
            SetFeedback($"{done}개만 처리되었습니다 — {FailMessage(lastFail)}", false);
        }
        else
        {
            SetFeedback(currentTab == Tab.Buy ? $"{done}개 구매했습니다." : $"{done}개 판매했습니다.", true);
        }

        RefreshAll();
    }

    private string FailMessage(TradeResult result)
    {
        switch (result)
        {
            case TradeResult.NotEnoughGold: return "골드가 부족합니다.";
            case TradeResult.NoItem: return "가지고 있지 않습니다.";
            case TradeResult.NotTradable: return "거래할 수 없는 물건입니다.";
            default: return "거래에 실패했습니다.";
        }
    }

    // ────────────────────────────── 갱신

    private void HandleGoldChanged(int gold) => RefreshAll();

    private void RefreshAll()
    {
        if (!IsOpen || progress == null) return;

        RefreshGold();
        RefreshList();
        RefreshCart();
    }

    private void RefreshGold()
    {
        if (goldText != null) goldText.text = $"{progress.Wallet.Gold:N0} G";
    }

    private void RefreshList()
    {
        ClearChildren(rowContainer);

        int count = 0;

        if (currentTab == Tab.Buy)
        {
            foreach (ItemData item in shop.ItemsForSale)
            {
                if (item == null || item.buyPrice <= 0) continue;

                ItemSlotUI row = Instantiate(rowPrefab, rowContainer);
                row.Bind(item, 1, tooltip, AddToCart, item.buyPrice);
                count++;
            }
        }
        else
        {
            foreach (KeyValuePair<ItemData, int> pair in progress.Inventory.Items)
            {
                if (pair.Key == null || pair.Key.sellPrice <= 0) continue;   // 팔 수 없는 물건은 제외

                ItemSlotUI row = Instantiate(rowPrefab, rowContainer);
                row.Bind(pair.Key, pair.Value, tooltip, AddToCart, pair.Key.sellPrice);
                count++;
            }
        }

        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(count == 0);
            emptyText.text = currentTab == Tab.Buy
                ? "파는 물건이 없습니다"
                : "팔 수 있는 물건이 없습니다";
        }
    }

    private void RefreshCart()
    {
        int index = 0;

        foreach (KeyValuePair<ItemData, int> pair in cart)
        {
            if (index >= cartSlots.Count) break;

            int unit = currentTab == Tab.Buy ? pair.Key.buyPrice : pair.Key.sellPrice;
            cartSlots[index].Bind(pair.Key, pair.Value, tooltip, RemoveFromCart, unit);
            index++;
        }

        for (int i = index; i < cartSlots.Count; i++)
            cartSlots[i].Clear();

        RefreshTotals();
    }

    private void RefreshTotals()
    {
        int total = CartTotal();
        int gold = progress != null ? progress.Wallet.Gold : 0;
        int after = currentTab == Tab.Buy ? gold - total : gold + total;

        if (totalValueText != null) totalValueText.text = $"{total:N0}";

        if (afterValueText != null)
        {
            afterValueText.text = $"{after:N0}";

            // 살 수 없는 금액이면 빨갛게. 누르기 전에 알 수 있습니다.
            afterValueText.color = after < 0 ? FailColor : NormalValueColor;
        }

        if (confirmButton != null)
            confirmButton.interactable = cart.Count > 0 && after >= 0;
    }

    private void SetFeedback(string message, bool success)
    {
        if (feedbackText == null) return;

        feedbackText.text = message;
        feedbackText.color = success ? SuccessColor : FailColor;
        feedbackHideTime = Time.time + feedbackDuration;
    }
}