using UnityEngine;
using System.Collections;

/// <summary>
/// Hiệu ứng dao đâm vào táo:
/// Đốm nhỏ → Ngôi sao + Chữ V → Mờ biến mất
/// </summary>
public class AppleHitEffect : MonoBehaviour
{
    [Header("── Sprites ──")]
    [SerializeField] private Sprite spriteGlow;    // shop_particles_woodoo_knives_0
    [SerializeField] private Sprite spriteStar;    // shop_particles_big_0
    [SerializeField] private Sprite spriteV;       // shop_particles_big_2

    [Header("── Timing ──")]
    [SerializeField] private float phase1Duration = 0.015f; // Đốm nhỏ
    [SerializeField] private float phase2Duration = 0.01f; // Nở to
    [SerializeField] private float phase3Duration = 0.015f; // Mờ biến mất

    [Header("── Scale ──")]
    [SerializeField] private float glowStartScale = 0.2f;
    [SerializeField] private float glowEndScale = 1f;
    [SerializeField] private float starMaxScale = 1.5f;
    [SerializeField] private float vMaxScale = 1.5f;

    public void Play(Vector3 position)
    {
        GameObject obj = new GameObject("AppleHitEffect");
        obj.transform.position = position;
        StartCoroutine(PlayRoutine(obj));
    }

    private IEnumerator PlayRoutine(GameObject root)
    {
        // Tạo các thành phần
        GameObject glowObj = CreatePart(root, spriteGlow, 8);
        GameObject starObj = CreatePart(root, spriteStar, 9);
        GameObject vObj1 = CreatePart(root, spriteV, 9);
        GameObject vObj2 = CreatePart(root, spriteV, 9);

        SpriteRenderer glowSr = glowObj.GetComponent<SpriteRenderer>();
        SpriteRenderer starSr = starObj.GetComponent<SpriteRenderer>();
        SpriteRenderer v1Sr = vObj1.GetComponent<SpriteRenderer>();
        SpriteRenderer v2Sr = vObj2.GetComponent<SpriteRenderer>();

        vObj1.transform.localRotation =
            Quaternion.Euler(0f, 0f, 45f);
        vObj2.transform.localRotation =
            Quaternion.Euler(0f, 0f, -45f);

        // ── FRAME 1 — Đốm nhỏ ──
        glowObj.transform.localScale = Vector3.one * 0.5f;
        starObj.SetActive(false);
        vObj1.SetActive(false);
        vObj2.SetActive(false);
        yield return null; // Chờ 1 frame

        // ── FRAME 2 — Nở to ──
        glowObj.transform.localScale = Vector3.one * glowEndScale;
        starObj.SetActive(true);
        starObj.transform.localScale = Vector3.one * starMaxScale;
        vObj1.SetActive(true);
        vObj2.SetActive(true);
        vObj1.transform.localScale =
            new Vector3(vMaxScale, vMaxScale * 0.5f, 1f);
        vObj2.transform.localScale =
            new Vector3(vMaxScale, vMaxScale * 0.5f, 1f);
        yield return null; // Chờ 1 frame

        // ── FRAME 3 — Mờ 50% ──
        SetAlpha(glowSr, 0.5f);
        SetAlpha(starSr, 0.5f);
        SetAlpha(v1Sr, 0.5f);
        SetAlpha(v2Sr, 0.5f);
        starObj.transform.localScale =
            Vector3.one * starMaxScale * 1.2f;
        yield return null; // Chờ 1 frame

        // ── FRAME 4 — Biến mất ──
        Destroy(root);
    }

    // ── Helpers ─────────────────────────────
    private GameObject CreatePart(
        GameObject parent,
        Sprite sprite,
        int order)
    {
        GameObject obj = new GameObject("Part");
        obj.transform.SetParent(parent.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;

        SpriteRenderer sr =
            obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;

        return obj;
    }

    private void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr == null) return;
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }
}