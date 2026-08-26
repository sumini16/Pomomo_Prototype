using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Nameplate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject healthBarRoot;
    [SerializeField] private Image fillImage;

    [Header("Options")]
    [SerializeField] private string displayName = "이름";
    [SerializeField] private Health targetHealth;

    [Tooltip("체력이 가득 찼을 때 체력 바를 숨깁니다. 잡몹에 사용.")]
    [SerializeField] private bool hideBarWhenFull = true;
    [SerializeField] private float hideDelay = 3f;

    private Transform cameraTransform;
    private float lastDamageTime = -999f;

    private void Awake()
    {
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;

        nameText.text = displayName;

        // 체력이 없는 대상(NPC 등)은 바 자체를 끕니다
        if (targetHealth == null)
            healthBarRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void Start()
    {
        if (targetHealth == null) return;

        UpdateFill(targetHealth.Current, targetHealth.Max);
        healthBarRoot.SetActive(!hideBarWhenFull);
    }

    private void HandleHealthChanged(int current, int max)
    {
        UpdateFill(current, max);

        lastDamageTime = Time.time;
        healthBarRoot.SetActive(true);
    }

    private void UpdateFill(int current, int max)
    {
        fillImage.fillAmount = max > 0 ? (float)current / max : 0f;
    }

    private void Update()
    {
        if (!hideBarWhenFull || targetHealth == null) return;

        // 피해를 입은 뒤 일정 시간이 지나면 다시 숨김
        if (healthBarRoot.activeSelf && Time.time - lastDamageTime > hideDelay)
            healthBarRoot.SetActive(false);
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        // 항상 카메라를 향하게 (빌보드)
        transform.forward = cameraTransform.forward;
    }
}