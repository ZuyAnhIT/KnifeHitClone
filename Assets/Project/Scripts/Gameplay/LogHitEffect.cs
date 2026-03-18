using UnityEngine;
using System.Collections;

public class LogHitEffect : MonoBehaviour
{
    [Header("── Scale Punch ──")]
    [SerializeField] private float punchAmount = 0.06f;
    [SerializeField] private float punchDuration = 0.08f;
    [SerializeField] private float returnDuration = 0.10f;

    [Header("── Overlay Mờ ──")]
    [SerializeField] private SpriteRenderer overlayRenderer;
    [SerializeField] private float overlayMaxAlpha = 0.35f;
    [SerializeField] private float overlayInDuration = 0.05f;
    [SerializeField] private float overlayOutDuration = 0.15f;

    private Vector3 _originalScale;
    private Coroutine _scaleRoutine;
    private Coroutine _overlayRoutine;

    private void Awake()
    {
        _originalScale = transform.localScale;

        // Đảm bảo overlay bắt đầu trong suốt
        if (overlayRenderer != null)
        {
            Color c = overlayRenderer.color;
            c.a = 0f;
            overlayRenderer.color = c;
        }
    }

    public void PlayHitEffect()
    {
        if (_scaleRoutine != null)
            StopCoroutine(_scaleRoutine);
        if (_overlayRoutine != null)
            StopCoroutine(_overlayRoutine);

        _scaleRoutine = StartCoroutine(ScalePunchRoutine());
        _overlayRoutine = StartCoroutine(OverlayRoutine());
    }

    // ── Scale thu phóng ────────────────────────
    private IEnumerator ScalePunchRoutine()
    {
        transform.localScale = _originalScale;
        Vector3 small = _originalScale * (1f - punchAmount);

        float t = 0f;
        while (t < punchDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(
                _originalScale, small,
                t / punchDuration);
            yield return null;
        }

        t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(
                small, _originalScale,
                t / returnDuration);
            yield return null;
        }

        transform.localScale = _originalScale;
    }

    // ── Overlay phủ lên Log ────────────────────
    private IEnumerator OverlayRoutine()
    {
        if (overlayRenderer == null) yield break;

        Color transparent = new Color(1f, 1f, 1f, 0f);
        Color visible = new Color(1f, 1f, 1f, overlayMaxAlpha);

        // Hiện overlay nhanh
        float t = 0f;
        while (t < overlayInDuration)
        {
            t += Time.deltaTime;
            overlayRenderer.color = Color.Lerp(
                transparent, visible,
                t / overlayInDuration);
            yield return null;
        }

        // Mờ dần về trong suốt
        t = 0f;
        while (t < overlayOutDuration)
        {
            t += Time.deltaTime;
            overlayRenderer.color = Color.Lerp(
                visible, transparent,
                t / overlayOutDuration);
            yield return null;
        }

        overlayRenderer.color = transparent;
    }
}