using System;
using UnityEngine;
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }
    /// <summary>(이전 상태, 새 상태) 순으로 전달됩니다.</summary>
    public event Action<GameState, GameState> OnStateChanged;

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
}
