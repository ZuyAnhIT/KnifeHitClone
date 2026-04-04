using UnityEngine;
using UnityEngine.UI;
using TMPro; // BƯỚC MỚI: Thêm thư viện này để dùng TextMeshPro
using System.Collections;

public class BossTransitionUI : MonoBehaviour
{
    [Header("── UI References ──")]
    [SerializeField] private RectTransform leftKnife;
    [SerializeField] private RectTransform rightKnife;

    [Header("── Center Group (Chữ + Icon chéo) ──")]
    [SerializeField] private CanvasGroup centerGroup;
    [SerializeField] private GameObject clashIcon;

    // BƯỚC MỚI: Thêm biến này để đổi màu Text BOSS FIGHT!
    [Tooltip("Kéo text BOSS FIGHT! vào đây")]
    [SerializeField] private TextMeshProUGUI txtBossFight;

    [Header("── Cấu hình Đổi Màu (Theme) ──")]
    [Tooltip("Image của thanh kiếm bay lên bên trái")]
    [SerializeField] private Image leftSwordIcon;
    [Tooltip("Image của thanh kiếm bay lên bên phải")]
    [SerializeField] private Image rightSwordIcon;
    [Tooltip("Image của icon hai kiếm chéo nhau ở giữa (nếu có)")]
    [SerializeField] private Image clashIconImage;

    [Header("── Âm thanh ──")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clashSound;

    [Header("── Cấu hình Timing ──")]
    [SerializeField] private float slideUpTime = 0.3f;
    [SerializeField] private float hypeWaitTime = 1.2f;
    [SerializeField] private float slideDownTime = 0.4f;

    [Header("── Tọa độ Dao bay ──")]
    [SerializeField] private Vector2 leftStartPos = new Vector2(-400f, -800f);
    [SerializeField] private Vector2 rightStartPos = new Vector2(400f, -800f);
    [SerializeField] private Vector2 leftClashPos = new Vector2(-50f, -100f);
    [SerializeField] private Vector2 rightClashPos = new Vector2(50f, -100f);


    /// <summary>
    /// Hàm này được GameManager gọi để đổi màu kiếm trước khi chạy hiệu ứng
    /// </summary>
    public void SetThemeColor(Color color)
    {
        if (leftSwordIcon != null) leftSwordIcon.color = color;
        if (rightSwordIcon != null) rightSwordIcon.color = color;
        if (clashIconImage != null) clashIconImage.color = color;

        // BƯỚC MỚI: Đổi màu cho chữ BOSS FIGHT!
        if (txtBossFight != null) txtBossFight.color = color;
    }

    public void PlayTransition(System.Action onComplete)
    {
        // Kiểm tra an toàn: Nếu chưa kéo thả UI vào Inspector, bỏ qua hiệu ứng để game không kẹt
        if (leftKnife == null || rightKnife == null || centerGroup == null)
        {
            Debug.LogError("BossTransitionUI: BẠN CHƯA KÉO ĐỦ UI VÀO INSPECTOR! Đã bỏ qua hiệu ứng.");
            onComplete?.Invoke();
            return;
        }

        gameObject.SetActive(true);
        StartCoroutine(TransitionRoutine(onComplete));
    }

    private IEnumerator TransitionRoutine(System.Action onComplete)
    {
        // 1. SETUP
        centerGroup.alpha = 0f;
        if (clashIcon != null) clashIcon.SetActive(false);

        leftKnife.gameObject.SetActive(true);
        rightKnife.gameObject.SetActive(true);
        leftKnife.anchoredPosition = leftStartPos;
        rightKnife.anchoredPosition = rightStartPos;

        // 2. TRƯỢT LÊN
        float t = 0f;
        while (t < slideUpTime)
        {
            t += Time.deltaTime;
            float ease = Mathf.Sin((t / slideUpTime) * Mathf.PI * 0.5f);

            leftKnife.anchoredPosition = Vector2.Lerp(leftStartPos, leftClashPos, ease);
            rightKnife.anchoredPosition = Vector2.Lerp(rightStartPos, rightClashPos, ease);
            yield return null;
        }

        // 3. VA CHẠM (KENG!)
        leftKnife.anchoredPosition = leftClashPos;
        rightKnife.anchoredPosition = rightClashPos;

        if (audioSource != null && clashSound != null)
            audioSource.PlayOneShot(clashSound);

        leftKnife.gameObject.SetActive(false);
        rightKnife.gameObject.SetActive(false);

        if (clashIcon != null) clashIcon.SetActive(true);
        centerGroup.alpha = 1f;

        // 4. GIỮ TRÊN MÀN HÌNH
        yield return new WaitForSeconds(hypeWaitTime);

        // 5. TÁCH VÀ MỜ ĐI
        if (clashIcon != null) clashIcon.SetActive(false);
        leftKnife.gameObject.SetActive(true);
        rightKnife.gameObject.SetActive(true);

        t = 0f;
        while (t < slideDownTime)
        {
            t += Time.deltaTime;
            float ease = (t / slideDownTime) * (t / slideDownTime);

            centerGroup.alpha = Mathf.Lerp(1f, 0f, t / slideDownTime);

            leftKnife.anchoredPosition = Vector2.Lerp(leftClashPos, leftStartPos, ease);
            rightKnife.anchoredPosition = Vector2.Lerp(rightClashPos, rightStartPos, ease);
            yield return null;
        }

        // 6. HOÀN THÀNH - GỌI BOSS RA
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }
}