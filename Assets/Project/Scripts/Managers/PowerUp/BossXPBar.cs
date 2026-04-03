using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Thanh XP hiển thị tiến trình boss XP.
/// Gắn script này vào object chứa Slider/Image Fill
/// trên màn hình Boss Defeated hoặc màn Power-Ups.
/// </summary>
public class BossXPBar : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Slider hoặc Image (Fill) thể hiện thanh XP")]
    [SerializeField] private Slider xpSlider;

    [Tooltip("(Tuỳ chọn) Text hiển thị 'LEVEL X'")]
    [SerializeField] private TextMeshProUGUI txtLevel;

    [Header("Animation")]
    [Tooltip("Thời gian thanh XP chạy lên sau khi boss defeated (giây)")]
    [SerializeField] private float fillDuration = 0.8f;

    private Coroutine _fillCoroutine;

    // ═══════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════
    private void OnEnable()
    {
        // Đăng ký lắng nghe PowerUpManager
        if (PowerUpManager.Instance != null)
            PowerUpManager.Instance.OnXPChanged += HandleXPChanged;

        // Hiển thị ngay giá trị hiện tại
        RefreshImmediate();
    }

    private void OnDisable()
    {
        if (PowerUpManager.Instance != null)
            PowerUpManager.Instance.OnXPChanged -= HandleXPChanged;
    }

    // ═══════════════════════════════════════════
    // HANDLERS
    // ═══════════════════════════════════════════
    private void HandleXPChanged(int currentXP, int maxXP)
    {
        float targetFill = (float)currentXP / maxXP;

        // Nếu XP reset về 0 (vừa level up), chạy từ đầy về trống
        float startFill = xpSlider != null ? xpSlider.value : 0f;

        if (_fillCoroutine != null)
            StopCoroutine(_fillCoroutine);

        _fillCoroutine = StartCoroutine(
            AnimateFill(startFill, targetFill, maxXP));

        UpdateLevelText();
    }

    private void RefreshImmediate()
    {
        if (PowerUpManager.Instance == null) return;

        float fill = (float)PowerUpManager.Instance.CurrentXP
                     / PowerUpManager.Instance.XPPerLevel;

        if (xpSlider != null)
            xpSlider.value = fill;

        UpdateLevelText();
    }

    private void UpdateLevelText()
    {
        if (txtLevel == null || PowerUpManager.Instance == null) return;
        txtLevel.text = $"LEVEL {PowerUpManager.Instance.UnlockedCount + 1}";
    }

    // ═══════════════════════════════════════════
    // ANIMATE
    // ═══════════════════════════════════════════
    private IEnumerator AnimateFill(float from, float to, int maxXP)
    {
        if (xpSlider == null) yield break;

        // Nếu đang level up (to < from) → chạy đầy rồi reset
        bool isLevelUp = to < from;

        if (isLevelUp)
        {
            // Chạy từ vị trí hiện tại → đầy (1.0)
            float t = 0f;
            while (t < fillDuration)
            {
                t += Time.deltaTime;
                xpSlider.value = Mathf.Lerp(from, 1f, t / fillDuration);
                yield return null;
            }
            xpSlider.value = 1f;

            yield return new WaitForSeconds(0.2f);

            // Về 0
            xpSlider.value = 0f;

            // Chạy từ 0 → giá trị mới
            t = 0f;
            while (t < fillDuration * 0.5f)
            {
                t += Time.deltaTime;
                xpSlider.value = Mathf.Lerp(0f, to, t / (fillDuration * 0.5f));
                yield return null;
            }
        }
        else
        {
            // Chạy bình thường
            float t = 0f;
            while (t < fillDuration)
            {
                t += Time.deltaTime;
                xpSlider.value = Mathf.Lerp(from, to, t / fillDuration);
                yield return null;
            }
        }

        xpSlider.value = to;
    }
}