using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChallengeCompleteUI : MonoBehaviour
{
    [Header("── Tham chiếu Giao diện ──")]
    [SerializeField] private Image imgIcon;                        // Img Icon
    [SerializeField] private TextMeshProUGUI txtNameChallenge;     // Text_Name_Challenge (Vd: MONSTERS)
    [SerializeField] private Image ribbonBackground;               // Background (Dải băng chứa chữ)
    [SerializeField] private TextMeshProUGUI txtCompleted;         // Text_Completed (Vd: CHALLENGE 2 COMPLETED!)

    [Header("── Nút bấm ──")]
    [SerializeField] private Button btnMenu;                       // Button_Menu
    [SerializeField] private Button btnNext;                       // Button_Next

    private void Awake()
    {
        btnMenu.onClick.AddListener(OnClickMenu);
        btnNext.onClick.AddListener(OnClickNext);
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);

        if (GameModeManager.CurrentMode == GameMode.Challenge && GameModeManager.CurrentChallenge != null)
        {
            ChallengeData data = GameModeManager.CurrentChallenge;

            // Lấy số level VỪA VƯỢT QUA
            int levelJustCompleted = GameModeManager.CurrentChallengeLevel;

            // 1. Cập nhật Text
            if (txtNameChallenge != null) txtNameChallenge.text = data.challengeName.ToUpper();
            if (txtCompleted != null) txtCompleted.text = $"CHALLENGE {levelJustCompleted}\nCOMPLETED!";

            // 2. Đổi màu dải băng (Background) theo Theme của màn
            if (ribbonBackground != null) ribbonBackground.color = data.themeColor;

            // 3. Đổi Icon (Tạm lấy ảnh gỗ thường, bạn có thể tạo thêm public Sprite icon vào ChallengeData sau này nếu muốn)
            if (imgIcon != null && data.normalLogSprite != null)
                imgIcon.sprite = data.normalLogSprite;

            // 4. LƯU TIẾN ĐỘ NGAY KHI HIỆN BẢNG
            GameModeManager.CompleteCurrentAndMoveToNext();
        }
    }

    private void OnClickMenu()
    {
        gameObject.SetActive(false);
        // Gọi SceneTransition để về Menu
        SceneTransition transition = FindObjectOfType<SceneTransition>();
        if (transition != null) transition.GoToMenu();
    }

    private void OnClickNext()
    {
        gameObject.SetActive(false);
        // Báo GameManager tải vòng tiếp theo
        GameManager.Instance.StartNextChallengeFromUI();
    }
}