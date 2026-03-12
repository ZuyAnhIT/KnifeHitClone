using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("── Knife Queue ──")]

    [SerializeField] private Transform knifeQueueParent; // HUD_KnifeQueue
    [SerializeField] private GameObject knifeIconPrefab;  // Prefab 1 icon dao
    [SerializeField] private float iconSpacing = 48f; // Khoảng cách icon

    [Header("── Position cố định ──")]
    [SerializeField] private float fixedPosX = 107f;  // Cố định X
    [SerializeField] private float fixedPosY = -500f; // Cố định Y

    [Header("── Icon Colors ──")]
    [SerializeField]
    private Color activeColor =
        new Color(1f, 1f, 1f, 1f);
    [SerializeField]
    private Color usedColor =
        new Color(0.25f, 0.25f, 0.25f, 0.8f);

    // ── Private ──
    private List<Image> _knifeIcons = new List<Image>();
    private int _totalKnives = 0;
    private int _knivesThrown = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ═══════════════════════════════════════════
    // PUBLIC
    // ═══════════════════════════════════════════

    /// <summary>
    /// Gọi mỗi đầu màn để tạo đúng số icon
    /// </summary>
    public void SetupKnifeQueue(int total)
    {
        _totalKnives = total;
        _knivesThrown = 0;

        ClearIcons();

        for (int i = 0; i < total; i++)
        {
            GameObject iconObj = Instantiate(
                knifeIconPrefab,
                knifeQueueParent
            );

            RectTransform rt = iconObj.GetComponent<RectTransform>();
            // Icon từ trên xuống dưới
            rt.anchoredPosition = new Vector2(0f, -i * iconSpacing);

            Image img = iconObj.GetComponent<Image>();
            img.color = activeColor;
            _knifeIcons.Add(img);
        }

        // Cố định vị trí X Y không thay đổi
        RectTransform parentRT =
            knifeQueueParent.GetComponent<RectTransform>();
        if (parentRT != null)
        {
            parentRT.anchoredPosition = new Vector2(
                fixedPosX,
                fixedPosY
            );
            parentRT.sizeDelta = new Vector2(
                parentRT.sizeDelta.x,
                total * iconSpacing
            );
        }

        Debug.Log($"HUDManager: Setup {total} icons tại " +
                  $"X={fixedPosX} Y={fixedPosY}");
    }
    /// <summary>
    /// Icon từ TRÊN xuống DƯỚI chuyển tối
    /// Dao đầu tiên phi → icon trên cùng tối trước
    /// </summary>
    public void OnKnifeThrown()
    {
        if (_knivesThrown >= _knifeIcons.Count) return;

        // ĐÚNG: Icon trên cùng tối trước (i=0 tối trước)
        _knifeIcons[_knivesThrown].color = usedColor;

        _knivesThrown++;
    }

    // ═══════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════
    private void ClearIcons()
    {
        // Xóa tất cả icon cũ
        foreach (var icon in _knifeIcons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
        _knifeIcons.Clear();

        // Xóa luôn các child còn sót
        foreach (Transform child in knifeQueueParent)
            Destroy(child.gameObject);
    }
}