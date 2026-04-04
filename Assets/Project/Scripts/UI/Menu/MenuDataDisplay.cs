using UnityEngine;
using TMPro;

/// <summary>
/// Hiển thị dữ liệu đã lưu lên màn Menu:
/// Best Stage, Best Score, Total Apple
/// </summary>
public class MenuDataDisplay : MonoBehaviour
{
    [Header("── Text References ──")]
    [SerializeField] private TextMeshProUGUI txtStage;
    [SerializeField] private TextMeshProUGUI txtScore;
    [SerializeField] private TextMeshProUGUI txtApple;
    public GameObject panelShopKnife;
    public GameObject panelMenu;

    [Header("── Knife Display ──")]
    [SerializeField] private UnityEngine.UI.Image imgMainKnife; // Kéo Img_MainKnife vào đây
    [SerializeField] private KnifeDatabase knifeDatabase; // Kéo KnifeDatabase vào đây

    private void Start()
    {
        RefreshDisplay();
        UpdateSelectedKnifeImage();
    }

    private void OnEnable()
    {
        // Refresh mỗi khi màn menu được bật
        RefreshDisplay();

        // Cập nhật hình ảnh con dao đã chọn
        UpdateSelectedKnifeImage();

        if (PlayerPrefs.GetInt("OpenShopKnife", 0) == 1)
        {
            panelShopKnife.SetActive(true);
            panelMenu.SetActive(false);
            // reset để lần sau không tự mở nữa
            PlayerPrefs.SetInt("OpenShopKnife", 0);
        }
    }

    public void RefreshDisplay()
    {
        if (SaveManager.Instance == null) return;

        // Stage cao nhất
        if (txtStage != null)
            txtStage.text = $"STAGE {SaveManager.Instance.BestStage}";

        // Score cao nhất  
        if (txtScore != null)
            txtScore.text = $"SCORE {SaveManager.Instance.BestScore}";

        // Tổng táo 
        if (txtApple != null)
            txtApple.text = SaveManager.Instance.TotalApple.ToString();
    }

    /// <summary>
    /// Cập nhật hình ảnh con dao đã chọn lên màn hình chính
    /// </summary>
    private void UpdateSelectedKnifeImage()
    {
        if (imgMainKnife == null || knifeDatabase == null) return;
        if (SaveManager.Instance == null) return;

        int savedPage = SaveManager.Instance.SelectedKnifePage;
        int savedSlot = SaveManager.Instance.SelectedKnifeSlot;

        Sprite selectedSprite = knifeDatabase.GetSprite(savedPage, savedSlot);
        if (selectedSprite != null)
        {
            imgMainKnife.sprite = selectedSprite;
            imgMainKnife.SetNativeSize(); // Giữ tỷ lệ gốc của ảnh
            Debug.Log($"MenuDataDisplay: Đã cập nhật dao → Page={savedPage}, Slot={savedSlot}");
        }
    }
}