using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Hộp quà đếm ngược ở màn Menu.
/// Tích hợp với SaveManager và ScoreManager có sẵn.
/// </summary>
public class GiftBoxController : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // CÀI ĐẶT — chỉnh tại đây
    // ═══════════════════════════════════════════
    [Header("── Cài đặt ──")]
    public float countdownDuration = 5f; // giây (5s)
    public int rewardAmount = 50;            // táo thưởng mỗi lần

    // ═══════════════════════════════════════════
    // REFERENCES
    // ═══════════════════════════════════════════
    [Header("── UI References ──")]
    public Button giftButton;
    public TextMeshProUGUI countdownText;
    public GameObject glowEffect;       // Image màu vàng nhấp nháy
    public Animator giftAnimator;       // Animator trên GiftIcon

    // ═══════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════
    private float _timeRemaining;
    private bool _isReady = false;
    private bool _isCollecting = false;

    // ═══════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════
    private void Start()
    {
        // Load thời gian còn lại từ lần thoát trước
        if (SaveManager.Instance != null)
            _timeRemaining = SaveManager.Instance
                .LoadGiftTimer(countdownDuration);
        else
            _timeRemaining = countdownDuration;

        giftButton.onClick.AddListener(OnGiftClicked);
        glowEffect.SetActive(false);

        // Nếu load về = 0 → đã sẵn sàng ngay
        if (_timeRemaining <= 0f)
            SetReady(true);
    }

    private void Update()
    {
        if (_isReady) return;

        _timeRemaining -= Time.deltaTime;

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            SetReady(true);
        }
        else
        {
            UpdateCountdownText();
        }
    }

    // Lưu timer khi thoát/pause app
    private void OnApplicationPause(bool paused)
    {
        if (paused && !_isReady && SaveManager.Instance != null)
            SaveManager.Instance.SaveGiftTimer(_timeRemaining);
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused) return;
        if (_isReady) return;
        if (SaveManager.Instance == null) return;

        _timeRemaining = SaveManager.Instance
            .LoadGiftTimer(countdownDuration);

        if (_timeRemaining <= 0f)
            SetReady(true);
    }

    private void OnDestroy()
    {
        // Lưu lại khi scene unload
        if (!_isReady && SaveManager.Instance != null)
            SaveManager.Instance.SaveGiftTimer(_timeRemaining);
    }

    // ═══════════════════════════════════════════
    // PRIVATE — LOGIC
    // ═══════════════════════════════════════════
    private void SetReady(bool ready)
    {
        _isReady = ready;
        glowEffect.SetActive(ready);
        giftButton.interactable = ready;

        if (ready)
        {
            countdownText.text = "Mở!";
            if (giftAnimator != null)
                giftAnimator.SetBool("IsReady", true);
        }
    }

    private void OnGiftClicked()
    {
        if (!_isReady || _isCollecting) return;
        StartCoroutine(CollectRoutine());
    }

    private IEnumerator CollectRoutine()
    {
        _isCollecting = true;

        // 1. Phát animation nhận thưởng
        if (giftAnimator != null)
        {
            giftAnimator.SetBool("IsReady", false);
            giftAnimator.SetTrigger("Collect");
        }

        // 2. Cộng táo vào hệ thống
        // ScoreManager cập nhật UI + SaveManager lưu tự động
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddAppleScore(rewardAmount);
        else if (SaveManager.Instance != null)
            SaveManager.Instance.AddApple(rewardAmount);

        // 3. Đợi animation chạy xong (~0.5s) rồi reset
        yield return new WaitForSeconds(0.5f);

        // 4. Reset đếm ngược
        _timeRemaining = countdownDuration;
        SetReady(false);
        _isCollecting = false;

        // 5. Lưu thời điểm bắt đầu đếm mới
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGiftTimer(_timeRemaining);
    }

    private void UpdateCountdownText()
    {
        int total = Mathf.CeilToInt(_timeRemaining);
        int h = total / 3600;
        int m = (total % 3600) / 60;
        int s = total % 60;

        countdownText.text = h > 0
            ? $"{h:D2}:{m:D2}:{s:D2}"
            : $"{m:D2}:{s:D2}";
    }
}