using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic; // BẮT BUỘC THÊM DÒNG NÀY ĐỂ DÙNG LIST
using System.Collections; // <--- THÊM DÒNG NÀY ĐỂ DÙNG HIỆU ỨNG THỜI GIAN (COROUTINE)

public class KnifeMenuManager : MonoBehaviour
{
    // =========================================================
    // KHU VỰC 1: KHAI BÁO BIẾN (VARIABLES)
    // =========================================================

    [Header("Giao diện Header (Tiêu đề)")]
    public Image imgHeaderBackground;     // Kéo ảnh nền của Header vào đây
    public TextMeshProUGUI txtHeaderText; // Kéo chữ "KNIVES" vào đây

    [Header("UI Elements (Knife Menu)")]
    public RectTransform yellowFrame;
    public Image topPreviewKnife;
    public KnifeSlotUI defaultSlot;

    [Header("UI Elements (Main Menu Connection)")]
    public Image imgMainKnifeOnMainMenu; // Kéo cái Img_MainKnife từ Screen_MainMenu thả vào đây

    [Header("Tiến độ thu thập (Progress)")]
    public TextMeshProUGUI txtKnifeCount;
    public Transform contentContainer;

    [Header("Hệ thống Gacha (Bốc thăm)")]
    public TextMeshProUGUI txtRandomPrice; // Kéo chữ "250" trên nút bấm vào đây
    public int currentApples = 1000;       // Giả lập số táo bạn đang có (sau này link với file Save)
    public int[] pagePrices = { 250, 500, 0, 0, 0, 0, 0, 0, 0, 0 };// Mảng lưu giá tiền: Vị trí 0 (Page 1) = 250, Vị trí 1 (Page 2) = 500
    private int currentPageIndex = 0;      // Mặc định vừa vào là Page 1 (Index = 0)
    private bool isSpinning = false;

    [Header("Mua Dao Trực Tiếp (Unlock Now)")]
    public GameObject btnUnlockNowObj;        // Kéo cả cái object Btn_UnlockNow vào đây để Bật/Tắt
    public GameObject btnWatchVideoObj;       // Nút Xem Video (Màu Xanh)  
    public TextMeshProUGUI txtUnlockNowPrice; // Kéo chữ số giá tiền trên nút vào đây
    public int[] directBuyPrices = { 500, 1000 }; // Mảng giá tiền: Page 1 = 500, Page 2 = 1000
    private KnifeSlotUI currentSelectedSlot;  // Biến ngầm để nhớ xem mình đang bấm vào con dao nào

    [Header("Thanh Tiến Trình Page 3")]
    public GameObject groupProgressBar;    // Kéo Group_ProgressBar vào đây
    public Image imgBarFill;               // Kéo Img_BarFill vào đây
    public TextMeshProUGUI txtBarProgress; // Kéo Txt_ProgressAmount vào đây

    [Header("Các nút cần ẩn ở Page 3")]
    public GameObject btnUnlockRandomMain; // Kéo cụm nút Unlock Random (250/500 táo) vào đây
    public GameObject btnWatchAd50Apples;  // Kéo nút xem Video lấy 50 táo vào đây4

    [Header("Thanh Tiến Trình Page 4 (Boss)")]
    public GameObject groupProgressBarBoss;
    public Image imgBarFillBoss;
    public TextMeshProUGUI txtBarProgressBoss;

    [Header("Giao diện Preview Page 4 (Boss)")]
    public GameObject bossPreviewPanel; // Kéo Group_BossPreview vừa tạo vào đây
    public Image bossIcon;             // Kéo Img_CrossedKnivesIcon vào đây
    public TextMeshProUGUI bossStatusText; // Kéo Txt_BossStatus vào đây
    public TextMeshProUGUI bossKnifeNameText; // Kéo Txt_BossKnifeName vào đây
    public Image rarityTagImage;   // Kéo object Img_RarityTag vào đây
    public Sprite rareTagSprite;   // Kéo ảnh thẻ RARE (chữ cam/vàng) vào đây
    public Sprite legendTagSprite; // Kéo ảnh thẻ LEGENDARY (chữ tím) vào đây

