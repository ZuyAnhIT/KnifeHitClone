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

    private void Start()
    {
        RefreshDisplay();
    }

    private void OnEnable()
    {
        // Refresh mỗi khi màn menu được bật
        RefreshDisplay();
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
            txtApple.text = SaveManager.Instance
                                .TotalApple.ToString();
    }
}