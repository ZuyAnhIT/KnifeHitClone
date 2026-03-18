using UnityEngine;
using UnityEngine.UI;
using TMPro; // BẮT BUỘC PHẢI THÊM DÒNG NÀY ĐỂ DÙNG TEXTMESHPRO

public class KnifeMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform yellowFrame;
    public Image topPreviewKnife;
    public KnifeSlotUI defaultSlot;

    [Header("Tiến độ thu thập (Progress)")]
    public TextMeshProUGUI txtKnifeCount; // Kéo Txt_Count vào đây
    public Transform contentContainer;    // Kéo object 'Content' vào đây

    void Start()
    {
        // 1. Chọn dao mặc định (Code cũ của bạn)
        if (defaultSlot != null)
        {
            SelectKnife(defaultSlot.myKnifeImage.sprite, defaultSlot.GetComponent<RectTransform>());
        }

        // 2. Tự động đếm và cập nhật số dao ngay khi vừa vào game
        UpdateKnifeProgress();
    }

    // Hàm thực hiện việc đếm dao
    public void UpdateKnifeProgress()
    {
        if (txtKnifeCount == null || contentContainer == null) return;

        int unlockedCount = 0;
        int totalKnives = 0;

        // Lớp 1: Quét qua các Trang nằm trong Content (Hiện tại chỉ có Page_1)
        foreach (Transform page in contentContainer)
        {
            // Lớp 2: Quét qua tất cả 16 ô dao nằm trong Page_1
            foreach (Transform slot in page)
            {
                KnifeSlotUI knifeUI = slot.GetComponent<KnifeSlotUI>();
                
                // Nếu đúng là một ô dao thì mới đếm
                if (knifeUI != null)
                {
                    totalKnives++; // Cộng 1 vào tổng số dao (Tổng sẽ là 16)
                    
                    if (knifeUI.isUnlocked == true) 
                    {
                        unlockedCount++; // Nếu đã tick mở khóa thì cộng 1 vào số dao đang có
                    }
                }
            }
        }

        // Cập nhật lên text
        txtKnifeCount.text = unlockedCount + "/" + totalKnives;
    }

    public void SelectKnife(Sprite clickedSprite, RectTransform slotRect)
    {
        // ... (Hàm này bạn GIỮ NGUYÊN code cũ không cần sửa gì cả) ...
        yellowFrame.gameObject.SetActive(true);
        yellowFrame.SetParent(slotRect);
        yellowFrame.anchoredPosition = Vector2.zero;
        yellowFrame.SetAsLastSibling();

        if (topPreviewKnife != null)
        {
            topPreviewKnife.sprite = clickedSprite;
            topPreviewKnife.SetNativeSize();
        }
    }
}