    [Header("Thanh Tiến Trình Page 6 (Challenge)")]
    public GameObject groupProgressBarChallenge;
    public Image imgBarFillChallenge;
    public TextMeshProUGUI txtBarProgressChallenge;

    [Header("Sprites Tags Page 6 (Challenge)")]
    public Sprite tagHàng1_Monsters;
    public Sprite tagHàng2_Pirates;
    public Sprite tagHàng3_Jungle;
    public Sprite tagHàng4_Treasure;

    [Header("Sprites Tags Page 7")]
    public Sprite tagHàng1_IceAge;    // Kéo ảnh thẻ "ICE AGE" vào
    public Sprite tagHàng2_PinkDao;

    [Header("Thanh Tiến Trình Page 8,9,10 (Packs)")]
    public GameObject groupProgressBarPacks;
    public Image imgBarFillPacks;
    public TextMeshProUGUI txtBarProgressPacks;


    // =========================================================
    // KHU VỰC 2: HÀM KHỞI TẠO (INITIALIZATION)
    // =========================================================

    void Start()
    {
        // Chọn dao mặc định
        if (defaultSlot != null)
        {
            SelectKnife(defaultSlot);
        }

        UpdateKnifeProgress();
    }


    // =========================================================
    // KHU VỰC 3: XỬ LÝ LỰA CHỌN & GIAO DIỆN (SELECTION & UI)
    // =========================================================

