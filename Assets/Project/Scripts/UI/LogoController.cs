using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LogoController : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeInTime = 1.5f;
    public float stayTime = 1f;
    public float fadeOutTime = 1.5f;
    public string nextScene = "MainMenu";

    void Start()
    {
        StartCoroutine(PlayLogo());
    }

    IEnumerator PlayLogo()
    {
        // Fade In
        canvasGroup.alpha = 0;
        float t = 0;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = t / fadeInTime;
            yield return null;
        }

        // Giữ nguyên logo
        yield return new WaitForSeconds(stayTime);

        // Fade Out
        t = 0;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1 - (t / fadeOutTime);
            yield return null;
        }

        // Chuyển scene
        SceneManager.LoadScene(nextScene);
    }
}