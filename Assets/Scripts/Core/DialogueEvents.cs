using System;

public static class DialogueEvents
{
    /// <summary>선택지 없는 일반 대사. (화자, 대사)</summary>
    public static event Action<string, string> OnRequested;

    /// <summary>수락/거절을 묻는 대사. (화자, 대사, 수락 시 실행, 거절 시 실행)</summary>
    public static event Action<string, string, Action, Action> OnChoiceRequested;

    public static void Request(string speaker, string line)
        => OnRequested?.Invoke(speaker, line);

    public static void RequestChoice(string speaker, string line,
                                     Action onAccept, Action onDecline)
        => OnChoiceRequested?.Invoke(speaker, line, onAccept, onDecline);
}