    // Hàm xử lý khi người chơi bấm vào 1 ô dao
    public void SelectKnife(KnifeSlotUI selectedSlot)
    {
        // Lưu lại ô dao đang được bấm để lát nữa biết đường mà mua
        currentSelectedSlot = selectedSlot;
        RectTransform slotRect = selectedSlot.GetComponent<RectTransform>();

        // A. LOGIC HIỂN THỊ TRÊN MENU DAO 
        if (yellowFrame != null)
        {
            yellowFrame.gameObject.SetActive(true);
            yellowFrame.SetParent(slotRect);
            yellowFrame.anchoredPosition = Vector2.zero;
            yellowFrame.SetAsLastSibling();
        }

        // --- 1. CẬP NHẬT DAO KHỔNG LỒ (GIỮ NGUYÊN LOGIC CŨ CHO MỌI TRANG) ---
        if (topPreviewKnife != null)
        {
            topPreviewKnife.sprite = selectedSlot.myKnifeImage.sprite;
            topPreviewKnife.SetNativeSize();
        }

        // --- 2. BẬT/TẮT BẢNG TRẠNG THÁI BOSS (HIỆN Ở PAGE 4 VÀ PAGE 5) ---
        int pageIndexPreview = selectedSlot.transform.parent.GetSiblingIndex();

        if (pageIndexPreview == 3 || pageIndexPreview == 4 || pageIndexPreview == 5 || pageIndexPreview == 6) // PAGE 4, 5, 6
        {
            if (bossPreviewPanel != null) bossPreviewPanel.SetActive(true);
            if (bossKnifeNameText != null) bossKnifeNameText.text = selectedSlot.knifeName;

            int slotIndex = selectedSlot.transform.GetSiblingIndex();
            bool isRare = (slotIndex < 8);

            // --- A. GẮN THẺ TAG (RARITY/COLLECTION) ---
            if (rarityTagImage != null)
            {
                if (pageIndexPreview == 4) // P5
                {
                    rarityTagImage.gameObject.SetActive(true);
                    rarityTagImage.sprite = isRare ? rareTagSprite : legendTagSprite;
                }
                else if (pageIndexPreview == 5) // P6 (Đổi Tag theo hàng)
                {
                    rarityTagImage.gameObject.SetActive(true);
                    if (slotIndex >= 0 && slotIndex <= 3) rarityTagImage.sprite = tagHàng1_Monsters;
                    else if (slotIndex >= 4 && slotIndex <= 7) rarityTagImage.sprite = tagHàng2_Pirates;
                    else if (slotIndex >= 8 && slotIndex <= 11) rarityTagImage.sprite = tagHàng3_Jungle;
                    else if (slotIndex >= 12 && slotIndex <= 15) rarityTagImage.sprite = tagHàng4_Treasure;
                }
                else if (pageIndexPreview == 6) // PAGE 7 (Ice Age...)
                {
                    rarityTagImage.gameObject.SetActive(true);
                    if (slotIndex >= 0 && slotIndex <= 3) rarityTagImage.sprite = tagHàng1_IceAge;
                    else if (slotIndex >= 4 && slotIndex <= 7) rarityTagImage.sprite = tagHàng2_PinkDao;
                }
                else // P4
                {
                    rarityTagImage.gameObject.SetActive(false);
                }
            }

            // --- B. ĐỔI ICON CHÉO NHAU ---
            if (bossIcon != null)
            {
                if (pageIndexPreview == 5 || pageIndexPreview == 6) // P6: Dùng Icon động từ ô dao
                {
                    bossIcon.sprite = selectedSlot.challengeIcon;
                }
                else // P4, 5: Bạn phải tạo 1 biến chứa ảnh 2 dao đan chéo mặc định (VD: defaultCrossedKnivesSprite) để gán lại vào đây nếu không nó bị kẹt ảnh Monster của P6.
                {
                    // Tạm thời bỏ qua nếu bạn dùng chung ảnh, hoặc thêm: bossIcon.sprite = defaultCrossedKnivesSprite;
                }
            }

            // --- C. XỬ LÝ MÀU SẮC DỰA VÀO UNLOCKED ---
            if (selectedSlot.isUnlocked == true)
            {
                // ĐÃ MỞ KHÓA
                if (bossStatusText != null) bossStatusText.text = "COMPLETE";

                if (pageIndexPreview == 5 || pageIndexPreview == 6)
                {
                    // PAGE 6: Icon giữ nguyên màu thật (Trắng), Tên Dao trắng, Status xanh lá (#0DCF3B)
                    if (bossIcon != null) bossIcon.color = Color.white;
                    if (bossKnifeNameText != null) bossKnifeNameText.color = Color.white;

                    ColorUtility.TryParseHtmlString("#0DCF3B", out Color greenColor);
                    if (bossStatusText != null) bossStatusText.color = greenColor;
                }
                else
                {
                    // PAGE 4,5: Nhuộm Xanh Lá cả Icon và Status
                    ColorUtility.TryParseHtmlString("#0DCF3B", out Color greenColor);
                    if (bossIcon != null) bossIcon.color = greenColor;
                    if (bossStatusText != null) bossStatusText.color = greenColor;
                    if (bossKnifeNameText != null) bossKnifeNameText.color = Color.white;
                }
            }
            else
            {
                // CHƯA MỞ KHÓA
                if (pageIndexPreview == 5 || pageIndexPreview == 6)
                {
                    // Nếu là Page 6: Luôn hiện COMPLETE
                    if (bossStatusText != null) bossStatusText.text = "COMPLETE";
                }
                else
                {
                    // Nếu là Page 4, 5: Hiện DEFEAT BOSS
                    if (bossStatusText != null) bossStatusText.text = "DEFEAT BOSS";
                }

                // Page 4,5,6 Icon chưa mở đều hiển thị màu Trắng/Gốc, Status Trắng
                if (bossIcon != null) bossIcon.color = Color.white;
                if (bossStatusText != null) bossStatusText.color = Color.white;

                if (pageIndexPreview == 5 || pageIndexPreview == 6)
                {
                    // PAGE 6: Tên dao lấy màu tương ứng của bóng dao (Hồng, Xanh, Vàng...)
                    if (bossKnifeNameText != null) bossKnifeNameText.color = selectedSlot.myKnifeImage.color;
                }
                else if (pageIndexPreview == 4)
                {
                    // PAGE 5: Tên Vàng hoặc Tím
                    ColorUtility.TryParseHtmlString(isRare ? "#FFC107" : "#A238FF", out Color p5Color);
                    if (bossKnifeNameText != null) bossKnifeNameText.color = p5Color;
                }
                else
                {
                    // PAGE 4: Tên Cam
                    ColorUtility.TryParseHtmlString("#FF6A00", out Color orangeColor);
                    if (bossKnifeNameText != null) bossKnifeNameText.color = orangeColor;
                }
            }
        }
        else // PAGE 1, 2, 3
        {
            if (bossPreviewPanel != null) bossPreviewPanel.SetActive(false);
        }

        // B. LOGIC ĐỒNG BỘ SANG MAIN MENU 
        if (selectedSlot.isUnlocked && imgMainKnifeOnMainMenu != null)
        {
            imgMainKnifeOnMainMenu.sprite = selectedSlot.coloredSprite;
        }

        // C. LOGIC ẨN/HIỆN NÚT UNLOCK NOW & NÚT VIDEO 
        if (selectedSlot.isUnlocked == true)
        {
            // Nếu dao ĐÃ SỞ HỮU -> Ẩn cả 2 nút đi
            if (btnUnlockNowObj != null) btnUnlockNowObj.SetActive(false);
            if (btnWatchVideoObj != null) btnWatchVideoObj.SetActive(false);
        }
        else
        {
            // Nếu dao CHƯA CÓ -> Kiểm tra xem nó ở Page mấy?
            int pageIndex = selectedSlot.transform.parent.GetSiblingIndex();

            if (pageIndex == 2)
            {
                // NẾU LÀ PAGE 3 (Dao VIP) -> HIỆN nút Video, ẨN nút Táo
                if (btnUnlockNowObj != null) btnUnlockNowObj.SetActive(false);
                if (btnWatchVideoObj != null) btnWatchVideoObj.SetActive(true);
            }
            else if (pageIndex >= 3 && pageIndex <= 9)
            {
                // NẾU LÀ PAGE 4 (Boss) -> ẨN CẢ 2 NÚT
                if (btnUnlockNowObj != null) btnUnlockNowObj.SetActive(false);
                if (btnWatchVideoObj != null) btnWatchVideoObj.SetActive(false);
            }
            else
            {
                // NẾU LÀ PAGE 1, 2 -> HIỆN nút Táo, ẨN nút Video
                if (btnUnlockNowObj != null) btnUnlockNowObj.SetActive(true);
                if (btnWatchVideoObj != null) btnWatchVideoObj.SetActive(false);

                // Áp giá tiền 500/1000 vào chữ trên nút Táo
                if (txtUnlockNowPrice != null && pageIndex < directBuyPrices.Length)
                {
                    txtUnlockNowPrice.text = directBuyPrices[pageIndex].ToString();
                }
            }
        }
    }

