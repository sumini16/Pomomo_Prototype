using UnityEngine;

/// <summary>
/// 적의 종류를 식별하는 데이터. ItemData와 같은 이유로 문자열이 아닌 에셋 참조로 식별합니다.
/// 표시 이름을 바꿔도 참조 비교는 영향을 받지 않습니다.
/// </summary>
[CreateAssetMenu(fileName = "Enemy_", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Tooltip("세이브 파일에 기록되는 고유 키. 한 번 정하면 바꾸지 않습니다.")]
    public string id;

    public string displayName;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
}
