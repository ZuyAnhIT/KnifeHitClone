using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LogoController : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    [Header("── Thời gian (Giây) ──")]
    public float fadeInTime = 1.0f; // Thời gian logo từ từ hiện lên
    public float stayTime = 1.5f;   // Thời gian logo giữ nguyên trên màn hình

    [Header("── Tên Scene tiếp theo ──")]
    public string nextScene = "SC_MenuGame"; // Đổi thành tên Scene Menu của bạn

    private void Awake()
    {
        // Ép alpha = 0 ngay từ đầu để tránh bị chớp ảnh lúc vừa bật game
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void Start()
    {
        StartCoroutine(PlayLogo());
    }

    private IEnumerator PlayLogo()
    {
        // 1. CHỐNG LAG: Đợi 0.1s để Unity load xong tài nguyên đầu game
        yield return new WaitForSeconds(0.1f);

        if (canvasGroup == null) yield break;

        // 2. FADE IN (Hiện dần lên)
        float t = 0;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeInTime);
            yield return null;
        }
        canvasGroup.alpha = 1f; // Đảm bảo rõ 100%

        // 3. CHỜ (Giữ nguyên logo trên màn hình)
        yield return new WaitForSeconds(stayTime);

        // 4. CHUYỂN THẲNG SANG MENU (Không cần mờ đi nữa)
        SceneManager.LoadScene(nextScene);
    }
}