    // Hàm gọi khi vuốt chuyển trang
    public void OnPageChanged(int newPageIndex)
    {
        currentPageIndex = newPageIndex;

        // A. LOGIC ĐỔI MÀU VÀ CHỮ HEADER
        if (imgHeaderBackground != null && txtHeaderText != null)
        {
            if (newPageIndex == 2) // NẾU LÀ TRANG 3 (WATCH VIDEOS)
            {
                txtHeaderText.text = "WATCH VIDEOS";
                ColorUtility.TryParseHtmlString("#A238FF", out Color purpleColor);
                imgHeaderBackground.color = purpleColor;
            }
            else if (newPageIndex == 3 || newPageIndex == 4) // NẾU LÀ TRANG 4 (BOSS KNIVES)
            {
                txtHeaderText.text = "BOSS KNIVES";
                ColorUtility.TryParseHtmlString("#FF6A00", out Color orangeColor); // MÀU CAM
                imgHeaderBackground.color = orangeColor;
            }
            else if (newPageIndex == 5 || newPageIndex == 6) // TRANG 6
            {
                txtHeaderText.text = "CHALLENGE KNIVES";
                ColorUtility.TryParseHtmlString("#E7B75B", out Color challengeColor);
                imgHeaderBackground.color = challengeColor;
            }
            else if (newPageIndex == 7 || newPageIndex == 8 || newPageIndex == 9)
            {
                txtHeaderText.text = "KNIFE PACKS";
                ColorUtility.TryParseHtmlString("#5C41FB", out Color packsColor); // MÀU TÍM XANH
                imgHeaderBackground.color = packsColor;
            }
            else // NẾU LÀ TRANG 1, 2
            {
                txtHeaderText.text = "GET FOR APPLES ";
                ColorUtility.TryParseHtmlString("#166890", out Color defaultBlueColor);
                imgHeaderBackground.color = defaultBlueColor;
            }

        }

        // B. LOGIC ẨN HIỆN NÚT VÀ THANH MÁU
        if (newPageIndex == 2) // PAGE 3
        {
            if (btnUnlockRandomMain != null) btnUnlockRandomMain.SetActive(false);
            if (btnWatchAd50Apples != null) btnWatchAd50Apples.SetActive(false);

            if (groupProgressBar != null) groupProgressBar.SetActive(true);
            if (groupProgressBarBoss != null) groupProgressBarBoss.SetActive(false);
            if (groupProgressBarChallenge != null) groupProgressBarChallenge.SetActive(false); // Tắt P6
            if (groupProgressBarPacks != null) groupProgressBarPacks.SetActive(false);
            UpdateSpecialPageProgress(newPageIndex);
        }
        else if (newPageIndex == 3 || newPageIndex == 4) // PAGE 4
        {
            if (btnUnlockRandomMain != null) btnUnlockRandomMain.SetActive(false);
            if (btnWatchAd50Apples != null) btnWatchAd50Apples.SetActive(false);

            if (groupProgressBar != null) groupProgressBar.SetActive(false);
            if (groupProgressBarBoss != null) groupProgressBarBoss.SetActive(true);
            if (groupProgressBarChallenge != null) groupProgressBarChallenge.SetActive(false); // Tắt P6
            if (groupProgressBarPacks != null) groupProgressBarPacks.SetActive(false);
            UpdateSpecialPageProgress(newPageIndex);
        }
        else if (newPageIndex == 5 || newPageIndex == 6) // PAGE 6
        {
            // Ẩn nút random & video
            if (btnUnlockRandomMain != null) btnUnlockRandomMain.SetActive(false);
            if (btnWatchAd50Apples != null) btnWatchAd50Apples.SetActive(false);

            // BẬT THANH MÁU P6, TẮT CÁC THANH KHÁC
            if (groupProgressBar != null) groupProgressBar.SetActive(false);
            if (groupProgressBarBoss != null) groupProgressBarBoss.SetActive(false);
            if (groupProgressBarChallenge != null) groupProgressBarChallenge.SetActive(true);
            if (groupProgressBarPacks != null) groupProgressBarPacks.SetActive(false);
            UpdateSpecialPageProgress(newPageIndex);
        }
        // ---> THÊM ĐOẠN ELSE IF MỚI NÀY CHO PAGE 8, 9, 10 <---
        else if (newPageIndex == 7 || newPageIndex == 8 || newPageIndex == 9)
        {
            if (btnUnlockRandomMain != null) btnUnlockRandomMain.SetActive(false);
            if (btnWatchAd50Apples != null) btnWatchAd50Apples.SetActive(false);

            if (groupProgressBar != null) groupProgressBar.SetActive(false);
            if (groupProgressBarBoss != null) groupProgressBarBoss.SetActive(false);
            if (groupProgressBarChallenge != null) groupProgressBarChallenge.SetActive(false);

            // CHỈ BẬT THANH MÁU CỦA PACKS
            if (groupProgressBarPacks != null) groupProgressBarPacks.SetActive(true);

            UpdateSpecialPageProgress(newPageIndex);
        }
        else // PAGE 1, 2
        {
            if (btnUnlockRandomMain != null) btnUnlockRandomMain.SetActive(true);
            if (btnWatchAd50Apples != null) btnWatchAd50Apples.SetActive(true);

            if (groupProgressBar != null) groupProgressBar.SetActive(false);
            if (groupProgressBarBoss != null) groupProgressBarBoss.SetActive(false);
            if (groupProgressBarChallenge != null) groupProgressBarChallenge.SetActive(false); // Tắt P6
            if (txtRandomPrice != null && pagePrices.Length > newPageIndex)
            {
                txtRandomPrice.text = pagePrices[newPageIndex].ToString();
            }
        }
    }


