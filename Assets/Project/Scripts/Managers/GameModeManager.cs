using UnityEngine;

public enum GameMode
{
    Normal,     // Chơi thường
    Challenge   // Chơi thử thách
}

/// <summary>
/// Trạm trung chuyển dữ liệu giữa SC_MenuGame và SC_Gameplay.
/// Do là biến static, nó sống độc lập và không bị reset khi chuyển Scene.
/// </summary>
public static class GameModeManager
{
    // Mặc định lúc vừa bật game lên là chế độ thường
    public static GameMode CurrentMode { get; private set; } = GameMode.Normal;

    // Dữ liệu thử thách đang được chọn ở Menu (nếu có)
    public static ChallengeData CurrentChallenge { get; private set; } = null;

    // ── HÀM ĐƯỢC GỌI TỪ MENU BÊN NGOÀI ──

    /// <summary>
    /// Gọi khi người chơi bấm nút "PLAY" bình thường
    /// </summary>
    public static void SetNormalMode()
    {
        CurrentMode = GameMode.Normal;
        CurrentChallenge = null;
        Debug.Log("GameModeManager: Chuyển sang chế độ NORMAL");
    }

    /// <summary>
    /// Gọi khi người chơi bấm vào 1 Thử thách cụ thể
    /// </summary>
    public static void SetChallengeMode(ChallengeData challenge)
    {
        if (challenge == null)
        {
            Debug.LogError("GameModeManager: ChallengeData truyền vào bị rỗng!");
            return;
        }

        CurrentMode = GameMode.Challenge;
        CurrentChallenge = challenge;
        Debug.Log($"GameModeManager: Chuyển sang chế độ CHALLENGE -> {challenge.challengeName}");
    }
}