using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Dòng này giúp Unity tự hiểu object này phải là một Nút bấm
[RequireComponent(typeof(Button))]
public class ChallengeButton : MonoBehaviour
{
    [Header("── Dữ liệu Thử Thách ──")]
    [Tooltip("Kéo file ChallengeData (từ Bước 1) tương ứng vào đây")]
    [SerializeField] private ChallengeData myChallengeData;

    private Button _btnPlay;

    private void Awake()
    {
        // Tự động lấy Component Button đang gắn trên chính object MONSTERS này
        _btnPlay = GetComponent<Button>();

        if (_btnPlay != null)
        {
            _btnPlay.onClick.AddListener(OnPlayClick);
        }
    }

    private void OnPlayClick()
    {
        if (myChallengeData == null)
        {
            Debug.LogError($"Bạn chưa kéo file ChallengeData vào thẻ {gameObject.name}!");
            return;
        }

        // 1. Ghi dữ liệu vào Trạm trung chuyển
        GameModeManager.SetChallengeMode(myChallengeData);

        // 2. Load sang màn Gameplay
        SceneManager.LoadScene("SC_Gameplay");
    }
}