using UnityEngine;

/// <summary>상점을 여는 NPC. 판정은 Shop이, 표시는 ShopUI가 담당합니다.</summary>
[RequireComponent(typeof(Shop))]
public class ShopNpc : Interactable
{
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private string shopName = "잡화점";

    [Tooltip("이 상인이 대화형 목표의 대상일 경우 지정합니다. 없으면 비워둡니다.")]
    [SerializeField] private NpcData npcData;

    private Shop shop;

    private void Awake() => shop = GetComponent<Shop>();

    public override void Interact(GameObject interactor)
    {
        if (!interactor.TryGetComponent(out PlayerProgress progress))
        {
            Debug.LogError($"{name}: 상호작용 대상에 PlayerProgress가 없습니다.");
            return;
        }

        progress.Flags.MarkTalked(npcData);
        shopUI.Open(shop, progress, shopName);
    }
}