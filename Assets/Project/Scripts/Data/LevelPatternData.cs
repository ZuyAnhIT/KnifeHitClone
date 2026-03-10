using UnityEngine;

/// <summary>
/// ScriptableObject chứa dữ liệu pattern xoay cho từng level.
/// Tạo mới: Chuột phải → KnifeHit → LevelPattern
/// </summary>
[CreateAssetMenu(fileName = "Pattern_New",
                 menuName = "KnifeHit/LevelPattern")]
public class LevelPatternData : ScriptableObject
{
    [Header("─── Thông tin Level ───")]
    public int levelIndex = 1;
    public bool isBossStage = false;

    [Header("─── Dao cần ném ───")]
    [Min(1)]
    public int knifeCount = 5;
    [Min(0)]
    public int preplacedKnives = 0;

    [Header("─── Tốc độ xoay ───")]
    [Range(20f, 300f)]
    public float speed = 100f;
    public bool startClockwise = true;

    [Header("─── Đảo chiều ───")]
    public bool canReverse = false;
    [Range(0.5f, 10f)]
    public float reverseInterval = 2f;

    [Header("─── AnimationCurve ───")]
    [Tooltip("Trục X = thời gian (0→1), Trục Y = hệ số tốc độ (0→2)")]
    public AnimationCurve speedCurve =
        AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [Range(0.5f, 10f)]
    public float patternDuration = 2f;
    public bool loopPattern = true;
}