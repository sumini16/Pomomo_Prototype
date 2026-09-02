using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 저장 파일의 읽기 쓰기만 담당합니다.
/// 무엇을 담을지는 SaveCoordinator가 정하고, 이 클래스는 그 결과를 파일로 옮기기만 합니다.
/// </summary>
public static class SaveSystem
{
    public const int CurrentVersion = 1;
    private const string FileName = "save.json";

    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);
    public static bool HasSave => File.Exists(FilePath);

    public static bool Save(SaveData data)
    {
        if (data == null) return false;

        data.version = CurrentVersion;
        data.savedAtUtc = DateTime.UtcNow.ToString("o");

        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
            Debug.Log($"[SaveSystem] 저장 완료  {FilePath}");
            return true;
        }
        catch (Exception e)
        {
            // 디스크가 가득 찼거나 권한이 없을 수 있습니다. 저장 실패가 게임을 멈추지는 않게 합니다.
            Debug.LogError($"[SaveSystem] 저장 실패: {e.Message}");
            return false;
        }
    }

    public static SaveData Load()
    {
        if (!HasSave)
        {
            Debug.Log("[SaveSystem] 저장 파일이 없습니다.");
            return null;
        }

        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));

            if (data == null)
            {
                Debug.LogError("[SaveSystem] 저장 파일을 해석하지 못했습니다.");
                return null;
            }

            // 형식이 바뀐 옛 파일을 그대로 읽으면 필드가 어긋난 채 복원됩니다.
            // 잘못 복원하느니 불러오지 않는 쪽을 택했습니다.
            if (data.version != CurrentVersion)
            {
                Debug.LogWarning($"[SaveSystem] 저장 형식이 다릅니다 (파일 {data.version} / 현재 {CurrentVersion}). 불러오지 않습니다.");
                return null;
            }

            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] 불러오기 실패: {e.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        if (!HasSave) return;

        try
        {
            File.Delete(FilePath);
            Debug.Log("[SaveSystem] 저장 파일을 삭제했습니다.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] 삭제 실패: {e.Message}");
        }
    }
}