using System;

/// <summary>
/// 화면을 덮는 UI(상점 등)가 열려 있는지를 알리는 창구.
/// 플레이어 입력 스크립트들이 이 값만 보고 스스로 입력을 무시합니다.
/// UI가 PlayerController를 직접 끄지 않으므로 서로를 알 필요가 없습니다.
/// </summary>
public static class UIState
{
    public static bool IsModalOpen { get; private set; }

    public static event Action<bool> OnModalChanged;

    public static void SetModal(bool open)
    {
        if (IsModalOpen == open) return;

        IsModalOpen = open;
        OnModalChanged?.Invoke(open);
    }
}