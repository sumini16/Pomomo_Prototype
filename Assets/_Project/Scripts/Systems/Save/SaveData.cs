using System;
using System.Collections.Generic;

/// <summary>
/// 저장 파일의 형태입니다.
///
/// 런타임에는 Dictionary와 ScriptableObject 참조를 쓰지만, JsonUtility는 둘 다 직렬화하지 못합니다.
/// 그래서 여기서는 Dictionary를 List로 펼치고, 에셋 참조는 문자열 id로 바꿔 담습니다.
/// 저장 형식과 런타임 자료구조를 분리한 것이기도 합니다  한쪽을 바꿔도 다른 쪽이 따라오지 않습니다.
/// </summary>
[Serializable]
public class SaveData
{
    [Serializable]
    public class CountEntry
    {
        public string id;
        public int count;
    }

    [Serializable]
    public class QuestEntry
    {
        public string id;

        // enum을 int로 저장하면 나중에 항목 순서가 바뀔 때 조용히 어긋납니다.
        // 이름으로 저장하면 순서가 바뀌어도 안전합니다.
        public string state;
    }

    public int version = 1;
    public string savedAtUtc;

    public string classId;
    public int gold;

    public float posX;
    public float posY;
    public float posZ;
    public float rotY;

    public List<CountEntry> inventory = new List<CountEntry>();
    public List<CountEntry> kills = new List<CountEntry>();
    public List<QuestEntry> quests = new List<QuestEntry>();
    public List<string> talkedNpcIds = new List<string>();
}