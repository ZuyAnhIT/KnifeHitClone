using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("── HUD References ──")]
    [SerializeField] private TextMeshProUGUI txtAppleCount;
    [SerializeField] private TextMeshProUGUI txtKnifeThrown;

    private int _appleScore = 0;
    private int _knifeThrown = 0;

    public System.Action<int> OnAppleScoreChanged;
    public System.Action<int> OnKnifeThrownChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Load tổng táo từ SaveManager ngay khi vào game
        LoadAppleFromSave();
    }

    // ═══════════════════════════════════════════
    // LOAD
    // ═══════════════════════════════════════════
    private void LoadAppleFromSave()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("ScoreManager: SaveManager chưa sẵn sàng!");
            return;
        }

        _appleScore = SaveManager.Instance.TotalApple;

        if (txtAppleCount != null)
            txtAppleCount.text = _appleScore.ToString();

        Debug.Log($"ScoreManager: Loaded apple = {_appleScore}");
    }

    // ═══════════════════════════════════════════
    // ADD
    // ═══════════════════════════════════════════
    public void AddKnifeThrown()
    {
        _knifeThrown++;

        if (txtKnifeThrown != null)
            txtKnifeThrown.text = _knifeThrown.ToString();

        OnKnifeThrownChanged?.Invoke(_knifeThrown);
    }

    public void AddAppleScore(int value)
    {
        _appleScore += value;

        if (txtAppleCount != null)
            txtAppleCount.text = _appleScore.ToString();

        // Lưu vào SaveManager → Tích lũy mãi mãi
        if (SaveManager.Instance != null)
            SaveManager.Instance.AddApple(value);

        OnAppleScoreChanged?.Invoke(_appleScore);
    }

    // ═══════════════════════════════════════════
    // RESET — Chỉ reset KnifeThrown khi Game Over
    // ═══════════════════════════════════════════
    public void ResetScore()
    {
        // Chỉ reset dao
        _knifeThrown = 0;
        if (txtKnifeThrown != null)
            txtKnifeThrown.text = "0";

        // Reload táo từ SaveManager
        // Đảm bảo hiện đúng tổng sau khi restart
        LoadAppleFromSave();

        Debug.Log("ScoreManager: Reset knife, reload apple");
    }

    // ═══════════════════════════════════════════
    // GETTERS
    // ═══════════════════════════════════════════
    public int AppleScore => _appleScore;
    public int KnifeThrown => _knifeThrown;
}