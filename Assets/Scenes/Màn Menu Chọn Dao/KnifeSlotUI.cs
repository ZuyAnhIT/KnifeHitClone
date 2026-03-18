using UnityEngine;
using UnityEngine.UI;

public class KnifeSlotUI : MonoBehaviour
{
    [Header("Giao diện")]
    public KnifeMenuManager manager;
    public Image myKnifeImage; // Ảnh con dao (thằng con)

    [Header("Dữ liệu Dao")]
    public bool isUnlocked = false;     // Trạng thái: True là có rồi, False là chưa có
    public Sprite coloredSprite;        // Ảnh dao có màu
    public Sprite shadowSprite;         // Ảnh bóng dao (màu trắng)

    private Image backgroundImage;      // Ảnh nền ô vuông (thằng cha)
    private Button btn;
    private RectTransform myRect;

    void Start()
    {
        manager = FindFirstObjectByType<KnifeMenuManager>();
        btn = GetComponent<Button>();
        myRect = GetComponent<RectTransform>();
        backgroundImage = GetComponent<Image>(); // Tự động lấy ô nền

        // Cập nhật giao diện ngay khi game chạy
        UpdateVisuals();

        btn.onClick.AddListener(OnSlotClicked);
    }

    // Hàm thay đổi giao diện dựa trên trạng thái
    public void UpdateVisuals()
    {
        if (isUnlocked)
        {
            // 1. ĐÃ SỞ HỮU: Hiện dao màu, KHÔNG đổ bóng
            myKnifeImage.sprite = coloredSprite;

            // Đặt màu dao về trắng tinh để hiển thị đúng 100% màu gốc của tấm ảnh
            myKnifeImage.color = Color.white;

            // Đặt màu nền của ô vuông về trắng tinh để giữ đúng màu gốc, không bị tối đi
            backgroundImage.color = Color.white;
        }
        else
        {
            // 2. CHƯA SỞ HỮU: Hiện bóng dao, nền tối thui
            myKnifeImage.sprite = shadowSprite;

            // Nhuộm cái bóng trắng thành màu tối chìm vào nền
            ColorUtility.TryParseHtmlString("#203545", out Color shadowColor);
            myKnifeImage.color = shadowColor;

            // Nhuộm nền ô vuông thành màu đen tối
            ColorUtility.TryParseHtmlString("#070F14", out Color lockedBgColor);
            backgroundImage.color = lockedBgColor;
        }
    }

    void OnSlotClicked()
    {
        // Gửi ảnh ĐANG HIỂN THỊ (màu hoặc bóng) lên Manager
        manager.SelectKnife(myKnifeImage.sprite, myRect);
    }
}