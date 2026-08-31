using TMPro;
using UnityEngine;

/// <summary>화면에 늘 떠 있는 골드 표시. 값을 소유하지 않고 Wallet을 구독만 합니다.</summary>
public class GoldHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private PlayerProgress progress;

    private void OnEnable()
    {
        if (progress == null)
        {
            Debug.LogError($"{name}: PlayerProgress가 할당되지 않았습니다.");
            return;
        }

        progress.Wallet.OnGoldChanged += Refresh;
        Refresh(progress.Wallet.Gold);   // 현재 값으로 1회 갱신
    }

    private void OnDisable()
    {
        if (progress != null)
            progress.Wallet.OnGoldChanged -= Refresh;
    }

    private void Refresh(int gold) => goldText.text = $"{gold:N0} G";
}