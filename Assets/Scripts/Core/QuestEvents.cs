using System;

/// <summary>
/// 퀘스트 진행 상황을 알리는 창구.
/// QuestGiver는 UI를 모르고, UI는 어느 NPC가 준 퀘스트인지 모릅니다.
/// </summary>
public static class QuestEvents
{
    public static event Action<QuestData> OnAccepted;
    public static event Action<QuestData> OnCompleted;

    public static void Accepted(QuestData quest) => OnAccepted?.Invoke(quest);
    public static void Completed(QuestData quest) => OnCompleted?.Invoke(quest);
}