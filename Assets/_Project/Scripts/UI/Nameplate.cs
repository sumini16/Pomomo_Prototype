using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class Nameplate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health targetHealth;   // 비워두면 부모에서 자동 탐색
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image fillImage;       // 비워두면 HealthBarRoot/Fill 자동 탐색

    [Header("Display")]
    [SerializeField] private string displayName;    // 비워두면 EnemyAI/Interactable에서 가져옴

    [Header("Debug")]
    [SerializeField] private bool logUpdates;

    private Transform cameraTransform;

    private void Awake()
    {
        if (targetHealth == null) targetHealth = GetComponentInParent<Health>();
        if (nameText == null) nameText = GetComponentInChildren<TMP_Text>(true);
        if (fillImage == null) ResolveFillImage();

        if (targetHealth == null)
            Debug.LogError($"[Nameplate] {name}: 부모에서 Health를 찾지 못했습니다.", this);
        if (fillImage == null)
            Debug.LogError($"[Nameplate] {name}: Fill Image가 비어 있습니다. HealthBarRoot/Fill 경로를 확인하세요.", this);
    }

    private void ResolveFillImage()
    {
        // GetComponentInChildren은 HealthBarRoot 자신의 Image를 먼저 잡으므로 경로로 찾는다
        Transform fill = transform.Find("HealthBarRoot/Fill");
        if (fill != null) fillImage = fill.GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (nameText != null) nameText.text = ResolveDisplayName();

        if (targetHealth == null) return;
        targetHealth.OnHealthChanged += HandleHealthChanged;
        HandleHealthChanged(targetHealth.Current, targetHealth.Max);   // 초기 1회 반영
    }

    private void OnDisable()
    {
        if (targetHealth != null) targetHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int current, int max)
    {
        float ratio = max <= 0 ? 0f : Mathf.Clamp01((float)current / max);

        if (logUpdates)
            Debug.Log($"[Nameplate] {targetHealth.name} → {current}/{max} = {ratio:0.00} / fillImage={(fillImage == null ? "NULL" : fillImage.name)}", this);

        if (fillImage != null) fillImage.fillAmount = ratio;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            if (Camera.main == null) return;
            cameraTransform = Camera.main.transform;
        }
        transform.rotation = cameraTransform.rotation;   // 항상 카메라를 향하게
    }

    private string ResolveDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(displayName)) return displayName;

        EnemyAI enemy = GetComponentInParent<EnemyAI>();
        if (enemy != null) return enemy.DisplayName;

        Interactable interactable = GetComponentInParent<Interactable>();
        if (interactable != null) return interactable.DisplayName;

        return name;
    }

   
}