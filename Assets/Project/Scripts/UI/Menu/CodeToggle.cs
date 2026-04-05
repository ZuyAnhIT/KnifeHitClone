using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// =====================================================================
/// CodeToggle.cs — Toggle UI có animation trượt mượt
/// =====================================================================
/// CÁCH HOẠT ĐỘNG TỔNG QUAN:
///
///   1. Start() → đọc trạng thái đã lưu từ SaveManager
///              → set vị trí/màu ngay lập tức (không animation)
///              → áp dụng hiệu ứng thực tế (tắt tiếng, v.v.)
///
///   2. Bấm nút → OnToggleClicked()
///              → lật isON (true ↔ false)
///              → phát tiếng click (UIButtonSound)
///              → lưu vào SaveManager / PlayerPrefs
///              → áp dụng hiệu ứng thực tế
///              → chạy coroutine animation trượt mượt
///
///   3. ToggleType quyết định toggle này làm gì:
///        Sound     → AudioListener.volume (0 hoặc 1)
///        Vibration → lưu cờ, script khác kiểm tra trước khi Handheld.Vibrate()
///        LeftHand  → bắn event tĩnh OnLeftHandChanged
///
/// CÁCH GẮN VÀO UNITY:
///   - Gắn script này lên Button object của từng toggle
///   - Kéo đúng các thành phần UI vào Inspector
///   - Chọn ToggleType khớp với chức năng của toggle đó
///   - (Tùy chọn) Kéo UIButtonSound vào slot buttonSound để có tiếng click
/// =====================================================================
/// </summary>
public class CodeToggle : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════
    // ENUM — Loại toggle
    // ═══════════════════════════════════════════════════════════════
    // Mỗi toggle trong Settings cần chọn đúng loại này trong Inspector.
    // Nhờ đó 1 script duy nhất xử lý được cả 3 toggle khác nhau.
    public enum ToggleType
    {
        Sound,      // Toggle SOUNDS    → điều khiển âm lượng toàn game
        Vibration,  // Toggle VIBRATION → điều khiển có rung hay không
        LeftHand    // Toggle LEFT HAND → đổi vùng chạm sang tay trái/phải
    }

    // ═══════════════════════════════════════════════════════════════
    // INSPECTOR
    // ═══════════════════════════════════════════════════════════════
    [Header("Thành phần UI")]
    public RectTransform handleRect;  // Cục tròn trượt  → kéo Img_Handle vào
    public Image backgroundImage;     // Nền của toggle  → kéo Img_Background vào
    public Image txtON;               // Chữ/ảnh "ON"    → kéo Txt_ON vào
    public Image txtOFF;              // Chữ/ảnh "OFF"   → kéo Txt_OFF vào

    [Header("Cài đặt Vị trí Cục tròn (Trục X)")]
    public float posX_ON = 55f;     // Tọa độ X khi BẬT  (trượt sang phải)
    public float posX_OFF = -55f;     // Tọa độ X khi TẮT  (trượt sang trái)

    [Header("Cài đặt Màu Nền")]
    public Color colorON = new Color(0.2f, 0.8f, 0.2f, 1f); // Xanh lá = đang bật
    public Color colorOFF = new Color(0.6f, 0.6f, 0.6f, 1f); // Xám     = đang tắt

    [Header("Loại Toggle — chọn đúng chức năng trong Inspector")]
    public ToggleType toggleType = ToggleType.Sound;

    [Header("Trạng thái mặc định (chỉ dùng lần đầu, khi chưa có dữ liệu lưu)")]
    public bool defaultIsON = false;

    // ── Âm thanh click ────────────────────────────────────────────
    // Kéo UIButtonSound từ bất kỳ GameObject nào vào đây.
    // Nếu để trống thì toggle vẫn hoạt động, chỉ không có tiếng click.
    //
    // LƯU Ý ĐẶC BIỆT với toggle Sound:
    //   Tiếng click sẽ vẫn phát dù Sound đang bị TẮT.
    //   Lý do: người dùng cần nghe phản hồi khi họ BẬT lại âm thanh.
    //   AudioSource của UIButtonSound nên được gắn vào một object riêng
    //   không bị ảnh hưởng bởi AudioListener.volume = 0,
    //   HOẶC chấp nhận rằng tiếng click sẽ mất khi Sound OFF (đơn giản hơn).
    [Header("Âm thanh click (tùy chọn)")]
    [Tooltip("Kéo component UIButtonSound vào đây để có tiếng click khi bấm toggle")]
    public UIButtonSound buttonSound;

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════════════════════════
    // isON: đọc từ ngoài được (public get), nhưng chỉ ghi được từ bên trong
    public bool isON { get; private set; }
    private Button myButton;
    private Coroutine slideCoroutine; // Giữ reference để dừng nếu bấm liên tục

    // ═══════════════════════════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════════════════════════
    void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnToggleClicked);

        // B1: Đọc trạng thái đã lưu từ PlayerPrefs (qua SaveManager)
        //     Nếu chưa có dữ liệu lưu → dùng giá trị defaultIsON trong Inspector
        isON = LoadState();

        // B2: Set ngay vị trí và màu KHÔNG có animation
        //     → Tránh thấy toggle "trượt" khi mới mở màn hình Settings
        SetStateImmediate(isON);

        // B3: Áp dụng hiệu ứng thực tế ngay khi vào scene
        //     → Ví dụ: Sound đang OFF thì tắt tiếng ngay từ đầu
        ApplyEffect(isON);
    }

    // ═══════════════════════════════════════════════════════════════
    // XỬ LÝ KHI BẤM TOGGLE
    // ═══════════════════════════════════════════════════════════════
    public void OnToggleClicked()
    {
        // Lật trạng thái: đang ON → OFF, đang OFF → ON
        isON = !isON;

        // Phát tiếng click qua UIButtonSound (nếu có gắn)
        if (buttonSound != null)
            buttonSound.PlayClick();

        // Lưu trạng thái mới → PlayerPrefs (qua SaveManager)
        // Dữ liệu tồn tại sau khi tắt app, sang scene khác
        SaveState(isON);

        // Áp dụng hiệu ứng thực tế ngay lập tức
        ApplyEffect(isON);

        // Chạy animation trượt
        // Nếu đang trượt dở mà bấm tiếp:
        //   → dừng coroutine cũ, bắt đầu coroutine mới từ vị trí hiện tại
        //   → cục tròn không bị "nhảy cóc" mà trượt tiếp mượt mà
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideToggleAnimation());
    }

    // ═══════════════════════════════════════════════════════════════
    // LOAD / SAVE — Thông qua SaveManager
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Đọc trạng thái của toggle từ SaveManager.
    /// Mỗi ToggleType đọc đúng property tương ứng.
    /// </summary>
    private bool LoadState()
    {
        if (SaveManager.Instance == null) return defaultIsON;

        return toggleType switch
        {
            ToggleType.Sound => SaveManager.Instance.SoundEnabled,
            ToggleType.Vibration => SaveManager.Instance.VibrationEnabled,
            ToggleType.LeftHand => SaveManager.Instance.LeftHandEnabled,
            _ => defaultIsON
        };
    }

    /// <summary>
    /// Ghi trạng thái mới vào SaveManager.
    /// SaveManager tự gọi PlayerPrefs.Save() bên trong.
    /// </summary>
    private void SaveState(bool value)
    {
        if (SaveManager.Instance == null) return;

        switch (toggleType)
        {
            case ToggleType.Sound:
                SaveManager.Instance.SaveSoundEnabled(value); break;
            case ToggleType.Vibration:
                SaveManager.Instance.SaveVibrationEnabled(value); break;
            case ToggleType.LeftHand:
                SaveManager.Instance.SaveLeftHandEnabled(value); break;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // ÁP DỤNG HIỆU ỨNG THỰC TẾ
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Mỗi ToggleType có hành động thực tế khác nhau.
    /// Được gọi cả lúc Start() lẫn lúc người chơi bấm toggle.
    /// </summary>
    private void ApplyEffect(bool value)
    {
        switch (toggleType)
        {
            // ── SOUND ──────────────────────────────────────────────
            // AudioListener.volume là volume master của TOÀN BỘ game.
            // 0 = tắt hết (nhạc nền, SFX, v.v.) | 1 = bình thường
            case ToggleType.Sound:
                AudioListener.volume = value ? 1f : 0f;
                break;

            // ── VIBRATION ──────────────────────────────────────────
            // Unity không có API "tắt rung toàn cục".
            // Cách xử lý: lưu cờ vào SaveManager, rồi ở BẤT KỲ chỗ nào
            // muốn rung, LUÔN kiểm tra cờ trước khi gọi Handheld.Vibrate():
            //
            //   if (SaveManager.Instance.VibrationEnabled)
            //       Handheld.Vibrate();
            //
            // SaveState() ở trên đã lưu cờ rồi, không cần làm thêm ở đây.
            case ToggleType.Vibration:
                // (không có hành động tức thì — cờ đã được lưu qua SaveState)
                break;

            // ── LEFT HAND ──────────────────────────────────────────
            // Bắn event tĩnh OnLeftHandChanged để các script khác tự điều chỉnh.
            // Dùng static event để bất kỳ script nào cũng đăng ký được
            // mà không cần giữ reference đến CodeToggle.
            //
            // Cách dùng ở script khác (ví dụ InputZoneManager):
            //   void OnEnable()  { CodeToggle.OnLeftHandChanged += HandleLeftHand; }
            //   void OnDisable() { CodeToggle.OnLeftHandChanged -= HandleLeftHand; }
            //   void HandleLeftHand(bool isLeft) {
            //       // Dịch chuyển vùng bấm sang trái hoặc phải
            //   }
            case ToggleType.LeftHand:
                OnLeftHandChanged?.Invoke(value);
                break;
        }
    }

    /// <summary>
    /// Event tĩnh: phát ra mỗi khi trạng thái Left Hand thay đổi.
    /// Tham số bool: true = tay trái, false = tay phải.
    /// </summary>
    public static System.Action<bool> OnLeftHandChanged;

    // ═══════════════════════════════════════════════════════════════
    // ANIMATION — Trượt mượt khi bấm toggle
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Coroutine chạy mỗi frame, cập nhật vị trí/màu theo thời gian.
    /// Dùng SmoothStep thay vì Lerp thuần để có cảm giác "phanh" ở đầu và cuối.
    /// </summary>
    IEnumerator SlideToggleAnimation()
    {
        // Xác định điểm đích dựa trên trạng thái hiện tại
        float targetX = isON ? posX_ON : posX_OFF;
        Color targetBgColor = isON ? colorON : colorOFF;
        float targetAlphaON = isON ? 1f : 0f;  // Chữ ON  hiện khi bật, mờ khi tắt
        float targetAlphaOFF = isON ? 0f : 1f;  // Chữ OFF mờ khi bật, hiện khi tắt

        float t = 0f;
        float duration = 0.15f; // Thời gian hoàn thành animation (giây)

        // Lưu điểm xuất phát TẠI THỜI ĐIỂM BẮT ĐẦU animation
        // Quan trọng khi bấm liên tục: xuất phát từ vị trí HIỆN TẠI, không phải vị trí gốc
        Vector2 startPos = handleRect.anchoredPosition;
        Color startBgColor = backgroundImage.color;
        float startAlphaON = txtON.color.a;
        float startAlphaOFF = txtOFF.color.a;

        while (t < 1f)
        {
            // Tăng t từ 0 → 1 trong `duration` giây
            t += Time.deltaTime / duration;

            // SmoothStep biến t tuyến tính thành đường cong sigmoid nhẹ:
            // tốc độ chậm ở đầu → nhanh ở giữa → chậm lại ở cuối
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 1. Trượt cục tròn theo trục X (giữ nguyên Y)
            handleRect.anchoredPosition =
                new Vector2(Mathf.Lerp(startPos.x, targetX, smoothT), startPos.y);

            // 2. Loang màu nền (xám ↔ xanh)
            backgroundImage.color =
                Color.Lerp(startBgColor, targetBgColor, smoothT);

            // 3. Fade chữ ON/OFF (một cái hiện dần, một cái mờ dần đồng thời)
            SetAlpha(txtON, Mathf.Lerp(startAlphaON, targetAlphaON, smoothT));
            SetAlpha(txtOFF, Mathf.Lerp(startAlphaOFF, targetAlphaOFF, smoothT));

            yield return null; // Chờ frame tiếp theo rồi lặp lại
        }

        // Snap đúng giá trị cuối — tránh sai số float nhỏ ở frame cuối
        SetStateImmediate(isON);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Set vị trí và màu tức thì, KHÔNG có animation.
    /// Dùng khi: khởi động game, load scene.
    /// </summary>
    void SetStateImmediate(bool on)
    {
        handleRect.anchoredPosition =
            new Vector2(on ? posX_ON : posX_OFF, handleRect.anchoredPosition.y);
        backgroundImage.color = on ? colorON : colorOFF;
        SetAlpha(txtON, on ? 1f : 0f);
        SetAlpha(txtOFF, on ? 0f : 1f);
    }

    /// <summary>
    /// Đổi alpha của Image mà không ảnh hưởng đến RGB.
    /// Null-check để tránh lỗi nếu quên gắn reference trong Inspector.
    /// </summary>
    void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}