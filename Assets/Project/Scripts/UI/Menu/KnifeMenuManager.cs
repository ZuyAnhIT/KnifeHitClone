using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // BẮT BUỘC THÊM DÒNG NÀY ĐỂ DÙNG LIST

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

    [Header("Hệ thống Gacha (Bốc thăm)")]
    public TextMeshProUGUI txtRandomPrice; // Kéo chữ "250" trên nút bấm vào đây
    public int currentApples = 1000;       // Giả lập số táo bạn đang có (sau này link với file Save)

    // Mảng lưu giá tiền: Vị trí 0 (Page 1) = 250, Vị trí 1 (Page 2) = 500
    public int[] pagePrices = { 250, 500 };

    private int currentPageIndex = 0; // Mặc định vừa vào là Page 1 (Index = 0)

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
    // --- BẮT ĐẦU THÊM MỚI TỪ ĐÂY ---

    // 1. Hàm này gọi khi bạn VUỐT TRANG để đổi giá tiền trên nút
    public void OnPageChanged(int newPageIndex)
    {
        currentPageIndex = newPageIndex;

        // Đổi text giá tiền thành 250 hoặc 500 tùy trang
        if (txtRandomPrice != null && pagePrices.Length > newPageIndex)
        {
            txtRandomPrice.text = pagePrices[newPageIndex].ToString();
        }
    }

    // 2. Hàm Gacha: Được gọi khi bấm nút "UNLOCK RANDOM"
    public void UnlockRandomKnife()
    {
        int cost = pagePrices[currentPageIndex];

        // Bước A: Kiểm tra xem có đủ táo không?
        if (currentApples < cost)
        {
            Debug.Log("Không đủ Táo để mua!");
            // TODO: Bạn có thể thêm hiệu ứng rung nút đỏ hoặc phát âm thanh báo lỗi ở đây
            return;
        }

        // Bước B: Lấy đúng cái Trang (Page) mà người chơi đang xem
        Transform activePage = contentContainer.GetChild(currentPageIndex);

        // Tạo một cái túi trống để gom các dao chưa mở khóa
        List<KnifeSlotUI> lockedKnives = new List<KnifeSlotUI>();

        // Quét từng ô dao trong trang đó
        foreach (Transform slot in activePage)
        {
            KnifeSlotUI knife = slot.GetComponent<KnifeSlotUI>();
            // Nếu dao này CHƯA MỞ KHÓA (isUnlocked == false), thì ném nó vào túi
            if (knife != null && knife.isUnlocked == false)
            {
                lockedKnives.Add(knife);
            }
        }

        // Bước C: Kiểm tra xem túi có rỗng không (Trang này đã mở full dao chưa?)
        if (lockedKnives.Count == 0)
        {
            Debug.Log("Trang này đã mở khóa toàn bộ dao!");
            // Tùy chọn: Làm mờ nút Random đi nếu đã mua hết
            return;
        }

        // Bước D: BỐC THĂM VÀ MỞ KHÓA!
        int randomIndex = Random.Range(0, lockedKnives.Count); // Random từ 0 đến số lượng dao trong túi
        KnifeSlotUI luckyKnife = lockedKnives[randomIndex];

        // Trừ tiền
        currentApples -= cost;
        // TODO: Gọi hàm cập nhật Text tổng số táo góc trên màn hình ở đây

        // Mở khóa dao trúng thưởng
        luckyKnife.isUnlocked = true;
        luckyKnife.UpdateVisuals(); // Bật ảnh màu lên, tắt bóng đen

        // Tự động nhảy khung viền vàng sang con dao vừa trúng thưởng luôn cho nóng!
        SelectKnife(luckyKnife);

        // Cập nhật lại text tiến độ (VD: Từ 10/16 lên 11/16)
        UpdateKnifeProgress();

        Debug.Log("Chúc mừng! Bạn vừa mở được một con dao mới!");
    }
}