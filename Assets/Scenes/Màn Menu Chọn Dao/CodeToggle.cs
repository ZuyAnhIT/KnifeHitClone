using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Bắt buộc phải có để dùng Coroutine

public class CodeToggle : MonoBehaviour
{
    [Header("Thành phần UI")]
    public RectTransform handleRect; // Kéo object Img_Handle vào đây
    public Image backgroundImage;    // Kéo object Img_Background vào đây
    public Image txtON;              // Kéo object Txt_ON vào đây
    public Image txtOFF;             // Kéo object Txt_OFF vào đây

    [Header("Cài đặt Vị trí Cục tròn (Trục X)")]
    public float posX_ON = 55f;      // Tọa độ X khi trượt sang phải (Bật)
    public float posX_OFF = -55f;    // Tọa độ X khi trượt sang trái (Tắt)

    [Header("Cài đặt Màu Nền")]
    public Color colorON = new Color(0.2f, 0.8f, 0.2f, 1f); // Màu xanh lá
    public Color colorOFF = new Color(0.6f, 0.6f, 0.6f, 1f); // Màu xám

    [Header("Trạng thái mặc định")]
    public bool isON = false;

    private Button myButton;
    private Coroutine slideCoroutine;

    void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnToggleClicked);
        
        // Vừa vào game thì set cứng vị trí luôn, không trượt
        SetStateImmediate(isON);
    }

    public void OnToggleClicked()
    {
        isON = !isON; // Lật trạng thái
        
        // Nếu đang trượt dở mà người chơi bấm tiếp thì dừng trượt cũ, chạy trượt mới
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideToggleAnimation());
    }

    // Thuật toán trượt mượt mà
    IEnumerator SlideToggleAnimation()
    {
        float targetX = isON ? posX_ON : posX_OFF;
        Color targetBgColor = isON ? colorON : colorOFF;
        float targetAlphaON = isON ? 1f : 0f;
        float targetAlphaOFF = isON ? 0f : 1f;

        float t = 0;
        float duration = 0.15f; // Thời gian trượt: 0.15 giây (Cực nhanh và mượt)

        Vector2 startPos = handleRect.anchoredPosition;
        Color startBgColor = backgroundImage.color;
        float startAlphaON = txtON.color.a;
        float startAlphaOFF = txtOFF.color.a;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            // SmoothStep giúp lúc bắt đầu và kết thúc có độ hãm phanh rất êm
            float smoothT = Mathf.SmoothStep(0, 1, t); 

            // 1. Trượt cục tròn
            handleRect.anchoredPosition = new Vector2(Mathf.Lerp(startPos.x, targetX, smoothT), startPos.y);
            
            // 2. Đổi màu nền (Loang màu từ từ)
            backgroundImage.color = Color.Lerp(startBgColor, targetBgColor, smoothT);
            
            // 3. Mờ/Hiện chữ từ từ
            SetAlpha(txtON, Mathf.Lerp(startAlphaON, targetAlphaON, smoothT));
            SetAlpha(txtOFF, Mathf.Lerp(startAlphaOFF, targetAlphaOFF, smoothT));

            yield return null; // Đợi khung hình tiếp theo
        }
    }

    // Hàm thiết lập tức thì (Dùng cho lúc Start game)
    void SetStateImmediate(bool on)
    {
        handleRect.anchoredPosition = new Vector2(on ? posX_ON : posX_OFF, handleRect.anchoredPosition.y);
        backgroundImage.color = on ? colorON : colorOFF;
        SetAlpha(txtON, on ? 1f : 0f);
        SetAlpha(txtOFF, on ? 0f : 1f);
    }

    // Hàm phụ trợ để thay đổi độ mờ (Alpha) của ảnh
    void SetAlpha(Image img, float alpha)
    {
        if (img != null) {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
}