    // =========================================================
    // KHU VỰC 4: HỆ THỐNG MUA/MỞ KHÓA DAO (UNLOCK SYSTEM)
    // =========================================================

    // Hàm 1: Nút bấm Bốc thăm ngẫu nhiên (Đã kiểm tra điều kiện quay)
    public void UnlockRandomKnife()
    {
        // Nếu đang trong lúc quay viền vàng thì cấm bấm tiếp
        if (isSpinning) return;

        int cost = pagePrices[currentPageIndex];

        if (currentApples < cost)
        {
            Debug.Log("Không đủ Táo để mua!");
            return;
        }

        Transform activePage = contentContainer.GetChild(currentPageIndex);
        List<KnifeSlotUI> lockedKnives = new List<KnifeSlotUI>();

        // Lọc ra các dao chưa mở khóa
        foreach (Transform slot in activePage)
        {
            KnifeSlotUI knife = slot.GetComponent<KnifeSlotUI>();
            if (knife != null && knife.isUnlocked == false)
            {
                lockedKnives.Add(knife);
            }
        }

        if (lockedKnives.Count == 0)
        {
            Debug.Log("Trang này đã mở khóa toàn bộ dao!");
            return;
        }
        lockedKnives.Sort((daoA, daoB) =>
        {
            // Lấy vị trí thật của dao trên lưới (từ 0 đến 15)
            int indexA = daoA.transform.GetSiblingIndex();
            int indexB = daoB.transform.GetSiblingIndex();

            // Công thức tính tọa độ Cột dọc (Vì lưới của chúng ta có 4 cột ngang)
            // (Lấy phần dư % 4 để biết nó ở Cột mấy, chia / 4 để biết Hàng mấy)
            int chieuDocA = (indexA % 4) * 4 + (indexA / 4);
            int chieuDocB = (indexB % 4) * 4 + (indexB / 4);

            return chieuDocA.CompareTo(chieuDocB);
        });

        // Trừ táo trước khi quay để an toàn
        currentApples -= cost;

        // BẮT ĐẦU CHẠY HIỆU ỨNG VÒNG QUAY VIỀN VÀNG
        StartCoroutine(SpinRouletteRoutine(lockedKnives));
    }

