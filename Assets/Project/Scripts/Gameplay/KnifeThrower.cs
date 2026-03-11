using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Quản lý việc ném dao:
/// Nhận input → Gọi Launch() → Spawn dao tiếp theo
/// Đếm số dao còn lại → Báo hết dao khi xong màn
/// </summary>
public class KnifeThrower : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // INSPECTOR
    // ═══════════════════════════════════════════
    [Header("── References ──")]
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("── Cấu hình màn ──")]
    [SerializeField] private int totalKnives = 5;
    [SerializeField] private float spawnDelay = 0.2f;

    // ═══════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════
    private int _knivesRemaining;
    private KnifeController _currentKnife;
    private bool _canThrow = false;
    private bool _isGameOver = false;
    private List<GameObject> _stuckKnives = new List<GameObject>();

    // ═══════════════════════════════════════════
    // EVENTS
    // ═══════════════════════════════════════════
    public System.Action<int> OnKnifeCountChanged;  // Số dao còn lại
    public System.Action OnAllKnivesStuck;     // Hết dao → qua màn
    public System.Action OnGameOver;           // Trúng dao → thua

    // ═══════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════
    private void Start()
    {
        _knivesRemaining = totalKnives;
        SpawnNextKnife();
    }

    private void Update()
    {
        if (!_canThrow) return;
        if (_isGameOver) return;

        // Input: Tap (mobile) hoặc Click chuột (editor)
        if (Input.GetMouseButtonDown(0))
        {
            TryThrow();
        }
    }

    // ═══════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Gọi từ LevelManager để setup màn mới
    /// </summary>
    public void SetupLevel(int knifeCount)
    {
        // Xóa dao cũ còn sót
        ClearAllKnives();

        totalKnives = knifeCount;
        _knivesRemaining = knifeCount;
        _isGameOver = false;
        _canThrow = false;

        SpawnNextKnife();
    }

    /// <summary>
    /// Cho phép hoặc khóa input
    /// Dùng khi hiện UI, pause game
    /// </summary>
    public void SetCanThrow(bool value)
    {
        _canThrow = value;
    }

    // ═══════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════

    private void TryThrow()
    {
        if (_currentKnife == null) return;
        if (!_currentKnife.IsWaiting) return;

        _canThrow = false;  // Khóa input cho đến khi spawn dao mới
        _currentKnife.Launch();
    }

    private void SpawnNextKnife()
    {
        if (_isGameOver) return;
        if (_knivesRemaining <= 0)
        {
            // Hết dao → Thắng màn
            Debug.Log("KnifeThrower: All knives thrown! Stage Clear!");
            OnAllKnivesStuck?.Invoke();
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        // Chờ một chút trước khi spawn dao mới
        yield return new WaitForSeconds(spawnDelay);

        if (_isGameOver) yield break;

        // Tạo dao mới tại SpawnPoint
        GameObject knifeObj = Instantiate(
            knifePrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        // Lấy KnifeController
        _currentKnife = knifeObj.GetComponent<KnifeController>();

        if (_currentKnife == null)
        {
            Debug.LogError("KnifeThrower: PRE_Knife thiếu KnifeController!");
            yield break;
        }

        // Đăng ký events
        _currentKnife.OnStuckInLog += HandleStuck;
        _currentKnife.OnHitKnife += HandleGameOver;
        _currentKnife.OnOutOfBounds += HandleOutOfBounds;

        // Trừ dao
        _knivesRemaining--;

        // Cập nhật HUD
        OnKnifeCountChanged?.Invoke(_knivesRemaining);

        Debug.Log($"KnifeThrower: Spawned knife. " +
                  $"Remaining: {_knivesRemaining}");

        // Cho phép ném
        _canThrow = true;
    }

    private void HandleStuck()
    {
        if (_currentKnife != null)
        {
            // Lưu dao đã cắm để xóa sau
            _stuckKnives.Add(_currentKnife.gameObject);
        }

        // Spawn dao tiếp theo
        SpawnNextKnife();
    }

    private void HandleGameOver()
    {
        if (_isGameOver) return;

        _isGameOver = true;
        _canThrow = false;

        Debug.Log("KnifeThrower: Game Over!");

        OnGameOver?.Invoke();
    }

    private void HandleOutOfBounds()
    {
        // Dao bay ra ngoài → Spawn lại
        Debug.LogWarning("KnifeThrower: Knife out of bounds, respawning...");
        _knivesRemaining++; // Hoàn lại dao
        SpawnNextKnife();
    }

    private void ClearAllKnives()
    {
        // Xóa tất cả dao cũ khi reset màn
        foreach (var knife in _stuckKnives)
        {
            if (knife != null)
                Destroy(knife);
        }
        _stuckKnives.Clear();

        if (_currentKnife != null)
        {
            Destroy(_currentKnife.gameObject);
            _currentKnife = null;
        }
    }

    // ═══════════════════════════════════════════
    // GETTERS
    // ═══════════════════════════════════════════
    public int KnivesRemaining => _knivesRemaining;
    public bool CanThrow => _canThrow;
    public bool IsGameOver => _isGameOver;
}