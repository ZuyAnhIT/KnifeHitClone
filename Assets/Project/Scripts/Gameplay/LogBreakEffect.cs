using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LogBreakEffect : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // INSPECTOR
    // ═══════════════════════════════════════════
    [Header("── Mảnh vỡ (Màn thường) ──")]
    [SerializeField] private List<GameObject> breakPiecePrefabs;
    [SerializeField] private int pieceCount = 8;
    [SerializeField] private float pieceSpeed = 8f;
    [SerializeField] private float pieceDuration = 0.5f;

    [Header("── Flash trắng ──")]
    [SerializeField] private SpriteRenderer flashOverlay;
    [SerializeField] private float flashInDuration = 0.08f;
    [SerializeField] private float flashOutDuration = 0.25f;

    [Header("── Timing ──")]
    [SerializeField] private float delayBeforeBreak = 0.05f;
    [SerializeField] private float delayBeforeNew = 0.6f;

    [Header("── Extra Effects ──")]
    [SerializeField] private LogBreakRingEffect ringEffect;
    [SerializeField] private BossExplodeEffect bossExplodeEffect;

    [Header("── Âm thanh ──")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip logBreakSound;

    // ── Private ──
    private SpriteRenderer _sr;
    private bool _isBossStage = false;
    private Color _bossColor = Color.yellow;

    // ── Event ──
    public System.Action OnBreakComplete;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    // ═══════════════════════════════════════════
    // PUBLIC
    // ═══════════════════════════════════════════
    public void PlayBreak(List<GameObject> stuckKnives)
    {
        StartCoroutine(BreakSequence(stuckKnives));
    }

    /// <summary>
    /// Gọi từ GameManager khi bắt đầu boss/normal stage
    /// </summary>
    public void SetBossMode(bool isBoss, Color color)
    {
        _isBossStage = isBoss;
        _bossColor = color;
        Debug.Log($"LogBreakEffect: BossMode={isBoss} | Color={color}");
    }

    // ═══════════════════════════════════════════
    // SEQUENCE
    // ═══════════════════════════════════════════
    private IEnumerator BreakSequence(
        List<GameObject> stuckKnives)
    {
        yield return new WaitForSeconds(delayBeforeBreak);

        if (audioSource != null && logBreakSound != null)
        {
            audioSource.PlayOneShot(logBreakSound);
        }

        // Gom tất cả item: dao phi + dao sẵn + táo
        List<GameObject> allItems =
            GatherAllLogItems(stuckKnives);

        // Tách tất cả ra khỏi Log
        DetachAllItems(allItems);

        // Flash trắng
        yield return StartCoroutine(FlashRoutine());

        // Ẩn Log
        if (_sr != null)
            _sr.enabled = false;

        // ── Vòng tròn TẤT CẢ màn ──
        if (ringEffect != null)
            ringEffect.Play(transform.position);
        else
            Debug.LogWarning("LogBreakEffect: ringEffect = NULL!");

        if (_isBossStage)
        {
            // ── BOSS: Chấm màu nổ tứ phía ──
            if (bossExplodeEffect != null)
            {
                Debug.Log($"LogBreakEffect: " +
                          $"Calling BossExplode | " +
                          $"Color={_bossColor}");
                bossExplodeEffect.Play(
                    transform.position, _bossColor);
            }
            else
                Debug.LogError("LogBreakEffect: " +
                               "bossExplodeEffect = NULL!");
        }
        else
        {
            // ── THƯỜNG: Mảnh vỡ gỗ ──
            SpawnBreakPieces();
        }

        // Tất cả items bay tứ phía
        LaunchAllItems(allItems);

        yield return new WaitForSeconds(delayBeforeNew);

        // Hiện lại Log cho màn tiếp
        if (_sr != null)
            _sr.enabled = true;

        OnBreakComplete?.Invoke();
    }

    // ═══════════════════════════════════════════
    // FLASH TRẮNG
    // ═══════════════════════════════════════════
    private IEnumerator FlashRoutine()
    {
        if (flashOverlay == null) yield break;

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color white = new Color(1f, 1f, 1f, 0.85f);

        float t = 0f;
        while (t < flashInDuration)
        {
            t += Time.deltaTime;
            flashOverlay.color = Color.Lerp(
                clear, white, t / flashInDuration);
            yield return null;
        }

        t = 0f;
        while (t < flashOutDuration)
        {
            t += Time.deltaTime;
            flashOverlay.color = Color.Lerp(
                white, clear, t / flashOutDuration);
            yield return null;
        }

        flashOverlay.color = clear;
    }

    // ═══════════════════════════════════════════
    // SPAWN MẢNH VỠ (Chỉ màn thường)
    // ═══════════════════════════════════════════
    private void SpawnBreakPieces()
    {
        if (breakPiecePrefabs == null ||
            breakPiecePrefabs.Count == 0)
        {
            Debug.LogWarning("LogBreakEffect: " +
                             "Không có breakPiecePrefabs!");
            return;
        }

        int total = breakPiecePrefabs.Count;
        float angleStep = 360f / total;

        for (int i = 0; i < total; i++)
        {
            float angle = i * angleStep
                           + Random.Range(-20f, 20f);
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(
                                Mathf.Cos(rad),
                                Mathf.Sin(rad));

            GameObject prefab = breakPiecePrefabs[i];
            if (prefab == null) continue;

            Vector3 spawnPos = transform.position
                              + (Vector3)(dir * 0.3f);

            GameObject piece = Instantiate(
                prefab,
                spawnPos,
                Quaternion.Euler(0f, 0f,
                    Random.Range(0f, 360f))
            );

            BreakPiece bp = piece.GetComponent<BreakPiece>();
            if (bp != null)
                bp.Launch(
                    dir,
                    Random.Range(pieceSpeed - 1f,
                                 pieceSpeed + 2f),
                    pieceDuration
                );
        }
    }

    // ═══════════════════════════════════════════
    // GATHER ALL ITEMS
    // ═══════════════════════════════════════════
    private List<GameObject> GatherAllLogItems(
        List<GameObject> stuckKnives)
    {
        List<GameObject> all = new List<GameObject>();

        // Dao đã phi vào
        if (stuckKnives != null)
            all.AddRange(stuckKnives);

        // Children của Log: dao sẵn + táo
        foreach (Transform child in transform)
        {
            GameObject obj = child.gameObject;

            if (obj.name == "LogOverlay") continue;
            if (obj.name == "PS_WoodChips") continue;

            if (obj.CompareTag("StuckKnife") ||
                obj.CompareTag("Apple"))
            {
                if (!all.Contains(obj))
                    all.Add(obj);
            }
        }

        Debug.Log($"LogBreakEffect: " +
                  $"Gathered {all.Count} items");
        return all;
    }

    // ═══════════════════════════════════════════
    // DETACH ALL ITEMS
    // ═══════════════════════════════════════════
    private void DetachAllItems(List<GameObject> items)
    {
        foreach (var item in items)
        {
            if (item == null) continue;

            item.transform.SetParent(null);

            KnifeController kc =
                item.GetComponent<KnifeController>();
            if (kc != null) kc.enabled = false;

            Apple apple = item.GetComponent<Apple>();
            if (apple != null) apple.enabled = false;

            Collider2D col =
                item.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    // ═══════════════════════════════════════════
    // LAUNCH ALL ITEMS
    // ═══════════════════════════════════════════
    private void LaunchAllItems(List<GameObject> items)
    {
        if (items == null) return;

        foreach (var item in items)
        {
            if (item == null) continue;

            Vector2 dir = (item.transform.position
                          - transform.position).normalized;
            dir = (dir + Random.insideUnitCircle * 0.4f)
                  .normalized;

            Rigidbody2D rb =
                item.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.gravityScale = 0.3f;
                rb.velocity = dir * Random.Range(
                                         pieceSpeed,
                                         pieceSpeed + 5f);
                rb.angularVelocity = Random.Range(-400f, 400f);
            }
            else
            {
                Rigidbody2D newRb =
                    item.AddComponent<Rigidbody2D>();
                newRb.gravityScale = 0.3f;
                newRb.velocity = dir * Random.Range(
                                            pieceSpeed,
                                            pieceSpeed + 3f);
                newRb.angularVelocity = Random.Range(-400f, 400f);
            }

            StartCoroutine(FadeAndDestroy(
                item, pieceDuration + 0.2f));
        }
    }

    // ═══════════════════════════════════════════
    // FADE AND DESTROY
    // ═══════════════════════════════════════════
    private IEnumerator FadeAndDestroy(
        GameObject obj, float duration)
    {
        if (obj == null) yield break;

        SpriteRenderer sr =
            obj.GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        float fadeStart = duration * 0.5f;

        while (elapsed < duration)
        {
            if (obj == null) yield break;

            elapsed += Time.deltaTime;

            if (sr != null && elapsed > fadeStart)
            {
                float t = (elapsed - fadeStart)
                         / (duration - fadeStart);
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }

            yield return null;
        }

        if (obj != null) Destroy(obj);
    }
}