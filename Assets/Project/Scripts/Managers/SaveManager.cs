using UnityEngine;

/// <summary>
/// Lưu và load dữ liệu game
/// Dùng PlayerPrefs để lưu local
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    // ── Keys ──
    private const string KEY_BEST_STAGE = "BestStage";
    private const string KEY_BEST_SCORE = "BestScore";
    private const string KEY_TOTAL_APPLE = "TotalApple";
    private const string KEY_GIFT_CLOSE_TIME = "GiftCloseTime";
    private const string KEY_GIFT_TIME_REMAINING = "GiftTimeRemaining";
    private const string KEY_SELECTED_KNIFE_PAGE = "SelectedKnifePage"; // [MỚI] Trang dao đã chọn4/4
    private const string KEY_SELECTED_KNIFE_SLOT = "SelectedKnifeSlot"; // [MỚI] Ô dao đã chọn4/4

    // ── Cached values ──
    private int _bestStage;
    private int _bestScore;
    private int _totalApple;
    private int _selectedKnifePage; // [MỚI]4/4
    private int _selectedKnifeSlot; // [MỚI]4/4

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAll();
    }

    // ═══════════════════════════════════════════
    // LOAD
    // ═══════════════════════════════════════════
    private void LoadAll()
    {
        _bestStage = PlayerPrefs.GetInt(KEY_BEST_STAGE, 0);
        _bestScore = PlayerPrefs.GetInt(KEY_BEST_SCORE, 0);
        _totalApple = PlayerPrefs.GetInt(KEY_TOTAL_APPLE, 0);
        _selectedKnifePage = PlayerPrefs.GetInt(KEY_SELECTED_KNIFE_PAGE, 0); // [MỚI]4/4
        _selectedKnifeSlot = PlayerPrefs.GetInt(KEY_SELECTED_KNIFE_SLOT, 0); // [MỚI]4/4

        Debug.Log($"SaveManager: Loaded | " +
                  $"BestStage={_bestStage} | " +
                  $"BestScore={_bestScore} | " +
                  $"TotalApple={_totalApple} | " +
                  $"SelectedKnife=Page{_selectedKnifePage}_Slot{_selectedKnifeSlot}");
    }

    // ═══════════════════════════════════════════
    // SAVE — BEST STAGE
    // ═══════════════════════════════════════════

    /// <summary>
    /// Cập nhật stage cao nhất
    /// Chỉ lưu nếu stage mới cao hơn
    /// </summary>
    public void UpdateBestStage(int stage)
    {
        if (stage <= _bestStage) return;

        _bestStage = stage;
        PlayerPrefs.SetInt(KEY_BEST_STAGE, _bestStage);
        PlayerPrefs.Save();

        Debug.Log($"SaveManager: New Best Stage = {_bestStage}");
    }

    // ═══════════════════════════════════════════
    // SAVE — BEST SCORE
    // ═══════════════════════════════════════════

    /// <summary>
    /// Cập nhật số dao cao nhất
    /// Chỉ lưu nếu score mới cao hơn
    /// </summary>
    public void UpdateBestScore(int score)
    {
        if (score <= _bestScore) return;

        _bestScore = score;
        PlayerPrefs.SetInt(KEY_BEST_SCORE, _bestScore);
        PlayerPrefs.Save();

        Debug.Log($"SaveManager: New Best Score = {_bestScore}");
    }

    // ═══════════════════════════════════════════
    // SAVE — TOTAL APPLE
    // ═══════════════════════════════════════════

    /// <summary>
    /// Cộng dồn táo vào tổng
    /// Gọi mỗi khi nhặt được táo
    /// </summary>
    public void AddApple(int amount)
    {
        _totalApple += amount;
        PlayerPrefs.SetInt(KEY_TOTAL_APPLE, _totalApple);
        PlayerPrefs.Save();

        Debug.Log($"SaveManager: TotalApple = {_totalApple}");
    }

    // ═══════════════════════════════════════════
    // SAVE — SELECTED KNIFE [MỚI]
    // ═══════════════════════════════════════════

    /// <summary>
    /// Lưu dao đã chọn theo pageIndex và slotIndex.
    /// Gọi từ KnifeMenuManager khi người chơi chọn dao đã mở khóa.
    /// </summary>
    public void SaveSelectedKnife(int pageIndex, int slotIndex)
    {
        _selectedKnifePage = pageIndex;
        _selectedKnifeSlot = slotIndex;
        PlayerPrefs.SetInt(KEY_SELECTED_KNIFE_PAGE, _selectedKnifePage);
        PlayerPrefs.SetInt(KEY_SELECTED_KNIFE_SLOT, _selectedKnifeSlot);
        PlayerPrefs.Save();

        Debug.Log($"SaveManager: SelectedKnife saved → Page={pageIndex}, Slot={slotIndex}");
    }

    // ═══════════════════════════════════════════
    // SAVE — GIFT BOX TIMER
    // ═══════════════════════════════════════════
    public void SaveGiftTimer(float timeRemaining)
    {
        PlayerPrefs.SetString(KEY_GIFT_CLOSE_TIME,
            System.DateTime.UtcNow.ToString("o"));
        PlayerPrefs.SetFloat(KEY_GIFT_TIME_REMAINING, timeRemaining);
        PlayerPrefs.Save();
    }

    public float LoadGiftTimer(float defaultDuration)
    {
        string savedTime = PlayerPrefs.GetString(
            KEY_GIFT_CLOSE_TIME, "");

        if (string.IsNullOrEmpty(savedTime))
            return defaultDuration;

        System.DateTime closeTime =
            System.DateTime.Parse(savedTime,
                null,
                System.Globalization.DateTimeStyles
                      .RoundtripKind);

        float elapsed = (float)(System.DateTime.UtcNow
                               - closeTime).TotalSeconds;
        float saved = PlayerPrefs.GetFloat(
            KEY_GIFT_TIME_REMAINING, defaultDuration);

        return Mathf.Max(0f, saved - elapsed);
    }

    // ═══════════════════════════════════════════
    // GETTERS
    // ═══════════════════════════════════════════
    public int BestStage => _bestStage;
    public int BestScore => _bestScore;
    public int TotalApple => _totalApple;
    public int SelectedKnifePage => _selectedKnifePage; // [MỚI]4/4
    public int SelectedKnifeSlot => _selectedKnifeSlot; // [MỚI]4/4
}