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
    public TextMeshProUGUI txtCurrency;

    [Header("── Effect ──")]
    [SerializeField] private GiftRewardEffect rewardEffect;
    // ═══════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════
    private float _timeRemaining;
    private bool _isReady = false;
    private bool _isCollecting = false;
    private int currentCurrency = 0;

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
        if (SaveManager.Instance != null)
        {
            currentCurrency = SaveManager.Instance.TotalApple;

            if (txtCurrency != null)
                txtCurrency.text = currentCurrency.ToString();
        }
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

        // 1. Play animation
        if (giftAnimator != null)
        {
            giftAnimator.SetBool("IsReady", false);
            giftAnimator.SetTrigger("Collect");
        }


        // ── Lấy tâm màn hình trong hệ tọa độ Canvas ──
        Vector2 spawnPos = Vector2.zero; // anchoredPosition (0,0) = tâm Canvas

        // Nếu Canvas dùng Screen Space - Camera hoặc World Space
        // thì dùng đoạn dưới thay thế:
        // RectTransform canvasRT = rewardEffect.GetComponent<RectTransform>()
        //     .root.GetComponent<RectTransform>();
        // spawnPos = Vector2.zero; // vẫn là (0,0) với pivot = 0.5

        // Phát hiệu ứng táo bay ra
        if (rewardEffect != null)
            rewardEffect.Play(rewardAmount, spawnPos);

        // Cộng táo
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddAppleScore(rewardAmount);
        }
           
        else if (SaveManager.Instance != null)
        // 2. Lưu vào SaveManager
        {
            SaveManager.Instance.AddApple(rewardAmount);
            currentCurrency = SaveManager.Instance.TotalApple;
        }

        // 3. Cập nhật UI ngay lập tức
        if (txtCurrency != null)
            txtCurrency.text = currentCurrency.ToString();

        // 4. Đợi animation
        yield return new WaitForSeconds(0.5f);

        // 5. Reset countdown
        _timeRemaining = countdownDuration;
        SetReady(false);
        _isCollecting = false;

        // 6. Lưu timer
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