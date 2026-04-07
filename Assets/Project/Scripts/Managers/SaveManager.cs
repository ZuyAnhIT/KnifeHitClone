using UnityEngine;

/// <summary>
/// =====================================================================
/// SaveManager.cs — Lưu và load toàn bộ dữ liệu game
/// =====================================================================
/// CÁCH HOẠT ĐỘNG TỔNG QUAN:
///
///   - Dùng Singleton (Instance) → truy cập từ bất kỳ script nào
///   - DontDestroyOnLoad → tồn tại xuyên suốt mọi scene
///   - Dữ liệu lưu bằng PlayerPrefs → tồn tại sau khi tắt app
///   - Mỗi loại dữ liệu có: key hằng số, cached value, Save method, getter
///
/// DỮ LIỆU ĐANG QUẢN LÝ:
///   1. BestStage        — Màn cao nhất đã đạt được
///   2. BestScore        — Điểm cao nhất
///   3. TotalApple       — Tổng số táo tích lũy
///   4. SelectedKnife    — Dao đang được chọn (page + slot)
///   5. KnifeUnlock      — Trạng thái mở khóa từng con dao
///   6. GiftTimer        — Đồng hồ hộp quà (tính thời gian thực)
///   7. [MỚI] Settings   — Sound / Vibration / LeftHand
///
/// CÁCH SỬ DỤNG TỪ SCRIPT KHÁC:
///   SaveManager.Instance.SoundEnabled        ← đọc
///   SaveManager.Instance.SaveSoundEnabled(false) ← ghi
/// =====================================================================
/// </summary>
public class SaveManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────
    // Chỉ tồn tại 1 instance duy nhất trong toàn bộ game
    public static SaveManager Instance { get; private set; }

    // ═══════════════════════════════════════════════════════════════
    // KEYS — Tên khóa lưu trong PlayerPrefs
    // ═══════════════════════════════════════════════════════════════
    // Dùng hằng số (const) để tránh lỗi typo khi gõ tay nhiều lần
    private const string KEY_BEST_STAGE = "BestStage";
    private const string KEY_BEST_SCORE = "BestScore";
    private const string KEY_TOTAL_APPLE = "TotalApple";
    private const string KEY_GIFT_CLOSE_TIME = "GiftCloseTime";
    private const string KEY_GIFT_TIME_REMAINING = "GiftTimeRemaining";
    private const string KEY_SELECTED_KNIFE_PAGE = "SelectedKnifePage";
    private const string KEY_SELECTED_KNIFE_SLOT = "SelectedKnifeSlot";

    // Format key unlock dao: "KnifeUnlock_P0_S3" (page 0, slot 3)
    private const string KEY_KNIFE_UNLOCK_PREFIX = "KnifeUnlock_P";

    // [MỚI] Keys cho Settings
    private const string KEY_SOUND_ENABLED = "SoundEnabled";
    private const string KEY_VIBRATION_ENABLED = "VibrationEnabled";
    private const string KEY_LEFT_HAND_ENABLED = "LeftHandEnabled";

    // ═══════════════════════════════════════════════════════════════
    // CACHED VALUES — Giá trị đã load vào RAM
    // ═══════════════════════════════════════════════════════════════
    // Lý do cache: Đọc PlayerPrefs từng frame sẽ chậm.
    // Thay vào đó: load 1 lần khi Awake, sau đó đọc từ biến này.
    private int _bestStage;
    private int _bestScore;
    private int _totalApple;
    private int _selectedKnifePage;
    private int _selectedKnifeSlot;

    // [MỚI] Cached settings
    private bool _soundEnabled;
    private bool _vibrationEnabled;
    private bool _leftHandEnabled;

    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    private void Awake()
    {
        // Singleton pattern: Nếu đã có instance khác → tự hủy
        // Điều này xảy ra khi load lại scene có chứa SaveManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Tồn tại xuyên scene
        LoadAll();                     // Load toàn bộ dữ liệu ngay khi tạo
    }

    // ═══════════════════════════════════════════════════════════════
    // LOAD ALL — Gọi 1 lần duy nhất khi Awake
    // ═══════════════════════════════════════════════════════════════
    private void LoadAll()
    {
        _bestStage = PlayerPrefs.GetInt(KEY_BEST_STAGE, 0);
        _bestScore = PlayerPrefs.GetInt(KEY_BEST_SCORE, 0);
        _totalApple = PlayerPrefs.GetInt(KEY_TOTAL_APPLE, 0);
        _selectedKnifePage = PlayerPrefs.GetInt(KEY_SELECTED_KNIFE_PAGE, 0);
        _selectedKnifeSlot = PlayerPrefs.GetInt(KEY_SELECTED_KNIFE_SLOT, 0);

        // [MỚI] Load settings
        // Tham số thứ 2 là giá trị mặc định nếu chưa lưu lần nào:
        //   Sound=1 (bật), Vibration=1 (bật), LeftHand=0 (tắt = tay phải)
        _soundEnabled = PlayerPrefs.GetInt(KEY_SOUND_ENABLED, 1) == 1;
        _vibrationEnabled = PlayerPrefs.GetInt(KEY_VIBRATION_ENABLED, 1) == 1;
        _leftHandEnabled = PlayerPrefs.GetInt(KEY_LEFT_HAND_ENABLED, 0) == 1;

        // Áp dụng âm lượng ngay khi load (trước khi bất kỳ scene nào hiện)
        // Đảm bảo Sound OFF vẫn được giữ ngay cả khi vào màn hình mới
        AudioListener.volume = _soundEnabled ? 1f : 0f;

        Debug.Log($"SaveManager: Loaded → " +
                  $"Stage={_bestStage} | Score={_bestScore} | Apple={_totalApple} | " +
                  $"Knife=P{_selectedKnifePage}_S{_selectedKnifeSlot} | " +
                  $"Sound={_soundEnabled} | Vib={_vibrationEnabled} | Left={_leftHandEnabled}");
    }

    // ═══════════════════════════════════════════════════════════════
    // SAVE — BEST STAGE
    // ═══════════════════════════════════════════════════════════════
    /// <summary>
    /// Chỉ lưu nếu stage mới CAO HƠN stage hiện tại.
    /// Gọi mỗi khi người chơi hoàn thành một màn.
    /// </summary>
    public void UpdateBestStage(int stage)
    {
        if (stage <= _bestStage) return; // Không cần ghi nếu không phải kỷ lục mới

        _bestStage = stage;
        PlayerPrefs.SetInt(KEY_BEST_STAGE, _bestStage);
        PlayerPrefs.Save();
        Debug.Log($"SaveManager: New Best Stage = {_bestStage}");
    }

    // ═══════════════════════════════════════════════════════════════
    // SAVE — BEST SCORE
    // ═══════════════════════════════════════════════════════════════
    /// <summary>
    /// Chỉ lưu nếu score mới CAO HƠN score hiện tại.
    /// </summary>
    public void UpdateBestScore(int score)
    {
        if (score <= _bestScore) return;

        _bestScore = score;
        PlayerPrefs.SetInt(KEY_BEST_SCORE, _bestScore);
        PlayerPrefs.Save();
        Debug.Log($"SaveManager: New Best Score = {_bestScore}");
    }

    // ═══════════════════════════════════════════════════════════════
    // SAVE — TOTAL APPLE
    // ═══════════════════════════════════════════════════════════════
    /// <summary>
    /// Cộng dồn táo vào tổng. Gọi mỗi khi nhặt được táo.
    /// </summary>
    public void AddApple(int amount)
    {
        _totalApple += amount;
        PlayerPrefs.SetInt(KEY_TOTAL_APPLE, _totalApple);
        PlayerPrefs.Save();
        Debug.Log($"SaveManager: TotalApple = {_totalApple}");
    }

    // ═══════════════════════════════════════════════════════════════
    // SAVE — SELECTED KNIFE
    // ═══════════════════════════════════════════════════════════════
    /// <summary>
    /// Lưu dao đang chọn (page + slot).
    /// Gọi từ KnifeMenuManager khi người chơi chọn dao đã mở khóa.
    /// </summary>
    public void SaveSelectedKnife(int pageIndex, int slotIndex)
    {
        _selectedKnifePage = pageIndex;
        _selectedKnifeSlot = slotIndex;
        PlayerPrefs.SetInt(KEY_SELECTED_KNIFE_PAGE, _selectedKnifePage);
        PlayerPrefs.SetInt(KEY_SELECTED_KNIFE_SLOT, _selectedKnifeSlot);
        PlayerPrefs.Save();
        Debug.Log($"SaveManager: SelectedKnife → Page={pageIndex}, Slot={slotIndex}");
    }

    // ═══════════════════════════════════════════════════════════════
    // SAVE / LOAD — TRẠNG THÁI UNLOCK DAO
    // ═══════════════════════════════════════════════════════════════
    /// <summary>
    /// Đánh dấu dao (pageIndex, slotIndex) là đã mở khóa.
    /// Gọi ngay sau khi set isUnlocked = true trong KnifeMenuManager.
    /// </summary>
    public void SaveKnifeUnlock(int pageIndex, int slotIndex)
    {
        string key = KEY_KNIFE_UNLOCK_PREFIX + pageIndex + "_S" + slotIndex;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        Debug.Log($"SaveManager: KnifeUnlocked → Page={pageIndex}, Slot={slotIndex}");
    }

    /// <summary>
    /// Kiểm tra dao (pageIndex, slotIndex) đã được mở khóa chưa.
    /// </summary>
    public bool LoadKnifeUnlock(int pageIndex, int slotIndex)
    {
        string key = KEY_KNIFE_UNLOCK_PREFIX + pageIndex + "_S" + slotIndex;
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    // ═══════════════════════════════════════════════════════════════
    // [MỚI] SAVE — SETTINGS (Sound / Vibration / LeftHand)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Bật/tắt âm thanh toàn game.
    /// Áp dụng ngay bằng AudioListener.volume sau khi lưu.
    /// Gọi từ CodeToggle khi người chơi bấm toggle Sound.
    /// </summary>
    public void SaveSoundEnabled(bool value)
    {
        _soundEnabled = value;
        PlayerPrefs.SetInt(KEY_SOUND_ENABLED, value ? 1 : 0);
        PlayerPrefs.Save();

        // Áp dụng tức thì — không cần chờ frame sau
        AudioListener.volume = value ? 1f : 0f;

        Debug.Log($"SaveManager: SoundEnabled = {value}");
    }

    /// <summary>
    /// Bật/tắt rung.
    /// Không có API tắt rung trực tiếp → chỉ lưu cờ.
    /// Mọi chỗ muốn rung phải kiểm tra: if (SaveManager.Instance.VibrationEnabled) Handheld.Vibrate();
    /// Gọi từ CodeToggle khi người chơi bấm toggle Vibration.
    /// </summary>
    public void SaveVibrationEnabled(bool value)
    {
        _vibrationEnabled = value;
        PlayerPrefs.SetInt(KEY_VIBRATION_ENABLED, value ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"SaveManager: VibrationEnabled = {value}");
    }

    /// <summary>
    /// Bật/tắt chế độ tay trái.
    /// Chỉ lưu cờ — script xử lý vùng chạm đọc getter này để tự điều chỉnh.
    /// Gọi từ CodeToggle khi người chơi bấm toggle LeftHand.
    /// </summary>
    public void SaveLeftHandEnabled(bool value)
    {
        _leftHandEnabled = value;
        PlayerPrefs.SetInt(KEY_LEFT_HAND_ENABLED, value ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"SaveManager: LeftHandEnabled = {value}");
    }

    // ═══════════════════════════════════════════════════════════════
    // SAVE — GIFT BOX TIMER
    // ═══════════════════════════════════════════════════════════════
    // Lưu thời điểm đóng app + thời gian còn lại
    // Khi mở lại: tính delta thời gian thực để trừ bớt
    public void SaveGiftTimer(float timeRemaining)
    {
        PlayerPrefs.SetString(KEY_GIFT_CLOSE_TIME,
            System.DateTime.UtcNow.ToString("o")); // "o" = ISO 8601, bảo toàn múi giờ
        PlayerPrefs.SetFloat(KEY_GIFT_TIME_REMAINING, timeRemaining);
        PlayerPrefs.Save();
    }

    public float LoadGiftTimer(float defaultDuration)
    {
        string savedTime = PlayerPrefs.GetString(KEY_GIFT_CLOSE_TIME, "");
        if (string.IsNullOrEmpty(savedTime)) return defaultDuration;

        System.DateTime closeTime = System.DateTime.Parse(
            savedTime, null,
            System.Globalization.DateTimeStyles.RoundtripKind);

        // Tính số giây đã trôi qua kể từ lúc lưu
        float elapsed = (float)(System.DateTime.UtcNow - closeTime).TotalSeconds;
        float saved = PlayerPrefs.GetFloat(KEY_GIFT_TIME_REMAINING, defaultDuration);

        return Mathf.Max(0f, saved - elapsed); // Không cho về số âm
    }
    // ═══════════════════════════════════════════════════════════════
    // SAVE — VideoProgress
    // ═══════════════════════════════════════════════════════════════
    // Lưu số  lần xem quảng cáo của 1 ô dao chưa mở kháo
    // Lưu số video còn lại cần xem cho 1 con dao cụ thể
    public void SaveVideoProgress(int pageIndex, int slotIndex, int remaining)
    {
        string key = $"VideoLeft_P{pageIndex}_S{slotIndex}";
        PlayerPrefs.SetInt(key, remaining);
        PlayerPrefs.Save();
    }

    // Load số video còn lại (Mặc định là 4 nếu chưa bao giờ xem)
    public int LoadVideoProgress(int pageIndex, int slotIndex)
    {
        string key = $"VideoLeft_P{pageIndex}_S{slotIndex}";
        return PlayerPrefs.GetInt(key, 4); // Số 4 là giá trị mặc định ban đầu
    }


    // ═══════════════════════════════════════════════════════════════
    // GETTERS — Đọc giá trị cached (nhanh, không đọc lại PlayerPrefs)
    // ═══════════════════════════════════════════════════════════════
    public int BestStage => _bestStage;
    public int BestScore => _bestScore;
    public int TotalApple => _totalApple;
    public int SelectedKnifePage => _selectedKnifePage;
    public int SelectedKnifeSlot => _selectedKnifeSlot;

    // [MỚI] Settings getters
    public bool SoundEnabled => _soundEnabled;
    public bool VibrationEnabled => _vibrationEnabled;
    public bool LeftHandEnabled => _leftHandEnabled;
}