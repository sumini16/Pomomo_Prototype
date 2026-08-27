using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private ItemSlotUI slotPrefab;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private TextMeshProUGUI emptyText;
    [SerializeField] private ItemTooltip tooltip;
    private bool isOpen;

    private void Awake() => panelRoot.SetActive(false);

    private void OnEnable() => inventory.OnInventoryChanged += RefreshIfOpen;
    private void OnDisable() => inventory.OnInventoryChanged -= RefreshIfOpen;

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.iKey.wasPressedThisFrame)
            Toggle();
    }

    private void Toggle()
    {
        isOpen = !isOpen;
        panelRoot.SetActive(isOpen);

        if (isOpen)
            Refresh();
        else
            tooltip.Hide();
    }

    private void RefreshIfOpen()
    {
        if (isOpen) Refresh();


    }

    private void Refresh()
    {
        // 기존 슬롯 정리
        for (int i = slotContainer.childCount - 1; i >= 0; i--)
            Destroy(slotContainer.GetChild(i).gameObject);

        foreach (var pair in inventory.Items)
        {
            ItemSlotUI slot = Instantiate(slotPrefab, slotContainer);
            slot.Bind(pair.Key, pair.Value, tooltip);
        }
    }
}