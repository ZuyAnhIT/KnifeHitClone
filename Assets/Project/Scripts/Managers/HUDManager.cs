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
    // [ĐÃ XÓA] fixedPosX → thay bằng 2 giá trị riêng bên dưới
    [SerializeField] private float fixedPosY = -500f; // Tọa độ Y cố định của cụm icon

    // [MỚI] Thay vì 1 posX cố định, nay có 2 posX cho 2 chế độ tay
    [SerializeField] private float posX_RightHand = 107f;  // Tọa độ X khi tay PHẢI (mặc định)
    [SerializeField] private float posX_LeftHand = -107f; // Tọa độ X khi tay TRÁI

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        // Lắng nghe event từ toggle Left Hand trong Settings
        CodeToggle.OnLeftHandChanged += OnLeftHandChanged;
    }

    private void OnDisable()
    {
        CodeToggle.OnLeftHandChanged -= OnLeftHandChanged;
    }

    private void Start()
    {
        // Áp đúng vị trí ngay khi vào scene, không cần chờ người chơi bấm toggle
        bool isLeft = SaveManager.Instance != null && SaveManager.Instance.LeftHandEnabled;
        ApplyQueuePosition(isLeft);
    }

    // ═══════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Khởi tạo hàng icon dao ở đầu mỗi màn chơi.
    /// Gọi từ: KnifeThrower.SetupLevel()
    /// </summary>
    public void SetupKnifeQueue(int total, Sprite knifeSprite = null)
    {
        _totalKnives = total;
        _knivesThrown = 0;

        ClearIcons();

        for (int i = 0; i < total; i++)
        {
            GameObject iconObj = Instantiate(knifeIconPrefab, knifeQueueParent);

            RectTransform rt = iconObj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, -i * iconSpacing);

            Image img = iconObj.GetComponent<Image>();
            img.color = activeColor;

            if (knifeSprite != null)
                img.sprite = knifeSprite;

            _knifeIcons.Add(img);
        }

        // Cập nhật kích thước parent
        RectTransform parentRT = knifeQueueParent.GetComponent<RectTransform>();
        if (parentRT != null)
        {
            parentRT.sizeDelta = new Vector2(
                parentRT.sizeDelta.x,
                total * iconSpacing
            );
        }

        // [MỚI] Đặt vị trí X đúng theo chế độ tay hiện tại
        // (SetupLevel gọi lại mỗi màn → vị trí phải luôn đúng)
        bool isLeft = SaveManager.Instance != null && SaveManager.Instance.LeftHandEnabled;
        ApplyQueuePosition(isLeft);

        Debug.Log($"HUDManager: Setup {total} icons | LeftHand={isLeft}");
    }

    /// <summary>
    /// Gọi mỗi khi người chơi ném 1 dao.
    /// Icon từ trên xuống dưới sẽ chuyển sang màu tối lần lượt.
    /// Gọi từ: KnifeThrower.TryThrow()
    /// </summary>
    public void OnKnifeThrown()
    {
        if (_knivesThrown >= _knifeIcons.Count) return;

        _knifeIcons[_knivesThrown].color = usedColor;
        _knivesThrown++;
    }

    // ═══════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Callback từ CodeToggle.OnLeftHandChanged.
    /// Tự động gọi khi người chơi bấm toggle Left Hand trong Settings.
    /// </summary>
    private void OnLeftHandChanged(bool isLeft)
    {
        ApplyQueuePosition(isLeft);
    }

    /// <summary>
    /// Dịch knifeQueueParent sang đúng vị trí X theo chế độ tay.
    /// isLeft = true  → posX_LeftHand  (bên trái màn hình)
    /// isLeft = false → posX_RightHand (bên phải màn hình)
    /// Y luôn giữ nguyên = fixedPosY
    /// </summary>
    private void ApplyQueuePosition(bool isLeft)
    {
        if (knifeQueueParent == null) return;

        RectTransform parentRT = knifeQueueParent.GetComponent<RectTransform>();
        if (parentRT == null) return;

        float targetX = isLeft ? posX_LeftHand : posX_RightHand;
        parentRT.anchoredPosition = new Vector2(targetX, fixedPosY);

        Debug.Log($"HUDManager: KnifeQueue → {(isLeft ? "TRÁI" : "PHẢI")} (x={targetX})");
    }

    /// <summary>
    /// Dọn sạch toàn bộ icon dao cũ.
    /// Gọi mỗi khi SetupKnifeQueue() được gọi lại (đầu màn mới).
    /// </summary>
    private void ClearIcons()
    {
        foreach (var icon in _knifeIcons)
            if (icon != null) Destroy(icon.gameObject);

        _knifeIcons.Clear();

        foreach (Transform child in knifeQueueParent)
            Destroy(child.gameObject);
    }
}