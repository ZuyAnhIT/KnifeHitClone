using UnityEngine;
using UnityEngine.UI;

public class KnifeSlotUI : MonoBehaviour
{
    [Header("Giao diện")]
    public KnifeMenuManager manager;
    public Image myKnifeImage; // Ảnh con dao (thằng con)

    [Header("Dữ liệu Dao")]
    public string knifeName;
    public Sprite challengeIcon; // <--- THÊM DÒNG NÀY (Kéo icon Monsters, Pirates... vào đây trên từng ô dao)
    public bool isUnlocked = false;     // Trạng thái: True là có rồi, False là chưa có
    public Sprite coloredSprite;        // Ảnh dao có màu
    public Sprite shadowSprite;         // Ảnh bóng dao (màu trắng)

    private Image backgroundImage;      // Ảnh nền ô vuông (thằng cha)
    private Button btn;
    private RectTransform myRect;

    void Awake()
    {
        // Khởi tạo các tham chiếu trong Awake để sẵn sàng
        // trước khi KnifeMenuManager.Start() gọi LoadAllUnlockedKnives()
        btn = GetComponent<Button>();
        myRect = GetComponent<RectTransform>();
        backgroundImage = GetComponent<Image>();
    }

    void Start()
    {
        manager = FindFirstObjectByType<KnifeMenuManager>();

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

            // --- THÊM LOGIC PHÂN LOẠI TRANG Ở ĐÂY ---
            // Kiểm tra xem dao này đang nằm ở Trang mấy (Page_1 = 0, Page_2 = 1, Page_3 = 2)
            int pageIndex = transform.parent.GetSiblingIndex();

            if (pageIndex == 2)
            {
                // Nếu là Page 3 (Watch Video): Nhuộm cái bóng trắng thành màu TÍM huyền bí
                ColorUtility.TryParseHtmlString("#A238FF", out Color purpleShadow);
                myKnifeImage.color = purpleShadow;
            }
            else if (pageIndex == 3)
            {
                // NẾU LÀ PAGE 4 (Boss Knives): Bóng CAM RỰC LỬA
                ColorUtility.TryParseHtmlString("#FF6A00", out Color orangeShadow);
                myKnifeImage.color = orangeShadow;
            }
            // ---> CHÈN THÊM ĐOẠN NÀY CHO PAGE 5 <---
            else if (pageIndex == 4) // NẾU LÀ PAGE 5 (Rare & Legendary)
            {
                int slotIndex = transform.GetSiblingIndex(); // Tự động lấy thứ tự của ô dao
                if (slotIndex < 8)
                {
                    // 2 hàng đầu (Từ ô 0 đến 7): Nhuộm Bóng VÀNG (Rare)
                    ColorUtility.TryParseHtmlString("#FFC107", out Color yellowShadow);
                    myKnifeImage.color = yellowShadow;
                }
                else
                {
                    // 2 hàng sau (Từ ô 8 trở đi): Nhuộm Bóng TÍM (Legendary)
                    ColorUtility.TryParseHtmlString("#A238FF", out Color purpleShadow);
                    myKnifeImage.color = purpleShadow;
                }
            }
            // ---> CHÈN THÊM ĐOẠN NÀY CHO PAGE 6 <---
            else if (pageIndex == 5) // NẾU LÀ PAGE 6 (Challenge Knives)
            {
                int slotIndex = transform.GetSiblingIndex();

                if (slotIndex >= 0 && slotIndex <= 3) // Hàng 1 (Monsters) -> Hồng
                {
                    ColorUtility.TryParseHtmlString("#FFAEEA", out Color pinkColor);
                    myKnifeImage.color = pinkColor;
                }
                else if (slotIndex >= 4 && slotIndex <= 7) // Hàng 2 (Pirates) -> Xanh Dương
                {
                    ColorUtility.TryParseHtmlString("#8BC3F4", out Color blueColor);
                    myKnifeImage.color = blueColor;
                }
                else if (slotIndex >= 8 && slotIndex <= 11) // Hàng 3 (Jungle) -> Xanh Lá
                {
                    ColorUtility.TryParseHtmlString("#C3F56B", out Color greenColor);
                    myKnifeImage.color = greenColor;
                }
                else if (slotIndex >= 12 && slotIndex <= 15) // Hàng 4 (Treasures) -> Vàng
                {
                    ColorUtility.TryParseHtmlString("#FFDA42", out Color yellowColor);
                    myKnifeImage.color = yellowColor;
                }
            }
            // ---> CHÈN ĐOẠN NÀY ĐỂ NHUỘM MÀU PAGE 7 <---
            else if (pageIndex == 6) // NẾU LÀ PAGE 7
            {
                int slotIndex = transform.GetSiblingIndex();

                if (slotIndex >= 0 && slotIndex <= 3) // Hàng 1 (Xanh lam sáng)
                {
                    // Mình dùng mã #00FFFF cho màu Xanh Lam cực sáng (Cyan)
                    ColorUtility.TryParseHtmlString("#00FFFF", out Color cyanColor);
                    myKnifeImage.color = cyanColor;
                }
                else if (slotIndex >= 4 && slotIndex <= 7) // Hàng 2 (Hồng sáng)
                {
                    // Mình dùng mã #FF33CC cho màu Hồng rực rỡ
                    ColorUtility.TryParseHtmlString("#FF33CC", out Color pinkColor);
                    myKnifeImage.color = pinkColor;
                }
            }
            else
            {
                // Nếu là Page 1, 2: Nhuộm cái bóng trắng thành màu xanh đen mặc định
                ColorUtility.TryParseHtmlString("#203545", out Color shadowColor);
                myKnifeImage.color = shadowColor;
            }

            // Nhuộm nền ô vuông thành màu đen tối (Giữ nguyên như cũ)
            ColorUtility.TryParseHtmlString("#070F14", out Color lockedBgColor);
            backgroundImage.color = lockedBgColor;
        }
    }

    void OnSlotClicked()
    {
        // Gửi ảnh ĐANG HIỂN THỊ (màu hoặc bóng) lên Manager
        //manager.SelectKnife(myKnifeImage.sprite, myRect);
        manager.SelectKnife(this);
    }
}