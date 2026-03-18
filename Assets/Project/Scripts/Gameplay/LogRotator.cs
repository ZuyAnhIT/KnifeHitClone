using UnityEngine;

public class LogRotator : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // INSPECTOR
    // ═══════════════════════════════════════════
    [Header("── Tốc độ ──")]
    [SerializeField] private float baseSpeed = 80f;
    [SerializeField] private bool clockwise = true;

    [Header("── Đảo chiều ──")]
    [SerializeField] private bool canReverse = false;
    [SerializeField] private float reverseInterval = 2f;

    // ═══════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════
    private bool _isActive = true;
    private float _currentDir;        // +1 hoặc -1
    private float _currentSpeed;      // Tốc độ hiện tại
    private float _targetSpeed;       // Tốc độ mục tiêu

    // Đảo chiều
    private float _rotatedAngle = 0f; // Góc đã xoay trong lượt này
    private float _targetAngle = 360f; // Mỗi lượt xoay đủ 1 vòng
    private bool _isDecelerating = false; // Đang giảm tốc

    // Tốc độ lerp
    private const float ACCEL_SPEED = 120f; // Tăng tốc
    private const float DECEL_SPEED = 80f;  // Giảm tốc
    private const float MIN_SPEED = 5f;   // Tốc độ tối thiểu

    // ═══════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════
    private void Start()
    {
        _currentDir = clockwise ? -1f : 1f;
        _currentSpeed = baseSpeed;
        _targetSpeed = baseSpeed;
        _rotatedAngle = 0f;
    }

    private void Update()
    {
        if (!_isActive) return;

        if (canReverse)
            UpdateReverseRotation();
        else
            UpdateNormalRotation();
    }

    // ═══════════════════════════════════════════
    // ROTATION MODES
    // ═══════════════════════════════════════════

    /// Quay bình thường — không đảo chiều
    private void UpdateNormalRotation()
    {
        // Lerp tốc độ về target
        _currentSpeed = Mathf.MoveTowards(
            _currentSpeed,
            _targetSpeed,
            ACCEL_SPEED * Time.deltaTime
        );

        transform.Rotate(
            0f, 0f,
            _currentDir * _currentSpeed * Time.deltaTime
        );
    }

    /// Quay đảo chiều — mỗi lượt xoay đủ 1 vòng rồi đổi chiều
    private void UpdateReverseRotation()
    {
        float deltaAngle = _currentSpeed * Time.deltaTime;

        if (!_isDecelerating)
        {
            // Đang tăng/giữ tốc
            _currentSpeed = Mathf.MoveTowards(
                _currentSpeed,
                baseSpeed,
                ACCEL_SPEED * Time.deltaTime
            );

            // Kiểm tra còn bao nhiêu góc để xoay
            float remaining = _targetAngle - _rotatedAngle;

            // Tính quãng đường cần để dừng (v²/2a)
            float stopDistance = (_currentSpeed * _currentSpeed)
                                 / (2f * DECEL_SPEED);

            // Bắt đầu giảm tốc khi sắp đủ vòng
            if (remaining <= stopDistance + 5f)
                _isDecelerating = true;
        }
        else
        {
            // Đang giảm tốc
            _currentSpeed = Mathf.MoveTowards(
                _currentSpeed,
                MIN_SPEED,
                DECEL_SPEED * Time.deltaTime
            );
        }

        // Xoay log
        deltaAngle = _currentSpeed * Time.deltaTime;
        transform.Rotate(0f, 0f,
            _currentDir * deltaAngle);
        _rotatedAngle += deltaAngle;

        // Đã xoay đủ vòng → Đảo chiều
        if (_rotatedAngle >= _targetAngle
            && _currentSpeed <= MIN_SPEED + 1f)
        {
            SwitchDirection();
        }
    }

    // ═══════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════
    private void SwitchDirection()
    {
        _currentDir *= -1f;          // Đảo chiều
        _rotatedAngle = 0f;           // Reset góc
        _isDecelerating = false;        // Reset giảm tốc
        _currentSpeed = MIN_SPEED;    // Bắt đầu từ tốc độ thấp

        // Random target angle 360~540 độ để không đều đặn
        _targetAngle = Random.Range(360f, 540f);

        Debug.Log($"LogRotator: Switched! " +
                  $"Dir={_currentDir} | " +
                  $"Target={_targetAngle:F0}°");
    }

    // ═══════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════
    public void SetPattern(LevelData data)
    {
        baseSpeed = data.logSpeed;
        canReverse = data.canReverse;
        reverseInterval = data.reverseInterval;
        clockwise = Random.value > 0.5f;

        // Reset
        _currentDir = clockwise ? -1f : 1f;
        _currentSpeed = canReverse ? MIN_SPEED : baseSpeed;
        _targetSpeed = baseSpeed;
        _rotatedAngle = 0f;
        _isDecelerating = false;
        _targetAngle = Random.Range(360f, 540f);

        Debug.Log($"LogRotator: Pattern set | " +
                  $"Speed={baseSpeed} | " +
                  $"Reverse={canReverse} | " +
                  $"CW={clockwise}");
    }

    public void SetPattern(LevelPatternData data)
    {
        if (data == null) return;

        baseSpeed = data.speed;
        clockwise = data.startClockwise;
        canReverse = data.canReverse;
        reverseInterval = data.reverseInterval;

        _currentDir = clockwise ? -1f : 1f;
        _currentSpeed = canReverse ? MIN_SPEED : baseSpeed;
        _targetSpeed = baseSpeed;
        _rotatedAngle = 0f;
        _isDecelerating = false;
        _targetAngle = Random.Range(360f, 540f);
    }

    public void SetActive(bool active)
    {
        _isActive = active;
        if (!active)
        {
            _currentSpeed = 0f;
            _rotatedAngle = 0f;
        }
    }

    public void ForceReverse()
    {
        SwitchDirection();
    }

    // ═══════════════════════════════════════════
    // GETTERS
    // ═══════════════════════════════════════════
    public bool IsActive => _isActive;
    public bool IsClockwise => _currentDir < 0;
    public float CurrentSpeed => _currentSpeed;
}