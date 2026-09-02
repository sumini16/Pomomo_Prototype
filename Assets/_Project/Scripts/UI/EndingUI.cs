using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 마지막 퀘스트를 수락하면 마무리 문구를 띄웁니다.
/// 프로토타입의 끝을 알리는 장치라 게임 로직은 건드리지 않고, 표시와 입력 차단만 합니다.
/// </summary>
public class EndingUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TextMeshProUGUI messageText;

    [Tooltip("이 퀘스트를 수락하면 엔딩을 띄웁니다. 목표가 없어 완료되지 않는 퀘스트라 수락 시점을 씁니다.")]
    [SerializeField] private QuestData finalQuest;

    [Tooltip("수락 대사를 읽을 시간을 준 뒤 시작합니다.")]
    [SerializeField] private float delay = 2f;
    [SerializeField] private float fadeDuration = 1.5f;

    [TextArea]
    [SerializeField] private string message = "여기까지가 프로토타입입니다.";

    private void Awake()
    {
        if (group == null)
        {
            Debug.LogError("[EndingUI] CanvasGroup이 비어 있습니다.", this);
            enabled = false;
            return;
        }

        // SetActive로 끄면 이 스크립트가 같은 오브젝트에 있어 함께 비활성화되고,
        // OnDisable이 구독을 해제해 수락 이벤트를 영영 받지 못합니다.
        // 오브젝트는 켜둔 채 투명하게만 만듭니다.
        Hide();
    }

    private void Hide()
    {
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }

    private void OnEnable() => QuestEvents.OnAccepted += HandleAccepted;
    private void OnDisable() => QuestEvents.OnAccepted -= HandleAccepted;

    private void HandleAccepted(QuestData quest)
    {
        if (finalQuest == null || quest != finalQuest) return;
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return new WaitForSeconds(delay);

        if (messageText != null) messageText.text = message;
        group.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        group.alpha = 1f;

        // 엔딩 이후에는 조작을 받지 않습니다.
        UIState.SetModal(true);
    }
}