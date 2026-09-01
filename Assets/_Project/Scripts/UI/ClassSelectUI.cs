using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 직업 목록을 카드로 펼치고, 고른 직업을 GameManager에 넘긴 뒤 게임 씬을 엽니다.
/// 카드를 손으로 배치하지 않고 ClassDatabase를 순회해 만들기 때문에,
/// 직업을 추가해도 이 씬은 수정할 것이 없습니다.
/// </summary>
public class ClassSelectUI : MonoBehaviour
{
    [SerializeField] private ClassDatabase database;
    [SerializeField] private ClassCardUI cardPrefab;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Button startButton;

    [Tooltip("Build 목록에 등록된 이름과 정확히 같아야 합니다.")]
    [SerializeField] private string gameSceneName = "SampleScene";

    private readonly List<ClassCardUI> cards = new List<ClassCardUI>();
    private ClassData selected;

    private void Start()
    {
        if (database == null || cardPrefab == null || cardContainer == null)
        {
            Debug.LogError("[ClassSelectUI] 인스펙터 연결이 비어 있습니다.", this);
            return;
        }

        BuildCards();

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }

        UpdateStartButton();
    }

    private void BuildCards()
    {
        foreach (ClassData data in database.All)
        {
            if (data == null) continue;

            ClassCardUI card = Instantiate(cardPrefab, cardContainer);
            card.Bind(data, Select);
            cards.Add(card);
        }
    }

    private void Select(ClassData data)
    {
        selected = data;

        foreach (ClassCardUI card in cards)
            card.SetSelected(card.Data == data);

        UpdateStartButton();
    }

    private void UpdateStartButton()
    {
        // 아무것도 고르지 않은 채로 시작하는 것을 막습니다.
        if (startButton != null) startButton.interactable = selected != null;
    }

    private void StartGame()
    {
        if (selected == null) return;

        if (GameManager.Instance != null)
            GameManager.Instance.SelectClass(selected);
        else
            Debug.LogWarning("[ClassSelectUI] GameManager가 없어 선택이 전달되지 않습니다.", this);

        SceneManager.LoadScene(gameSceneName);
    }
}