using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    [SerializeField] private Health targetHealth;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    private void Awake()
    {
        if (targetHealth == null)
            Debug.LogError($"{name}: 대상 Health가 지정되지 않았습니다.");
    }

    private void OnEnable()
    {
        targetHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        targetHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void Start()
    {
        HandleHealthChanged(targetHealth.Current, targetHealth.Max);
    }

    private void HandleHealthChanged(int current, int max)
    {
        fillImage.fillAmount = max > 0 ? (float)current / max : 0f;
        healthText.text = $"{current} / {max}";
    }
}