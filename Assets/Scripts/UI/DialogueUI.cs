using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI lineText;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;

    // 버튼이 눌렸을 때 실행할 동작. UI는 내용을 모르고 보관만 합니다.
    private Action pendingAccept;
    private Action pendingDecline;

    private void Awake()
    {
        panelRoot.SetActive(false);

        closeButton.onClick.AddListener(Hide);
        acceptButton.onClick.AddListener(OnAcceptClicked);
        declineButton.onClick.AddListener(OnDeclineClicked);
    }

    private void OnDestroy()
    {
        closeButton.onClick.RemoveListener(Hide);
        acceptButton.onClick.RemoveListener(OnAcceptClicked);
        declineButton.onClick.RemoveListener(OnDeclineClicked);
    }

    private void OnEnable()
    {
        DialogueEvents.OnRequested += ShowSimple;
        DialogueEvents.OnChoiceRequested += ShowChoice;
    }

    private void OnDisable()
    {
        DialogueEvents.OnRequested -= ShowSimple;
        DialogueEvents.OnChoiceRequested -= ShowChoice;
    }

    private void ShowSimple(string speaker, string line)
    {
        SetContent(speaker, line);

        closeButton.gameObject.SetActive(true);
        acceptButton.gameObject.SetActive(false);
        declineButton.gameObject.SetActive(false);

        pendingAccept = null;
        pendingDecline = null;
    }

    private void ShowChoice(string speaker, string line, Action onAccept, Action onDecline)
    {
        SetContent(speaker, line);

        closeButton.gameObject.SetActive(false);
        acceptButton.gameObject.SetActive(true);
        declineButton.gameObject.SetActive(true);

        pendingAccept = onAccept;
        pendingDecline = onDecline;
    }

    private void SetContent(string speaker, string line)
    {
        speakerText.text = speaker;
        lineText.text = line;
        panelRoot.SetActive(true);
    }

    private void OnAcceptClicked()
    {
        Action action = pendingAccept;   // Hide가 먼저 비우므로 미리 보관
        Hide();
        action?.Invoke();
    }

    private void OnDeclineClicked()
    {
        Action action = pendingDecline;
        Hide();
        action?.Invoke();
    }

    private void Hide()
    {
        panelRoot.SetActive(false);
        pendingAccept = null;
        pendingDecline = null;
    }
}