    // Hàm Hiệu ứng chạy Viền Vàng (Coroutine)
    private IEnumerator SpinRouletteRoutine(List<KnifeSlotUI> lockedKnives)
    {
        isSpinning = true; // Khóa nút bốc thăm

        // 1. Chốt trước con dao trúng thưởng ngầm ở bên trong
        int winningIndex = Random.Range(0, lockedKnives.Count);
        KnifeSlotUI luckyKnife = lockedKnives[winningIndex];

        // 2. Tính toán số bước nhảy: Chạy cuốn chiếu 3 vòng danh sách rồi dừng ở winningIndex
        int totalJumps = (lockedKnives.Count * 3) + winningIndex;
        float spinDelay = 0.05f; // Tốc độ chạy lúc đầu (Rất nhanh: 0.05 giây 1 ô)

        // 3. Bắt đầu vòng lặp nhảy viền vàng
        for (int i = 0; i <= totalJumps; i++)
        {
            // Lấy ô dao theo thứ tự từ trên xuống dưới trong danh sách bị khóa
            int currentIndex = i % lockedKnives.Count;
            KnifeSlotUI currentFocus = lockedKnives[currentIndex];

            // Dịch chuyển viền vàng vào ô đang xét
            if (yellowFrame != null)
            {
                yellowFrame.gameObject.SetActive(true);
                yellowFrame.SetParent(currentFocus.GetComponent<RectTransform>());
                yellowFrame.anchoredPosition = Vector2.zero;
                yellowFrame.SetAsLastSibling();
            }

            // 4. Hiệu ứng hồi hộp: Chậm dần ở 6 nhịp nhảy cuối cùng
            if (i >= totalJumps - 6)
            {
                spinDelay += 0.08f; // Càng gần đích nhảy càng chậm
            }

            // Lệnh đợi thời gian của Unity
            yield return new WaitForSeconds(spinDelay);
        }

        // --- KẾT THÚC VÒNG QUAY: MỞ KHÓA VÀ GÁN DỮ LIỆU ---
        luckyKnife.isUnlocked = true;
        luckyKnife.UpdateVisuals();

        SelectKnife(luckyKnife); // Tự động load dao trúng thưởng lên bảng to
        UpdateKnifeProgress();

        Debug.Log("Chúc mừng! Bạn vừa quay trúng một con dao mới!");

        isSpinning = false; // Mở khóa nút cho phép quay tiếp
    }

