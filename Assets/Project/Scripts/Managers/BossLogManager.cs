using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý log Boss:
/// Random từ danh sách → Apply vào Log object
/// </summary>
public class BossLogManager : MonoBehaviour
{
    public static BossLogManager Instance
    { get; private set; }

    [Header("── Danh sách Boss Logs ──")]
    [SerializeField] private List<BossLogData> bossLogs;

    [Header("── References ──")]
    [SerializeField] private SpriteRenderer logRenderer;
    [SerializeField] private LogRotator logRotator;

    // Track boss đã dùng để không lặp
    private List<int> _usedIndices = new List<int>();
    private BossLogData _currentBoss;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ═══════════════════════════════════════════
    // PUBLIC
    // ═══════════════════════════════════════════

    /// <summary>
    /// Random boss log mới
    /// Không trùng boss vừa chơi
    /// </summary>
    public BossLogData GetRandomBoss()
    {
        if (bossLogs == null || bossLogs.Count == 0)
        {
            Debug.LogError("BossLogManager: Chưa có boss logs!");
            return null;
        }

        // Reset nếu đã dùng hết
        if (_usedIndices.Count >= bossLogs.Count)
            _usedIndices.Clear();

        // Tìm index chưa dùng
        int index;
        int maxTry = 20;
        do
        {
            index = Random.Range(0, bossLogs.Count);
            maxTry--;
        }
        while (_usedIndices.Contains(index)
               && maxTry > 0);

        _usedIndices.Add(index);
        _currentBoss = bossLogs[index];

        Debug.Log($"BossLog: {_currentBoss.bossName}");
        return _currentBoss;
    }

    /// <summary>
    /// Apply boss log vào scene
    /// Gọi từ GameManager khi bắt đầu boss stage
    /// </summary>
    public void ApplyBossLog(BossLogData data)
    {
        if (data == null) return;

        // Đổi sprite Log
        if (logRenderer != null && data.logSprite != null)
            logRenderer.sprite = data.logSprite;

        // Set rotation pattern
        if (logRotator != null)
        {
            // Tạo LevelData tạm cho boss
            LevelData bossLevel = new LevelData
            {
                logSpeed = data.logSpeed,
                canReverse = data.canReverse,
                reverseInterval = data.reverseInterval
            };
            logRotator.SetPattern(bossLevel);
        }
    }

    public BossLogData CurrentBoss => _currentBoss;
}