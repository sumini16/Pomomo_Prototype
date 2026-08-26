using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TextMeshProUGUI promptText;

    private Interactable lastTarget;

    private void Awake()
    {
        if (interactor == null)
            Debug.LogError($"{name}: PlayerInteractor가 지정되지 않았습니다.");

        promptRoot.SetActive(false);
    }

    private void Update()
    {
        Interactable target = interactor.CurrentTarget;

        // 대상이 바뀌지 않았으면 아무것도 하지 않음
        if (target == lastTarget) return;

        lastTarget = target;

        if (target == null)
        {
            promptRoot.SetActive(false);
            return;
        }

        promptText.text = $"{target.DisplayName}  [E]";
        promptRoot.SetActive(true);
    }
}