    // Hàm 2: Mua trực tiếp 1 con dao bằng Táo
    public void BuySpecificKnife()
    {
        if (currentSelectedSlot == null || currentSelectedSlot.isUnlocked == true) return;

        int pageIndex = currentSelectedSlot.transform.parent.GetSiblingIndex();
        int cost = directBuyPrices[pageIndex];

        if (currentApples >= cost)
        {
            currentApples -= cost;

            currentSelectedSlot.isUnlocked = true;
            currentSelectedSlot.UpdateVisuals();

            SelectKnife(currentSelectedSlot);
            UpdateKnifeProgress();
            Debug.Log("Mua đứt con dao thành công với giá " + cost);
        }
        else
        {
            Debug.Log("Không đủ Táo để mua con dao này!");
        }
    }

    // Hàm 3: Mở khóa bằng cách xem Video (Cho Page 3)
    public void UnlockByWatchingVideo()
    {
        if (currentSelectedSlot == null || currentSelectedSlot.isUnlocked == true) return;

        Debug.Log("Đang bật Video Quảng Cáo... Đợi người chơi xem xong...");

        currentSelectedSlot.isUnlocked = true;
        currentSelectedSlot.UpdateVisuals();

        SelectKnife(currentSelectedSlot);
        UpdateKnifeProgress();
        UpdateSpecialPageProgress(2);
        Debug.Log("Nhận dao VIP thành công nhờ xem Video!");
    }


    // =========================================================
    // KHU VỰC 5: HỆ THỐNG CẬP NHẬT TIẾN ĐỘ (PROGRESS TRACKING)
    // =========================================================

    // Hàm đếm tổng số lượng dao (Góc trên cùng)
    public void UpdateKnifeProgress()
    {
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

        // Cập nhật text kèm chữ phía sau
        txtKnifeCount.text = unlockedCount + "/" + totalKnives;
    }


