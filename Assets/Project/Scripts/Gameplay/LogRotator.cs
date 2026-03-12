using UnityEngine;

/// <summary>
/// Điều khiển xoay Log theo pattern từ LevelPatternData.
/// Gắn trực tiếp vào GameObject "Log" trong scene.
/// </summary>
public class LogRotator : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // INSPECTOR — Chỉnh trực tiếp để test
    // ═══════════════════════════════════════════
    [Header("─── Base Rotation ───")]
    [Range(20f, 300f)]
    [SerializeField] private float baseSpeed = 100f;
    [SerializeField] private bool clockwise = true;

    [Header("─── Animation Pattern ───")]
    [SerializeField]
    private AnimationCurve speedCurve
        = AnimationCurve.Linear(0f, 1f, 1f, 1f);
    [Range(0.5f, 10f)]
    [SerializeField] private float patternDuration = 2f;
    [SerializeField] private bool loopPattern = true;

    [Header("─── Reverse Settings ───")]
    [SerializeField] private bool canReverse = false;
    [Range(0.5f, 10f)]
    [SerializeField] private float reverseInterval = 2f;

    // ═══════════════════════════════════════════
    // PRIVATE VARIABLES
    // ═══════════════════════════════════════════
    private float _patternTimer = 0f;  // Timer cho curve
    private float _reverseTimer = 0f;  // Timer đảo chiều
    private float _currentDir = -1f; // -1 = CW, +1 = CCW
    private bool _isActive = true;

    // ═══════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════
    private void Start()
    {
        _currentDir = clockwise ? -1f : 1f;
    }

    private void Update()
    {
        if (!_isActive) return;

        // 1. Cập nhật timer pattern
        _patternTimer += Time.deltaTime;
        if (_patternTimer >= patternDuration)
        {
            _patternTimer = loopPattern ? 0f : patternDuration;
        }

        // 2. Lấy hệ số tốc độ từ curve
        float t = _patternTimer / patternDuration;
        float curveValue = speedCurve.Evaluate(t);

        // 3. Tính tốc độ thực tế
        float currentSpeed = baseSpeed * curveValue;

        // 4. Xoay Log
        transform.Rotate(
            0f,
            0f,
            _currentDir * currentSpeed * Time.deltaTime
        );

        // 5. Xử lý đảo chiều
        if (canReverse)
        {
            _reverseTimer += Time.deltaTime;
            if (_reverseTimer >= reverseInterval)
            {
                _currentDir *= -1f;
                _reverseTimer = 0f;
            }
        }
    }

    // ═══════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Gọi từ LevelManager để load pattern theo level
    /// </summary>
    public void SetPattern(LevelPatternData data)
    {
        if (data == null)
        {
            Debug.LogWarning("LogRotator: LevelPatternData is null!");
            return;
        }

        baseSpeed = data.speed;
        clockwise = data.startClockwise;
        canReverse = data.canReverse;
        reverseInterval = data.reverseInterval;
        speedCurve = data.speedCurve;
        patternDuration = data.patternDuration;
        loopPattern = data.loopPattern;

        // Reset timers
        _patternTimer = 0f;
        _reverseTimer = 0f;
        _currentDir = clockwise ? -1f : 1f;

        Debug.Log($"LogRotator: Pattern loaded — " +
                  $"Speed={baseSpeed} | " +
                  $"CW={clockwise} | " +
                  $"Reverse={canReverse}");
    }

    public void SetPattern(LevelData data)
    {
        baseSpeed = data.logSpeed;
        canReverse = data.canReverse;
        reverseInterval = data.reverseInterval;
        clockwise = Random.value > 0.5f; // Random chiều mỗi màn

        // Reset timers
        _patternTimer = 0f;
        _reverseTimer = 0f;
        _currentDir = clockwise ? -1f : 1f;

        Debug.Log($"LogRotator: Speed={baseSpeed} | " +
                  $"Reverse={canReverse} | " +
                  $"CW={clockwise}");
    }

    /// <summary>
    /// Dừng/tiếp tục xoay log (dùng khi Game Over hoặc Stage Complete)
    /// </summary>
    public void SetActive(bool active)
    {
        _isActive = active;

        if (!active)
        {
            // Dừng hoàn toàn khi inactive
            _patternTimer = 0f;
            _reverseTimer = 0f;
        }
    }

    /// <summary>
    /// Đảo chiều ngay lập tức (dùng cho effect đặc biệt)
    /// </summary>
    public void ForceReverse()
    {
        _currentDir *= -1f;
        _reverseTimer = 0f;
    }

    // ═══════════════════════════════════════════
    // GETTERS
    // ═══════════════════════════════════════════
    public bool IsActive => _isActive;
    public float CurrentSpeed => baseSpeed *
        speedCurve.Evaluate(_patternTimer / patternDuration);
    public bool IsClockwise => _currentDir < 0;
}