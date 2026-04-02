using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class ContinueUI : MonoBehaviour
{
    [Header("── UI References ──")]
    [SerializeField] private TextMeshProUGUI txtCurrentScore;
    [SerializeField] private RectTransform knifeMoving;
    [SerializeField] private RectTransform ringObject;

    [Header("── Buttons ──")]
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnShop;
    [SerializeField] private Button btnNoThanks;

    [Header("── Animation & Timing ──")]
    [Tooltip("Tọa độ X, Y lúc dao RÚT RA NGOÀI chờ đút vào")]
    [SerializeField] private Vector2 startPos = new Vector2(-55f, 75f);
    [Tooltip("Tọa độ X, Y lúc dao ĐÃ CẮM NGẬP VÀO VỎ")]
    [SerializeField] private Vector2 endPos = new Vector2(5f, 16f);
    [SerializeField] private float countdownTime = 6f; // Đã tăng lên 6 giây

    [Header("── Audio ──")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tickSound;

    private bool _isActionTaken = false;

    private void Start()
    {
        btnContinue.onClick.AddListener(OnClickContinue);
        btnShop.onClick.AddListener(OnClickShop);
        btnNoThanks.onClick.AddListener(OnClickNoThanks);
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
        _isActionTaken = false;

        if (txtCurrentScore != null && ScoreManager.Instance != null)
            txtCurrentScore.text = ScoreManager.Instance.KnifeThrown.ToString();

        // Ẩn No Thanks lúc đầu
        btnNoThanks.gameObject.SetActive(false);

        // Set dao về đúng vị trí lòi ra ngoài (sẵn sàng trượt chéo)
        knifeMoving.anchoredPosition = startPos;

        StartCoroutine(CountdownRoutine());
        StartCoroutine(RingPulseRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        float elapsed = 0f;
        float lastTick = 0f;

        while (elapsed < countdownTime)
        {
            if (_isActionTaken) yield break;

            elapsed += Time.deltaTime;
            float timeRemaining = countdownTime - elapsed;

            // HIỆN NÚT NO THANKS Ở GIÂY THỨ 3 (Khi thời gian còn <= 3 giây)
            if (timeRemaining <= 3f && !btnNoThanks.gameObject.activeSelf)
            {
                btnNoThanks.gameObject.SetActive(true);
            }

            // TRƯỢT DAO THEO ĐƯỜNG CHÉO (Nội suy giữa 2 điểm X,Y)
            knifeMoving.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / countdownTime);

            if (elapsed - lastTick >= 1f)
            {
                if (audioSource != null && tickSound != null)
                    audioSource.PlayOneShot(tickSound);
                lastTick = elapsed;
            }

            yield return null;
        }

        HandleGameOver();
    }

    private IEnumerator RingPulseRoutine()
    {
        Vector3 originalScale = Vector3.one;
        while (!_isActionTaken)
        {
            float scale = 1f + Mathf.PingPong(Time.time * 2f, 0.06f);
            if (ringObject != null)
                ringObject.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        if (ringObject != null) ringObject.localScale = originalScale;
    }

    private void OnClickContinue()
    {
        if (_isActionTaken) return;
        _isActionTaken = true;

        gameObject.SetActive(false);
        GameManager.Instance.ReviveGame();
    }

    private void OnClickShop()
    {
        if (_isActionTaken) return;
        _isActionTaken = true;
        SceneManager.LoadScene("SC_MenuStore");
    }

    private void OnClickNoThanks()
    {
        if (_isActionTaken) return;
        _isActionTaken = true;
        HandleGameOver();
    }

    private void HandleGameOver()
    {
        gameObject.SetActive(false);
        GameManager.Instance.ShowFinalGameOver();
    }
}