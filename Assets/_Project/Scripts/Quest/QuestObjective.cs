using UnityEngine;

/// <summary>
/// 퀘스트 완료 조건의 추상 베이스.
///
/// 세 종류(수집·처치·대화)가 각각 다른 저장소에 완료 여부를 물어보지만,
/// QuestGiver는 IsComplete 한 줄만 호출하므로 목표가 늘어나도 수정되지 않습니다.
///
/// 완료 여부는 '사건 집계'가 아니라 '묻는 시점의 상태 조회'로 판정합니다.
/// 언제 몇 번에 걸쳐 달성했는지가 조건에 영향을 주지 않습니다.
/// </summary>
public abstract class QuestObjective : ScriptableObject
{
    /// <summary>지금 이 순간 조건이 충족되어 있는가.</summary>
    public abstract bool IsComplete(QuestContext ctx);

    /// <summary>퀘스트 트래커에 표시할 진행 문구.</summary>
    public abstract string GetProgressText(QuestContext ctx);

    /// <summary>
    /// 퀘스트 완료 처리 시 1회 실행. 소모가 필요한 목표만 재정의합니다.
    /// (수집은 아이템 차감, 처치·대화는 아무것도 하지 않음)
    /// </summary>
    public virtual void OnQuestCompleted(QuestContext ctx) { }
}
