using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Quản lý hệ thống XP và unlock Power-Up.
/// - Mỗi lần đánh bại boss: +1 XP (max xpPerLevel → level up → reset về 0)
/// - Mỗi khi đủ XP: mở khóa Power-Up tiếp theo
/// - Dữ liệu lưu qua PlayerPrefs
///
/// DontDestroyOnLoad — tồn tại suốt session.
/// Các PowerUpSlot tự đăng ký/hủy đăng ký khi scene load/unload.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    // ── Config ──
    [Header("XP Config")]
    [Tooltip("Số XP cần để level up và mở 1 Power-Up")]
    [SerializeField] private int xpPerLevel = 10;

    [Header("Tổng số Power-Up trong game")]
    [SerializeField] private int totalPowerUps = 6;

    // ── Save Keys ──
    private const string KEY_XP = "PowerUp_XP";
    private const string KEY_UNLOCKED = "PowerUp_UnlockedCount";

    // ── Runtime State ──
    private int _currentXP;
    private int _unlockedCount;

    /// <summary>
    /// Các slot UI hiện đang sống trong scene.
    /// Key = slotIndex (0-based), Value = PowerUpSlot component.
    /// Được cập nhật tự động khi slot Awake/OnDestroy.
    /// </summary>
    private readonly Dictionary<int, PowerUpSlot> _liveSlots
        = new Dictionary<int, PowerUpSlot>();

    // ── Events ──
    public System.Action<int, int> OnXPChanged;   // (currentXP, maxXP)
    public System.Action<int> OnPowerUpUnlocked; // (slotIndex)

    // ═══════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadData();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnhookGameManager();
    }

    // ═══════════════════════════════════════════
    // SLOT REGISTRATION — gọi bởi PowerUpSlot
    // ═══════════════════════════════════════════

    /// <summary>
    /// PowerUpSlot.Awake() gọi hàm này để đăng ký bản thân.
    /// Manager lập tức set đúng trạng thái dựa vào save data.
    /// </summary>
    public void RegisterSlot(int index, PowerUpSlot slot)
    {
        _liveSlots[index] = slot;

        // Set trạng thái ngay lập tức theo save data
        if (index < _unlockedCount)
            slot.Unlock();
        else
            slot.Lock();

        Debug.Log($"PowerUpManager: Slot[{index}] registered → " +
                  $"{(index < _unlockedCount ? "UNLOCKED" : "LOCKED")}");
    }

    /// <summary>
    /// PowerUpSlot.OnDestroy() gọi hàm này để hủy đăng ký.
    /// </summary>
    public void UnregisterSlot(int index)
    {
        _liveSlots.Remove(index);
    }

    // ═══════════════════════════════════════════
    // SCENE LOAD HOOK
    // ═══════════════════════════════════════════
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(HookGameManagerNextFrame());
    }

    private IEnumerator HookGameManagerNextFrame()
    {
        yield return null;
        UnhookGameManager();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStageClear += HandleStageClear;
            Debug.Log("PowerUpManager: Hooked GameManager.OnStageClear ✓");
        }
    }

    private void UnhookGameManager()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStageClear -= HandleStageClear;
    }

    // ═══════════════════════════════════════════
    // BOSS DEFEATED → TĂNG XP
    // ═══════════════════════════════════════════
    private void HandleStageClear()
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.CurrentLevel.isBossStage) return;

        AddXP(1);
    }

    public void AddXP(int amount)
    {
        _currentXP += amount;

        while (_currentXP >= xpPerLevel)
        {
            _currentXP -= xpPerLevel;
            UnlockNextPowerUp();
        }

        SaveData();
        OnXPChanged?.Invoke(_currentXP, xpPerLevel);

        Debug.Log($"PowerUpManager: XP={_currentXP}/{xpPerLevel}" +
                  $" | Unlocked={_unlockedCount}");
    }

    // ═══════════════════════════════════════════
    // UNLOCK
    // ═══════════════════════════════════════════
    private void UnlockNextPowerUp()
    {
        if (_unlockedCount >= totalPowerUps) return;

        int idx = _unlockedCount;
        _unlockedCount++;

        // Nếu slot đang sống trong scene → unlock ngay có animation
        if (_liveSlots.TryGetValue(idx, out PowerUpSlot slot) && slot != null)
            slot.Unlock(withAnimation: true);

        OnPowerUpUnlocked?.Invoke(idx);
        Debug.Log($"PowerUpManager: Power-Up #{idx + 1} UNLOCKED!");
    }

    // ═══════════════════════════════════════════
    // LOAD / SAVE
    // ═══════════════════════════════════════════
    private void LoadData()
    {
        _currentXP = PlayerPrefs.GetInt(KEY_XP, 0);
        _unlockedCount = PlayerPrefs.GetInt(KEY_UNLOCKED, 0);
        Debug.Log($"PowerUpManager: Loaded XP={_currentXP}" +
                  $" | Unlocked={_unlockedCount}");
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(KEY_XP, _currentXP);
        PlayerPrefs.SetInt(KEY_UNLOCKED, _unlockedCount);
        PlayerPrefs.Save();
    }

    // ═══════════════════════════════════════════
    // DEBUG / CHEAT — chỉnh save game
    // ═══════════════════════════════════════════

    /// <summary>
    /// Reset toàn bộ XP và Power-Up về ban đầu.
    /// Gọi từ Inspector button hoặc debug menu.
    /// </summary>
    [ContextMenu("DEBUG — Reset tất cả về 0")]
    public void DebugResetAll()
    {
        _currentXP = 0;
        _unlockedCount = 0;
        SaveData();
        RefreshAllLiveSlots();
        OnXPChanged?.Invoke(_currentXP, xpPerLevel);
        Debug.Log("PowerUpManager: DEBUG RESET — tất cả về 0");
    }

    /// <summary>
    /// Set thẳng số XP và số Power-Up đã unlock.
    /// Ví dụ: DebugSetState(3, 5) → đã mở 3 power-up, còn 5 XP.
    /// </summary>
    [ContextMenu("DEBUG — Set state mẫu (3 unlocked, 5 XP)")]
    public void DebugSetStateSample() => DebugSetState(3, 5);

    public void DebugSetState(int unlockedCount, int currentXP)
    {
        _unlockedCount = Mathf.Clamp(unlockedCount, 0, totalPowerUps);
        _currentXP = Mathf.Clamp(currentXP, 0, xpPerLevel - 1);
        SaveData();
        RefreshAllLiveSlots();
        OnXPChanged?.Invoke(_currentXP, xpPerLevel);
        Debug.Log($"PowerUpManager: DEBUG SET → " +
                  $"Unlocked={_unlockedCount} | XP={_currentXP}");
    }

    /// <summary>
    /// Thêm thẳng XP (có thể dùng để test nhanh).
    /// </summary>
    [ContextMenu("DEBUG — +1 XP ngay bây giờ")]
    public void DebugAddOneXP() => AddXP(1);

    // ═══════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════
    private void RefreshAllLiveSlots()
    {
        foreach (var kv in _liveSlots)
        {
            if (kv.Value == null) continue;
            if (kv.Key < _unlockedCount)
                kv.Value.Unlock();
            else
                kv.Value.Lock();
        }
    }

    // ── Getters ──
    public int CurrentXP => _currentXP;
    public int XPPerLevel => xpPerLevel;
    public int UnlockedCount => _unlockedCount;
}