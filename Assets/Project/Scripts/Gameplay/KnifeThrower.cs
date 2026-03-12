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
    [SerializeField] private int totalKnives = 7;
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
    public System.Action<int> OnKnifeCountChanged; // Số dao còn lại
    public System.Action OnAllKnivesStuck;    // Hết dao → qua màn
    public System.Action OnGameOver;          // Trúng dao → thua

    // ═══════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════
    //private void Start()
    //{
    //    _knivesRemaining = totalKnives;

    //    // Setup HUD queue ngay từ đầu
    //    if (HUDManager.Instance != null)
    //        HUDManager.Instance.SetupKnifeQueue(totalKnives);

    //    SpawnNextKnife();
    //}

    private void Update()
    {
        if (!_canThrow) return;
        if (_isGameOver) return;

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
        ClearAllKnives();

        totalKnives = knifeCount;
        _knivesRemaining = knifeCount;
        _isGameOver = false;
        _canThrow = false;

        // Reset HUD queue
        if (HUDManager.Instance != null)
            HUDManager.Instance.SetupKnifeQueue(knifeCount);

        SpawnNextKnife();
    }

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

        _canThrow = false;
        _currentKnife.Launch();

        // Cập nhật HUD icon dao
        if (HUDManager.Instance != null)
            HUDManager.Instance.OnKnifeThrown();
    }

    private void SpawnNextKnife()
    {
        if (_isGameOver) return;

        if (_knivesRemaining <= 0)
        {
            Debug.Log("KnifeThrower: All knives thrown! Stage Clear!");
            OnAllKnivesStuck?.Invoke();
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (_isGameOver) yield break;

        // Tạo dao mới tại SpawnPoint
        GameObject knifeObj = Instantiate(
            knifePrefab,
            spawnPoint.position,
            Quaternion.identity
        );

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

        // Cập nhật HUD số dao
        OnKnifeCountChanged?.Invoke(_knivesRemaining);

        Debug.Log($"KnifeThrower: Spawned knife. Remaining: {_knivesRemaining}");

        _canThrow = true;
    }

    private void HandleStuck()
    {
        if (_currentKnife != null)
            _stuckKnives.Add(_currentKnife.gameObject);

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
        Debug.LogWarning("KnifeThrower: Knife out of bounds, respawning...");
        _knivesRemaining++;
        SpawnNextKnife();
    }

    private void ClearAllKnives()
    {
        foreach (var knife in _stuckKnives)
            if (knife != null) Destroy(knife);
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
