using UnityEngine;


[RequireComponent(typeof(Shop))]
public class ShopNpc : Interactable
{
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private string shopName = "잡화점";

    
    [SerializeField] private NpcData npcData;

    private Shop shop;

   
    public override string DisplayName =>
        npcData != null ? npcData.displayName : base.DisplayName;

    private void Awake() => shop = GetComponent<Shop>();

    public override void Interact(GameObject interactor)
    {
        if (!interactor.TryGetComponent(out PlayerProgress progress))
        {
            
            return;
        }

        // NpcData가 없는 상인도 있을 수 있으므로 기록은 있을 때만 남깁니다.
        if (npcData != null) progress.Flags.MarkTalked(npcData);

        shopUI.Open(shop, progress, shopName);
    }
}