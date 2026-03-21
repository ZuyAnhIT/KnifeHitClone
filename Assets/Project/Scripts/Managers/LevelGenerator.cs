using UnityEngine;


public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance { get; private set; }

    // ═══════════════════════════════════════════
    // CẤU HÌNH ĐỘ KHÓ
    // ═══════════════════════════════════════════
    [Header("── Knife Count ──")]
    [SerializeField] private int baseKnifeMin = 5;
    [SerializeField] private int baseKnifeMax = 8;
    [SerializeField] private int knifeIncreasePerCycle = 1;
    [SerializeField] private int knifeMaxCap = 15;

    [Header("── Preplaced Knives ──")]
    [SerializeField] private int preplacedMax = 4;

    [Header("── Apple ──")]
    [SerializeField] private int appleMin = 1;
    [SerializeField] private int appleMax = 3;

    [Header("── Log Speed ──")]
    [SerializeField] private float stage1Speed = 80f;
    [SerializeField] private float baseSpeed = 80f;
    [SerializeField] private float speedIncrease = 15f;
    [SerializeField] private float speedMaxCap = 220f;

    [Header("── Boss Stage ──")]
    [SerializeField] private int bossKnifeMin = 10;
    [SerializeField] private int bossKnifeMax = 14;
    [SerializeField] private float bossSpeedMin = 150f;
    [SerializeField] private float bossSpeedMax = 200f;

    // ═══════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════
    private int _currentStage = 0;

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
    /// Tăng stage lên 1 và generate data
    /// KHÔNG reset — stage tăng liên tục qua boss
    /// Chỉ Reset() khi Game Over
    /// </summary>
    public LevelData GenerateNext()
    {
        _currentStage++;
        return GenerateForStage(_currentStage);
    }

    /// <summary>
    /// Generate data cho stage cụ thể
    /// </summary>
    public LevelData GenerateForStage(int stageNumber)
    {
        LevelData data = new LevelData();
        int cycleSize = 5; // 4 thường + 1 boss

        data.stageNumber = stageNumber;
        data.cycleNumber = ((stageNumber - 1) / cycleSize) + 1;
        data.stageInCycle = ((stageNumber - 1) % cycleSize) + 1;
        data.isBossStage = data.stageInCycle == cycleSize;

        if (data.isBossStage)
            GenerateBossStage(data);
        else
            GenerateNormalStage(data);

        Debug.Log($"LevelGenerator: {data}");
        return data;
    }

    /// <summary>
    /// Chỉ gọi khi Game Over
    /// Reset về stage 1
    /// </summary>
    public void Reset()
    {
        _currentStage = 0;
        Debug.Log("LevelGenerator: Reset to Stage 1");
    }

    public int CurrentStage => _currentStage;

    // ═══════════════════════════════════════════
    // PRIVATE — NORMAL STAGE
    // ═══════════════════════════════════════════
    private void GenerateNormalStage(LevelData data)
    {
        int stageNum = data.stageNumber;
        int cycle = data.cycleNumber;
        int pos = data.stageInCycle;

        // ════════════════════════════════
        // STAGE 1 — Đặc biệt, dễ nhất
        // ════════════════════════════════
        if (stageNum == 1)
        {
            data.knifeCount = Random.Range(
                                       baseKnifeMin,
                                       baseKnifeMax + 1);
            data.preplacedCount = 0;
            data.appleCount = 0;
            data.logSpeed = stage1Speed;
            data.canReverse = false;
            data.reverseInterval = 0f;
            data.logSpriteIndex = 0;
            return;
        }

        // ════════════════════════════════
        // STAGE 2+ — Random logic
        // ════════════════════════════════

        // ── Knife Count ──
        // Tăng dần theo cycle và vị trí trong cycle
        int minKnife = baseKnifeMin
                      + (cycle - 1) * knifeIncreasePerCycle
                      + (pos - 1);
        int maxKnife = baseKnifeMax
                      + (cycle - 1) * knifeIncreasePerCycle
                      + (pos - 1);

        minKnife = Mathf.Min(minKnife, knifeMaxCap - 2);
        maxKnife = Mathf.Min(maxKnife, knifeMaxCap);
        data.knifeCount = Random.Range(minKnife, maxKnife + 1);

        // ── Items trên Log ──
        // Case 0: Chỉ táo
        // Case 1: Chỉ dao sẵn
        // Case 2: Cả táo và dao sẵn
        int randomCase = Random.Range(0, 3);
        switch (randomCase)
        {
            case 0:
                data.appleCount = Random.Range(
                                          appleMin,
                                          appleMax + 1);
                data.preplacedCount = 0;
                break;

            case 1:
                data.appleCount = 0;
                data.preplacedCount = Random.Range(1,
                                          Mathf.Min(
                                              cycle + 1,
                                              preplacedMax) + 1);
                break;

            case 2:
                data.appleCount = Random.Range(
                                          appleMin,
                                          appleMax + 1);
                data.preplacedCount = Random.Range(1,
                                          Mathf.Min(
                                              cycle + 1,
                                              preplacedMax) + 1);
                break;
        }

        // ── Log Speed ──
        // Tăng nhẹ theo stage + random nhỏ
        float speed = baseSpeed
                         + (stageNum - 2) * (speedIncrease / 4f)
                         + Random.Range(-8f, 8f);
        data.logSpeed = Mathf.Clamp(
                              speed, baseSpeed, speedMaxCap);

        // ── Rotation Logic ──
        // Type 0: Quay bình thường
        // Type 1: Quay đảo chiều
        // Type 2: Quay nhanh hơn
        int rotationType = Random.Range(0, 3);
        switch (rotationType)
        {
            case 0:
                data.canReverse = false;
                data.reverseInterval = 0f;
                data.logSpeed = Mathf.Clamp(
                                           data.logSpeed,
                                           baseSpeed,
                                           baseSpeed + 20f);
                break;

            case 1:
                data.canReverse = true;
                data.reverseInterval = Random.Range(1.5f, 3.0f);
                break;

            case 2:
                data.canReverse = false;
                data.reverseInterval = 0f;
                data.logSpeed = Mathf.Clamp(
                                           data.logSpeed + 30f,
                                           baseSpeed + 30f,
                                           speedMaxCap);
                break;
        }

        // ── Log Sprite ──
        data.logSpriteIndex = Random.Range(0, 5);

        Debug.Log($"Stage {stageNum} | " +
                  $"Cycle={cycle} | " +
                  $"RotType={rotationType} | " +
                  $"Speed={data.logSpeed:F0} | " +
                  $"Reverse={data.canReverse} | " +
                  $"Knives={data.knifeCount} | " +
                  $"Preplaced={data.preplacedCount} | " +
                  $"Apples={data.appleCount}");
    }

    // ═══════════════════════════════════════════
    // PRIVATE — BOSS STAGE
    // ═══════════════════════════════════════════
    private void GenerateBossStage(LevelData data)
    {
        // Boss: nhiều dao hơn, nhanh hơn, đảo chiều
        // Sprite do BossLogManager quản lý riêng
        data.knifeCount = Random.Range(
                                   bossKnifeMin,
                                   bossKnifeMax + 1);
        data.preplacedCount = Random.Range(0, 3);
        data.appleCount = Random.Range(1, 3);
        data.logSpeed = Random.Range(
                                   bossSpeedMin,
                                   bossSpeedMax);
        data.canReverse = true;
        data.reverseInterval = Random.Range(0.8f, 1.5f);
        data.logSpriteIndex = -1; // BossLogManager tự xử lý

        Debug.Log($"BOSS Stage {data.stageNumber} | " +
                  $"Cycle={data.cycleNumber} | " +
                  $"Speed={data.logSpeed:F0} | " +
                  $"Knives={data.knifeCount} | " +
                  $"Preplaced={data.preplacedCount} | " +
                  $"Apples={data.appleCount}");
    }
}