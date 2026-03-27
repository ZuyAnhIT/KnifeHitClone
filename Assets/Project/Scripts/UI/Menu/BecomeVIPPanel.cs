using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BecomeVIPPanel : MonoBehaviour
{
    RectTransform panel;
    float panelHeight;
    bool isAnimating = false;
    bool isInitialized = false;

    [Header("Panels cần tắt khi VIP mở")]
    [SerializeField] GameObject[] panelsToHide;

    // ── KHÔNG dùng Awake/Start nữa ──
    // Vì object đang inactive, Awake/Start sẽ không chạy

    void Init()
    {
        if (isInitialized) return;

        panel = GetComponent<RectTransform>();

        // Bật lên 1 tích tắc để Unity tính layout
        gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
        panelHeight = panel.rect.height;

        Debug.Log($"[VIP] panelHeight = {panelHeight}");

        isInitialized = true;
    }

    public void Open()
    {
        if (isAnimating) return;

        Init(); // ← init ngay lần đầu gọi

        HideOtherPanels();

        panel.anchoredPosition = new Vector2(0, -panelHeight);
        gameObject.SetActive(true);
        StartCoroutine(Slide(targetY: 0, opening: true, duration: 0.4f));
    }

    public void Close()
    {
        if (isAnimating) return;
        StartCoroutine(Slide(targetY: -panelHeight, opening: false, duration: 0.35f));
    }

    void HideOtherPanels()
    {
        foreach (var p in panelsToHide)
            if (p != null) p.SetActive(false);
    }

    void ShowOtherPanels()
    {
        foreach (var p in panelsToHide)
            if (p != null) p.SetActive(true);
    }

    IEnumerator Slide(float targetY, bool opening, float duration)
    {
        isAnimating = true;

        float startY = panel.anchoredPosition.y;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float ease = opening
                ? 1f - Mathf.Pow(1f - t, 3f)
                : t * t * t;

            panel.anchoredPosition = new Vector2(0,
                Mathf.Lerp(startY, targetY, ease));

            yield return null;
        }

        panel.anchoredPosition = new Vector2(0, targetY);

        if (!opening)
        {
            ShowOtherPanels();
            gameObject.SetActive(false);
        }

        isAnimating = false;
    }
}