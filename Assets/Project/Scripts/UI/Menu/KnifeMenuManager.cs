using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KnifeMenuManager : MonoBehaviour
{
    [Header("UI Elements (Knife Menu)")] // Gom nhóm lại cho dễ nhìn
    public RectTransform yellowFrame;
    public Image topPreviewKnife;
    public KnifeSlotUI defaultSlot;

    [Header("UI Elements (Main Menu Connection)")] // --- THÊM MỚI ---
                                                   // Kéo cái Img_MainKnife từ Screen_MainMenu thả vào đây
    public SpriteRenderer imgMainKnifeOnMainMenu;

    [Header("Tiến độ thu thập (Progress)")]
    public TextMeshProUGUI txtKnifeCount;
    public Transform contentContainer;

    void Start()
    {
        // 1. Chọn dao mặc định (SỬA LẠI THAM SỐ TRUYỀN VÀO)
        if (defaultSlot != null)
        {
            SelectKnife(defaultSlot); // Truyền nguyên cái ô defaultSlot vào
        }

        UpdateKnifeProgress();
    }

    // --- HÀM SELECT KNIFE ĐƯỢC SỬA LẠI HOÀN TOÀN LOGIC ---
    // Tham số truyền vào giờ là KnifeSlotUI thay vì Sprite và Rect
    public void SelectKnife(KnifeSlotUI selectedSlot)
    {
        // Lấy RectTransform của ô dao vừa chọn
        RectTransform slotRect = selectedSlot.GetComponent<RectTransform>();

        // A. LOGIC HIỂN THỊ TRÊN MENU DAO (Giữ nguyên tính năng cũ)

        // 1. Cập nhật viền vàng sang ô vừa chọn
        if (yellowFrame != null)
        {
            yellowFrame.gameObject.SetActive(true);
            yellowFrame.SetParent(slotRect);
            yellowFrame.anchoredPosition = Vector2.zero;
            yellowFrame.SetAsLastSibling();
        }

        // 2. Cập nhật ảnh preview phía trên (Hiện ảnh ĐANG hiển thị: bóng hoặc màu)
        if (topPreviewKnife != null)
        {
            topPreviewKnife.sprite = selectedSlot.myKnifeImage.sprite;
            topPreviewKnife.SetNativeSize();
        }

        // B. LOGIC ĐỒNG BỘ SANG MAIN MENU (TÍNH NĂNG MỚI THEO YÊU CẦU)

        // 3. Nếu con dao này ĐÃ SỞ HỮU (isUnlocked = true), ta mới cập nhật sang MainMenu
        if (selectedSlot.isUnlocked && imgMainKnifeOnMainMenu != null)
        {
            // Ép MainMenu hiển thị ảnh CÓ MÀU chuẩn của con dao đó
            imgMainKnifeOnMainMenu.sprite = selectedSlot.coloredSprite;
            // imgMainKnifeOnMainMenu.SetNativeSize(); // Mở dòng này nếu muốn ảnh tự về kích thước gốc
        }
        // Nếu nó CHƯA SỞ HỮU (isUnlocked = false), code dừng ở đây, MainMenu giữ nguyên dao cũ.
    }

    // --- HÀM UpdateKnifeProgress Giữ nguyên không đổi ---
    public void UpdateKnifeProgress()
    {
        // ... (Code đếm dao cũ của bạn giữ nguyên) ...
        if (txtKnifeCount == null || contentContainer == null) return;
        int unlockedCount = 0;
        int totalKnives = 0;
        foreach (Transform page in contentContainer)
        {
            foreach (Transform slot in page)
            {
                KnifeSlotUI knifeUI = slot.GetComponent<KnifeSlotUI>();
                if (knifeUI != null)
                {
                    totalKnives++;
                    if (knifeUI.isUnlocked == true) { unlockedCount++; }
                }
            }
        }
        txtKnifeCount.text = unlockedCount + "/" + totalKnives;
    }
}