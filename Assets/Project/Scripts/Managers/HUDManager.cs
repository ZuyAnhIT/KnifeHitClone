using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    // ═══════════════════════════════════════════
    // INSPECTOR VARIABLES
    // ═══════════════════════════════════════════

    [Header("── Knife Queue ──")]
    [SerializeField] private Transform knifeQueueParent; // Kéo HUD_KnifeQueue vào đây
    [SerializeField] private GameObject knifeIconPrefab;  // Prefab 1 icon dao nhỏ trên HUD
    [SerializeField] private float iconSpacing = 48f;     // Khoảng cách giữa các icon dao

    [Header("── Position cố định ──")]
    [SerializeField] private float fixedPosX = 107f;  // Tọa độ X cố định của cụm icon
    [SerializeField] private float fixedPosY = -500f; // Tọa độ Y cố định của cụm icon

    [Header("── Icon Colors ──")]
    // Màu icon khi dao CHƯA được ném (sáng bình thường)
    [SerializeField] private Color activeColor = new Color(1f, 1f, 1f, 1f);
    // Màu icon khi dao ĐÃ được ném (tối đi để biết đã dùng)
    [SerializeField] private Color usedColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);

    // ═══════════════════════════════════════════
    // PRIVATE VARIABLES
    // ═══════════════════════════════════════════

    private List<Image> _knifeIcons = new List<Image>(); // Danh sách toàn bộ icon dao trên HUD
    private int _totalKnives = 0; // Tổng số dao trong màn hiện tại
    private int _knivesThrown = 0; // Số dao đã ném (dùng để biết icon nào cần tối tiếp)

    // ═══════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════

    private void Awake()
    {
        // Singleton: Đảm bảo chỉ tồn tại 1 HUDManager duy nhất trong scene
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ═══════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Khởi tạo hàng icon dao ở đầu mỗi màn chơi.
    ///
    /// Gọi từ: KnifeThrower.SetupLevel()
    ///
    /// [THAM SỐ]
    ///   total       : Tổng số dao trong màn, dùng để tạo đúng số icon.
    ///   knifeSprite : (Tùy chọn) Sprite của con dao người chơi đang dùng.
    ///                 - Nếu truyền vào → icon HUD hiện đúng hình dao đã chọn ở Menu.
    ///                 - Nếu để trống (null) → icon dùng sprite mặc định của prefab,
    ///                   không ảnh hưởng đến các chỗ gọi cũ trong nhóm.
    ///
    /// [CÁCH DÙNG CŨ - vẫn hoạt động bình thường, không cần sửa]
    ///   HUDManager.Instance.SetupKnifeQueue(7);
    ///
    /// [CÁCH DÙNG MỚI - truyền thêm sprite dao đã chọn]
    ///   HUDManager.Instance.SetupKnifeQueue(7, _selectedKnifeSprite);
    /// </summary>
    public void SetupKnifeQueue(int total, Sprite knifeSprite = null)
    {
        _totalKnives = total;
        _knivesThrown = 0;

        // Xóa toàn bộ icon cũ trước khi tạo lại cho màn mới
        ClearIcons();

        for (int i = 0; i < total; i++)
        {
            // Tạo 1 icon dao và đặt vào đúng vị trí trong hàng
            GameObject iconObj = Instantiate(knifeIconPrefab, knifeQueueParent);

            RectTransform rt = iconObj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, -i * iconSpacing); // Icon từ trên xuống dưới

            Image img = iconObj.GetComponent<Image>();
            img.color = activeColor; // Tất cả icon bắt đầu ở trạng thái sáng (chưa ném)

            // [MỚI] Nếu có truyền sprite dao vào → áp lên icon để khớp với dao người chơi đang dùng
            // Nếu knifeSprite == null → giữ nguyên sprite mặc định của prefab, không ảnh hưởng gì
            if (knifeSprite != null)
                img.sprite = knifeSprite;

            _knifeIcons.Add(img);
        }

        // Cố định vị trí X Y của cụm icon, không bị dịch chuyển dù số lượng thay đổi
        RectTransform parentRT = knifeQueueParent.GetComponent<RectTransform>();
        if (parentRT != null)
        {
            parentRT.anchoredPosition = new Vector2(fixedPosX, fixedPosY);
            parentRT.sizeDelta = new Vector2(
                parentRT.sizeDelta.x,
                total * iconSpacing // Chiều cao cụm icon tự động co giãn theo số dao
            );
        }

        Debug.Log($"HUDManager: Setup {total} icons tại X={fixedPosX} Y={fixedPosY}");
    }

    /// <summary>
    /// Gọi mỗi khi người chơi ném 1 dao.
    /// Icon từ TRÊN xuống DƯỚI sẽ chuyển sang màu tối (usedColor) lần lượt.
    /// Dao đầu tiên ném → icon trên cùng (index 0) tối trước.
    ///
    /// Gọi từ: KnifeThrower.TryThrow()
    /// </summary>
    public void OnKnifeThrown()
    {
        // Bảo vệ: Không vượt quá số icon hiện có
        if (_knivesThrown >= _knifeIcons.Count) return;

        _knifeIcons[_knivesThrown].color = usedColor;
        _knivesThrown++;
    }

    // ═══════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Dọn sạch toàn bộ icon dao cũ.
    /// Gọi nội bộ mỗi khi SetupKnifeQueue() được gọi lại (đầu màn mới).
    /// </summary>
    private void ClearIcons()
    {
        // Destroy từng icon đã lưu trong danh sách
        foreach (var icon in _knifeIcons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
        _knifeIcons.Clear();

        // Xóa luôn các child còn sót lại trong parent (phòng trường hợp có object ngoài danh sách)
        foreach (Transform child in knifeQueueParent)
            Destroy(child.gameObject);
    }
}