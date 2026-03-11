using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Quản lý HUD_KnifeQueue
/// Dao phi đi → icon chuyển tối
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("── Knife Queue Icons ──")]
    [SerializeField] private List<Image> knifeIcons;
    [SerializeField] private Sprite knifeIconActive;   // Dao sáng
    [SerializeField] private Sprite knifeIconUsed;     // Dao tối

    [Header("── Colors ──")]
    [SerializeField]
    private Color activeColor =
        new Color(1f, 1f, 1f, 1f);        // Trắng đậm
    [SerializeField]
    private Color usedColor =
        new Color(0.3f, 0.3f, 0.3f, 0.8f); // Tối mờ

    private int _totalKnives;
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

    /// <summary>
    /// Setup icons khi bắt đầu màn
    /// </summary>
    public void SetupKnifeQueue(int total)
    {
        _totalKnives = total;
        _knivesThrown = 0;

        // Ẩn tất cả icon trước
        foreach (var icon in knifeIcons)
            icon.gameObject.SetActive(false);

        // Hiện đúng số icon cần thiết
        for (int i = 0; i < total && i < knifeIcons.Count; i++)
        {
            knifeIcons[i].gameObject.SetActive(true);
            SetIconActive(knifeIcons[i], true);
        }
    }

    /// <summary>
    /// Gọi khi phi 1 dao
    /// </summary>
    public void OnKnifeThrown()
    {
        if (_knivesThrown >= knifeIcons.Count) return;

        // Icon từ dưới lên → dao dưới cùng phi trước
        int iconIndex = _totalKnives - 1 - _knivesThrown;
        if (iconIndex >= 0 && iconIndex < knifeIcons.Count)
        {
            SetIconActive(knifeIcons[iconIndex], false);
        }

        _knivesThrown++;
    }

    private void SetIconActive(Image icon, bool active)
    {
        if (active)
        {
            icon.color = activeColor;
            if (knifeIconActive != null)
                icon.sprite = knifeIconActive;
        }
        else
        {
            icon.color = usedColor;
            if (knifeIconUsed != null)
                icon.sprite = knifeIconUsed;
        }
    }
}