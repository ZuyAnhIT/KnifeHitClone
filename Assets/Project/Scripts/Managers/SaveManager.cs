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

    // ── Cached values ──
    private int _bestStage;
    private int _bestScore;
    private int _totalApple;

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

        Debug.Log($"SaveManager: Loaded | " +
                  $"BestStage={_bestStage} | " +
                  $"BestScore={_bestScore} | " +
                  $"TotalApple={_totalApple}");
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
    // GETTERS
    // ═══════════════════════════════════════════
    public int BestStage => _bestStage;
    public int BestScore => _bestScore;
    public int TotalApple => _totalApple;
}