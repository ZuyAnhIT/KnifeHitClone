using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Đại diện cho 1 ô Power-Up trong danh sách.
/// Tự đăng ký vào PowerUpManager khi được tạo ra (Awake),
/// và tự hủy đăng ký khi bị destroy (scene reload).
/// → PowerUpManager không cần giữ reference cứng nữa.
/// </summary>
public class PowerUpSlot : MonoBehaviour
{
    [Header("Vị trí trong danh sách (0 = đầu tiên)")]
    [SerializeField] private int slotIndex = 0;

    [Header("Trạng thái khóa")]
    [SerializeField] private GameObject lockedVisual;
    [SerializeField] private TextMeshProUGUI lockedLabel;

    [Header("Trạng thái mở")]
    [SerializeField] private GameObject unlockedVisual;
    [SerializeField] private TextMeshProUGUI unlockedLabel;

    [Header("Hiệu ứng mở khóa (tuỳ chọn)")]
    [SerializeField] private Animator unlockAnimator;
    [SerializeField] private string unlockTriggerName = "Unlock";

    private bool _isUnlocked = false;

    // ═══════════════════════════════════════════
    // LIFECYCLE — tự đăng ký / hủy đăng ký
    // ═══════════════════════════════════════════
    private void Awake()
    {
        // Đăng ký vào Manager ngay khi object được tạo
        // Manager sẽ gọi Unlock() hoặc Lock() để set đúng trạng thái
        PowerUpManager.Instance?.RegisterSlot(slotIndex, this);
    }

    private void OnDestroy()
    {
        PowerUpManager.Instance?.UnregisterSlot(slotIndex);
    }

    // ═══════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════
    public void Unlock(bool withAnimation = false)
    {
        _isUnlocked = true;

        if (lockedVisual != null) lockedVisual.SetActive(false);
        if (unlockedVisual != null) unlockedVisual.SetActive(true);
        if (lockedLabel != null) lockedLabel.gameObject.SetActive(false);
        if (unlockedLabel != null) unlockedLabel.gameObject.SetActive(true);

        if (withAnimation && unlockAnimator != null)
            unlockAnimator.SetTrigger(unlockTriggerName);

        Debug.Log($"PowerUpSlot[{slotIndex}] {gameObject.name}: UNLOCKED");
    }

    public void Lock()
    {
        _isUnlocked = false;

        if (lockedVisual != null) lockedVisual.SetActive(true);
        if (unlockedVisual != null) unlockedVisual.SetActive(false);
        if (lockedLabel != null) lockedLabel.gameObject.SetActive(true);
        if (unlockedLabel != null) unlockedLabel.gameObject.SetActive(false);
    }

    public bool IsUnlocked => _isUnlocked;
    public int SlotIndex => slotIndex;
}