    // Hàm đếm chung cho Thanh Máu ở Page 3, 4, 5
    public void UpdateSpecialPageProgress(int pageIndex)
    {
        if (contentContainer == null) return;

        int totalKnives = 0;
        int unlockedKnives = 0;

        // 1. NẾU LÀ DAO BOSS (PAGE 4 HOẶC PAGE 5) -> CỘNG GỘP CẢ 2 TRANG
        if (pageIndex == 3 || pageIndex == 4)
        {
            // Dùng vòng lặp for để quét qua cả index 3 (Page 4) và index 4 (Page 5)
            for (int i = 3; i <= 4; i++)
            {
                // Kiểm tra an toàn xem có tồn tại trang đó không
                if (i >= contentContainer.childCount) continue;

                foreach (Transform slot in contentContainer.GetChild(i))
                {
                    KnifeSlotUI knife = slot.GetComponent<KnifeSlotUI>();
                    if (knife != null)
                    {
                        totalKnives++;
                        if (knife.isUnlocked) unlockedKnives++;
                    }
                }
            }

            // Hiển thị lên thanh máu Boss
            if (totalKnives > 0)
            {
                if (txtBarProgressBoss != null) txtBarProgressBoss.text = unlockedKnives + "/" + totalKnives + " BOSSES DEFEATED";
                if (imgBarFillBoss != null) imgBarFillBoss.fillAmount = (float)unlockedKnives / totalKnives;
            }
        }

        // ---> THÊM LOGIC ĐẾM CỘNG GỘP CHO PAGE 6 VÀ 7 <---
        else if (pageIndex == 5 || pageIndex == 6)
        {
            // Vòng lặp quét qua cả 2 trang 6 (index 5) và 7 (index 6)
            for (int i = 5; i <= 6; i++)
            {
                if (i >= contentContainer.childCount) continue;

                foreach (Transform slot in contentContainer.GetChild(i))
                {
                    KnifeSlotUI knife = slot.GetComponent<KnifeSlotUI>();
                    // Chỉ đếm nếu đối tượng đó thực sự có script KnifeSlotUI (Bỏ qua cái ảnh đen Coming Soon)
                    if (knife != null)
                    {
                        totalKnives++;
                        if (knife.isUnlocked) unlockedKnives++;
                    }
                }
            }

            if (totalKnives > 0)
            {
                if (txtBarProgressChallenge != null) txtBarProgressChallenge.text = unlockedKnives + "/" + totalKnives + " KNIVES UNLOCKED";
                if (imgBarFillChallenge != null) imgBarFillChallenge.fillAmount = (float)unlockedKnives / totalKnives;
            }
        }
        // ---> THÊM LOGIC ĐẾM CỘNG GỘP CHO PAGE 8, 9, 10 <---
        else if (pageIndex == 7 || pageIndex == 8 || pageIndex == 9)
        {
            // Quét qua cả 3 trang: index 7, 8, 9
            for (int i = 7; i <= 9; i++)
            {
                if (i >= contentContainer.childCount) continue;

                foreach (Transform slot in contentContainer.GetChild(i))
                {
                    KnifeSlotUI knife = slot.GetComponent<KnifeSlotUI>();
                    if (knife != null)
                    {
                        totalKnives++;
                        if (knife.isUnlocked) unlockedKnives++;
                    }
                }
            }

            // Ghi chữ "KNIVES ACQUIRES"
            if (totalKnives > 0)
            {
                if (txtBarProgressPacks != null) txtBarProgressPacks.text = unlockedKnives + "/" + totalKnives + " KNIVES ACQUIRES";
                if (imgBarFillPacks != null) imgBarFillPacks.fillAmount = (float)unlockedKnives / totalKnives;
            }
        }
        // 2. NẾU LÀ DAO VIDEO (PAGE 3) -> CHỈ ĐẾM MÌNH TRANG ĐÓ
        else if (pageIndex == 2)
        {
            if (2 >= contentContainer.childCount) return;

            foreach (Transform slot in contentContainer.GetChild(2))
            {
                KnifeSlotUI knife = slot.GetComponent<KnifeSlotUI>();
                if (knife != null)
                {
                    totalKnives++;
                    if (knife.isUnlocked) unlockedKnives++;
                }
            }

            // Hiển thị lên thanh máu Video
            if (totalKnives > 0)
            {
                if (txtBarProgress != null) txtBarProgress.text = unlockedKnives + "/" + totalKnives + " KNIVES UNLOCKED";
                if (imgBarFill != null) imgBarFill.fillAmount = (float)unlockedKnives / totalKnives;
            }
        }
    }
}