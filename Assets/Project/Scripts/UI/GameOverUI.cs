using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("── Panel ──")]
    [SerializeField] private GameObject panel;

    [Header("── TopBar ──")]
    [SerializeField] private TextMeshProUGUI txtAppleCount;
    [SerializeField] private Button btnHome;

    [Header("── Banner ──")]
    [SerializeField] private TextMeshProUGUI txtKnifeCount;
    [SerializeField] private TextMeshProUGUI txtStageInfo;

    [Header("── Buttons ──")]
    [SerializeField] private Button btnRestart;
    [SerializeField] private Button btnGetMoreKnives;

    [Header("── Objects cần ẩn khi Game Over ──")]
    [SerializeField] private GameObject log;
    [SerializeField] private GameObject knifeSpawnPoint;
    [SerializeField] private GameObject hudKnifeThrown;
    [SerializeField] private GameObject hudAppleGroup;
    [SerializeField] private GameObject hudKnifeQueue;
    [SerializeField] private GameObject hudLivesGroup;
    [SerializeField] private GameObject hudStageProgress;
    [SerializeField] private GameObject hudStageName;
    [SerializeField] private GameObject txtBossName;
    [SerializeField] private GameObject bossIconCenter;
    [SerializeField] private GameObject flashScreen;

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += ShowPanel;

        btnRestart.onClick.AddListener(OnRestartClick);
        btnHome.onClick.AddListener(OnHomeClick);

        panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= ShowPanel;
    }

    private void ShowPanel()
    {
        SetGameplayVisible(false);
        panel.SetActive(true);
        UpdatePanelInfo();
    }

    private void HidePanel()
    {
        panel.SetActive(false);
        SetGameplayVisible(true);
    }

    private void UpdatePanelInfo()
    {
        if (txtKnifeCount != null &&
            ScoreManager.Instance != null)
            txtKnifeCount.text =
                ScoreManager.Instance.KnifeThrown.ToString();

        if (txtStageInfo != null &&
            GameManager.Instance != null)
            txtStageInfo.text =
                "STAGE " +
                GameManager.Instance.CurrentLevel.stageNumber;

        if (txtAppleCount != null &&
            SaveManager.Instance != null)
            txtAppleCount.text =
                SaveManager.Instance.TotalApple.ToString();
    }

    private void SetGameplayVisible(bool visible)
    {
        if (log != null) log.SetActive(visible);
        if (knifeSpawnPoint != null) knifeSpawnPoint.SetActive(visible);
        if (hudKnifeThrown != null) hudKnifeThrown.SetActive(visible);
        if (hudAppleGroup != null) hudAppleGroup.SetActive(visible);
        if (hudKnifeQueue != null) hudKnifeQueue.SetActive(visible);
        if (hudLivesGroup != null) hudLivesGroup.SetActive(visible);
        if (hudStageProgress != null) hudStageProgress.SetActive(visible);
        if (hudStageName != null) hudStageName.SetActive(visible);
        if (txtBossName != null) txtBossName.SetActive(visible);
        if (bossIconCenter != null) bossIconCenter.SetActive(visible);
        if (flashScreen != null) flashScreen.SetActive(visible);

        if (!visible) ClearAllStuckKnives();
    }

    // ── Button Events ──────────────────────────
    private void OnRestartClick()
    {
        HidePanel();

        // Thêm debug để kiểm tra
        Debug.Log("Restart clicked!");

        if (GameManager.Instance == null)
            Debug.LogError("GameManager.Instance = NULL!");
        else
            GameManager.Instance.RestartCurrentStage();
    }
    private void OnHomeClick()
    {
        // Dùng SceneManager trực tiếp
        // Không phụ thuộc bất kỳ Singleton nào
        SceneManager.LoadScene("SC_MenuGame");
    }

    private void ClearAllStuckKnives()
    {
        // Cách 1: Tìm KnifeThrower tự động
        KnifeThrower kt = FindObjectOfType<KnifeThrower>();
        if (kt != null)
            kt.ClearAllKnivesPublic();

        // Xóa dao StuckKnife còn sót
        GameObject[] stuckKnives =
            GameObject.FindGameObjectsWithTag("StuckKnife");
        foreach (var knife in stuckKnives)
            Destroy(knife);

        // Xóa dao đang bay
        GameObject[] flyingKnives =
            GameObject.FindGameObjectsWithTag("Knife");
        foreach (var knife in flyingKnives)
            Destroy(knife);
    }
}