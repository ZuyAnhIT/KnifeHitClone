using UnityEngine;
using TMPro;

/// <summary>
/// Quản lý điểm số toàn game
/// Singleton — gọi từ bất kỳ đâu
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // ── SINGLETON ───────────────────────────
    public static ScoreManager Instance { get; private set; }

    // ── INSPECTOR ───────────────────────────
    [Header("── HUD References ──")]
    [SerializeField] private TextMeshProUGUI txtAppleCount;
    [SerializeField] private TextMeshProUGUI txtKnifeThrown;

    // ── PRIVATE ─────────────────────────────
    private int _appleScore = 0;
    private int _knifeThrown = 0;

    // ── EVENTS ──────────────────────────────
    public System.Action<int> OnAppleScoreChanged;
    public System.Action<int> OnKnifeThrownChanged;

    // ── UNITY LIFECYCLE ─────────────────────
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ── PUBLIC ──────────────────────────────

    /// <summary>
    /// Gọi khi dao cắm vào Log thành công
    /// </summary>
    public void AddKnifeThrown()
    {
        _knifeThrown++;

        // Cập nhật HUD
        if (txtKnifeThrown != null)
            txtKnifeThrown.text = _knifeThrown.ToString();

        OnKnifeThrownChanged?.Invoke(_knifeThrown);

        Debug.Log($"Score: Knife thrown = {_knifeThrown}");
    }

    /// <summary>
    /// Gọi khi thu thập táo
    /// </summary>
    public void AddAppleScore(int value)
    {
        _appleScore += value;

        // Cập nhật HUD
        if (txtAppleCount != null)
            txtAppleCount.text = _appleScore.ToString();

        OnAppleScoreChanged?.Invoke(_appleScore);

        Debug.Log($"Score: Apple score = {_appleScore}");
    }

    /// <summary>
    /// Reset điểm khi bắt đầu màn mới
    /// </summary>
    public void ResetScore()
    {
        _appleScore = 0;
        _knifeThrown = 0;

        if (txtAppleCount != null)
            txtAppleCount.text = "0";
        if (txtKnifeThrown != null)
            txtKnifeThrown.text = "0";
    }

    // ── GETTERS ─────────────────────────────
    public int AppleScore => _appleScore;
    public int KnifeThrown => _knifeThrown;
}