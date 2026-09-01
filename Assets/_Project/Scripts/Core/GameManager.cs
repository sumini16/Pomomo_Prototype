using System;
using UnityEngine;
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }
    /// <summary>(이전 상태, 새 상태) 순으로 전달됩니다.</summary>
    public event Action<GameState, GameState> OnStateChanged;


    [Header("직업")]
    [SerializeField] private ClassDatabase classDatabase;

    /// <summary>선택된 직업. 선택 씬을 거치지 않고 바로 실행하면 null입니다.</summary>
    public ClassData SelectedClass { get; private set; }
    public ClassDatabase Classes => classDatabase;
    private void Awake()
    {
        if(Instance != null && Instance !=this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);


        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
    }


    public void SetState(GameState newState)
    {
        //같은 상태라면
        if (CurrentState == newState) return;

        GameState pre = CurrentState;
        CurrentState = newState;

        ApplyTimeScale(newState);
        OnStateChanged?.Invoke(pre, newState);
    }
    private void ApplyTimeScale(GameState state)
    {
        Time.timeScale = state == GameState.Paused ? 0f : 1f;
    }

    public void SelectClass(ClassData data)
    {
        SelectedClass = data;
    }

    /// <summary>세이브 복원용. 저장된 id로 직업을 되돌립니다.</summary>
    public void SelectClassById(string id)
    {
        SelectedClass = classDatabase != null ? classDatabase.GetById(id) : null;
    }
}
