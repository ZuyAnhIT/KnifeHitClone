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

    // Biến lưu số thứ tự Challenge hiện tại (1, 2, 3...) để hiển thị Text
    public static int CurrentChallengeLevel { get; private set; } = 1;

    // ── HÀM ĐƯỢC GỌI TỪ MENU BÊN NGOÀI ──

    /// <summary>
    /// Gọi khi người chơi bấm nút "PLAY" bình thường
    /// </summary>
    public static void SetNormalMode()
    {
        CurrentMode = GameMode.Normal;
        CurrentChallenge = null;
        CurrentChallengeLevel = 1; // Reset về cấp 1 khi thoát ra chơi thường
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

        // ── BƯỚC MỚI: TẢI TIẾN ĐỘ ĐÃ LƯU TỪ TRONG MÁY ──
        // Đọc xem người chơi đã đến Challenge mấy. Nếu chưa chơi bao giờ thì trả về 1.
        CurrentChallengeLevel = PlayerPrefs.GetInt($"Challenge_{challenge.challengeName}_Level", 1);

        Debug.Log($"GameModeManager: Chuyển sang chế độ CHALLENGE -> {challenge.challengeName} | Bắt đầu từ Level {CurrentChallengeLevel}");
    }

    /// <summary>
    /// Gọi từ UI Chúc mừng khi người chơi hoàn thành 1 Challenge
    /// Hàm này tăng Level và Lưu ngay tiến độ xuống thiết bị
    /// </summary>
    public static void CompleteCurrentAndMoveToNext()
    {
        if (CurrentChallenge == null) return;

        CurrentChallengeLevel++; // Tăng cấp độ lên (VD: 1 -> 2)

        // Lưu thẳng xuống bộ nhớ thiết bị
        PlayerPrefs.SetInt($"Challenge_{CurrentChallenge.challengeName}_Level", CurrentChallengeLevel);
        PlayerPrefs.Save();

        Debug.Log($"GameModeManager: Đã lưu tiến độ {CurrentChallenge.challengeName} -> Lên Level {CurrentChallengeLevel}");
    }
}