using UnityEngine;

/// <summary>
/// Tự động generate LevelData cho mỗi stage
/// Không cần vẽ tay từng màn!
/// 
/// Cấu trúc: mỗi CYCLE = 4 màn thường + 1 màn BOSS
/// Cycle tăng dần → độ khó tăng dần
/// </summary>
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
    [SerializeField] private int preplacedCycle1 = 0; // Cycle 1: không có dao sẵn
    [SerializeField] private int preplacedCycle2 = 1; // Cycle 2: 1 dao sẵn
    [SerializeField] private int preplacedMax = 4; // Tối đa 4 dao sẵn

    [Header("── Apple ──")]
    [SerializeField] private int appleMin = 1;
    [SerializeField] private int appleMax = 3;

    [Header("── Log Speed ──")]
    [SerializeField] private float baseSpeed = 80f;
    [SerializeField] private float speedIncrease = 15f;
    [SerializeField] private float speedMaxCap = 220f;

    [Header("── Boss Stage ──")]
    [SerializeField] private int bossKnifeMin = 10;
    [SerializeField] private int bossKnifeMax = 14;
    [SerializeField] private float bossSpeedMin = 150f;
    [SerializeField] private float bossSpeedMax = 200f;
    [SerializeField] private int bossLogCount = 4; // Số loại log boss

    // ═══════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════
    private int _currentStage = 0; // Stage đang chơi (1-based)

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
    /// Generate data cho stage tiếp theo
    /// Gọi mỗi khi cần load màn mới
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

        // Tính cycle và vị trí trong cycle
        // Mỗi cycle = 5 stage (4 thường + 1 boss)
        int cycleSize = 5;
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
    /// Reset về stage 1
    /// </summary>
    public void Reset()
    {
        _currentStage = 0;
    }

    public int CurrentStage => _currentStage;

    // ═══════════════════════════════════════════
    // PRIVATE — NORMAL STAGE
    // ═══════════════════════════════════════════
    private void GenerateNormalStage(LevelData data)
    {
        int cycle = data.cycleNumber;
        int pos = data.stageInCycle;

        // ── Knife Count ──
        int minKnife = baseKnifeMin
                      + (cycle - 1) * knifeIncreasePerCycle
                      + (pos - 1);
        int maxKnife = baseKnifeMax
                      + (cycle - 1) * knifeIncreasePerCycle
                      + (pos - 1);

        minKnife = Mathf.Min(minKnife, knifeMaxCap - 2);
        maxKnife = Mathf.Min(maxKnife, knifeMaxCap);
        data.knifeCount = Random.Range(minKnife, maxKnife + 1);

        // ── Stage 1 KHÔNG có táo và dao sẵn ──
        if (data.stageNumber == 1)
        {
            data.preplacedCount = 0;
            data.appleCount = 0;
        }
        else
        {
            // Stage 2+ random có thể có táo HOẶC dao HOẶC cả 2
            // 3 trường hợp random:
            // Case 1: Chỉ có táo
            // Case 2: Chỉ có dao sẵn
            // Case 3: Cả táo và dao sẵn
            int randomCase = Random.Range(0, 3);

            switch (randomCase)
            {
                case 0: // Chỉ táo
                    data.appleCount = Random.Range(appleMin, appleMax + 1);
                    data.preplacedCount = 0;
                    break;

                case 1: // Chỉ dao sẵn
                    data.appleCount = 0;
                    data.preplacedCount = cycle == 1 ? 0 :
                        Random.Range(1, Mathf.Min(cycle, preplacedMax) + 1);
                    break;

                case 2: // Cả táo và dao sẵn
                    data.appleCount = Random.Range(appleMin, appleMax + 1);
                    data.preplacedCount = cycle == 1 ? 0 :
                        Random.Range(1, Mathf.Min(cycle, preplacedMax) + 1);
                    break;
            }
        }

        // ── Log Speed ──
        float speed = baseSpeed + (cycle - 1) * speedIncrease
                    + Random.Range(-10f, 10f);
        data.logSpeed = Mathf.Clamp(speed, baseSpeed, speedMaxCap);

        // ── Can Reverse ──
        data.canReverse = cycle >= 2 && Random.value > 0.4f;
        data.reverseInterval = Random.Range(1.5f, 3.5f);

        // ── Log Sprite ──
        data.logSpriteIndex = Random.Range(0, 5);
    }

    // ═══════════════════════════════════════════
    // PRIVATE — BOSS STAGE
    // ═══════════════════════════════════════════
    private void GenerateBossStage(LevelData data)
    {
        // Boss: nhiều dao hơn, nhanh hơn, luôn đảo chiều
        data.knifeCount = Random.Range(bossKnifeMin,
                                            bossKnifeMax + 1);
        data.preplacedCount = Random.Range(2, 4);
        data.appleCount = Random.Range(1, 3);
        data.logSpeed = Random.Range(bossSpeedMin,
                                            bossSpeedMax);
        data.canReverse = true;
        data.reverseInterval = Random.Range(0.8f, 1.5f);

        // Boss dùng log đặc biệt (index 5+)
        data.logSpriteIndex = Random.Range(5,
                                            5 + bossLogCount);
    }
}