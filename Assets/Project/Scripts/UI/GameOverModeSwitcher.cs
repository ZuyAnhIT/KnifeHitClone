using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Thêm thư viện quản lý Scene

public class GameOverModeSwitcher : MonoBehaviour
{
    [Header("── UI Màn Thường ──")]
    [Tooltip("Kéo BannerGroup gốc vào đây")]
    [SerializeField] private GameObject normalBannerGroup;
    [SerializeField] private GameObject btnShare;
    [SerializeField] private GameObject btnGetMoreKnives;
    [SerializeField] private TextMeshProUGUI txtTopLeftButton;

    [Header("── UI Màn Challenge ──")]
    [Tooltip("Kéo ChallengeBannerGroup mới tạo vào đây")]
    [SerializeField] private GameObject challengeBannerGroup;
    [SerializeField] private Image challengeRibbonBg;
    [SerializeField] private Image challengeIcon;
    [SerializeField] private TextMeshProUGUI txtChallengeName;
    [SerializeField] private TextMeshProUGUI txtChallengeFailed;

    [Header("── Fix lỗi Điều Hướng (Kéo nút vào đây) ──")]
    [SerializeField] private Button btnHome;      // Kéo Btn_Home góc trái trên vào
    [SerializeField] private Button btnRestart;   // Kéo Btn_Restart màu xanh lá vào

    private void Awake()
    {
        // Gắn sự kiện bằng code. 
        // LƯU Ý: Phải xóa sạch các sự kiện cũ trong ô On Click () của 2 nút này trên Inspector!
        if (btnHome != null)
        {
            btnHome.onClick.AddListener(OnHomeClicked);
        }

        if (btnRestart != null)
        {
            btnRestart.onClick.AddListener(OnRestartClicked);
        }
    }

    private void OnEnable()
    {
        if (GameModeManager.CurrentMode == GameMode.Challenge && GameModeManager.CurrentChallenge != null)
        {
            if (normalBannerGroup != null) normalBannerGroup.SetActive(false);
            if (btnShare != null) btnShare.SetActive(false);
            if (btnGetMoreKnives != null) btnGetMoreKnives.SetActive(false);
            if (txtTopLeftButton != null) txtTopLeftButton.text = "CHALLENGES";

            if (challengeBannerGroup != null) challengeBannerGroup.SetActive(true);

            ChallengeData data = GameModeManager.CurrentChallenge;

            if (challengeRibbonBg != null) challengeRibbonBg.color = data.themeColor;
            if (txtChallengeName != null) txtChallengeName.text = data.challengeName.ToUpper();
            if (txtChallengeFailed != null) txtChallengeFailed.text = $"CHALLENGE {GameModeManager.CurrentChallengeLevel} FAILED";

            if (challengeIcon != null && data.challengeBadgeIcon != null)
                challengeIcon.sprite = data.challengeBadgeIcon;
        }
        else
        {
            if (normalBannerGroup != null) normalBannerGroup.SetActive(true);
            if (btnShare != null) btnShare.SetActive(true);
            if (btnGetMoreKnives != null) btnGetMoreKnives.SetActive(true);
            if (txtTopLeftButton != null) txtTopLeftButton.text = "HOME";

            if (challengeBannerGroup != null) challengeBannerGroup.SetActive(false);
        }
    }

    // ── HÀM XỬ LÝ NÚT BẤM MỚI ──

    private void OnHomeClicked()
    {
        Time.timeScale = 1f; // Đảm bảo game không bị pause

        if (GameModeManager.CurrentMode == GameMode.Challenge)
        {
            // Đi về màn chọn Challenge
            // LƯU Ý: Bạn hãy đổi "SC_Challenge" thành đúng tên Scene chọn thử thách của bạn nhé!
            SceneManager.LoadScene("SC_Challenge");
        }
        else
        {
            // Đi về Menu chính
            // LƯU Ý: Bạn hãy đổi "SC_MenuGame" thành đúng tên Scene Menu của bạn nhé!
            SceneManager.LoadScene("SC_MenuGame");
        }
    }

    private void OnRestartClicked()
    {
        Time.timeScale = 1f;

        // Cách Reset an toàn nhất: Load lại y nguyên Scene hiện tại!
        // GameModeManager sẽ tự nhớ đang chơi Normal hay Challenge để sắp xếp màn chơi
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}