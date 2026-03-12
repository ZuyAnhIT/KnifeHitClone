using UnityEngine;

/// <summary>
/// Dữ liệu của 1 màn chơi
/// Được tạo tự động bởi LevelGenerator
/// </summary>
[System.Serializable]
public class LevelData
{
    [Header("── Thông tin màn ──")]
    public int stageNumber;       // Số thứ tự stage (1, 2, 3...)
    public int cycleNumber;       // Chu kỳ thứ mấy (1, 2, 3...)
    public int stageInCycle;      // Vị trí trong cycle (1-4 = thường, 5 = boss)
    public bool isBossStage;       // Có phải màn boss không

    [Header("── Dao ──")]
    public int knifeCount;        // Số dao cần phi
    public int preplacedCount;    // Số dao cắm sẵn trên Log

    [Header("── Táo ──")]
    public int appleCount;        // Số táo trên Log

    [Header("── Log ──")]
    public float logSpeed;          // Tốc độ xoay
    public bool canReverse;        // Có đảo chiều không
    public float reverseInterval;   // Bao lâu đảo 1 lần
    public int logSpriteIndex;    // Index sprite Log (random)

    public override string ToString()
    {
        return $"Stage {stageNumber} | " +
               $"Cycle {cycleNumber} | " +
               $"Boss={isBossStage} | " +
               $"Knives={knifeCount} | " +
               $"Preplaced={preplacedCount} | " +
               $"Apples={appleCount} | " +
               $"Speed={logSpeed}";
    }
}