using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LogBreakEffect : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // INSPECTOR
    // ═══════════════════════════════════════════
    [Header("── Mảnh vỡ ──")]
    [SerializeField] private List<GameObject> breakPiecePrefabs;
    [SerializeField] private int pieceCount = 8;
    [SerializeField] private float pieceSpeed = 5f;
    [SerializeField] private float pieceDuration = 0.9f;

    [Header("── Flash trắng ──")]
    [SerializeField] private SpriteRenderer flashOverlay;
    [SerializeField] private float flashInDuration = 0.08f;
    [SerializeField] private float flashOutDuration = 0.25f;

    [Header("── Timing ──")]
    [SerializeField] private float delayBeforeBreak = 0.15f;
    [SerializeField] private float delayBeforeNew = 1.0f;

    // ── Private ──
    private SpriteRenderer _sr;

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

    // ═══════════════════════════════════════════
    // SEQUENCE
    // ═══════════════════════════════════════════
    private IEnumerator BreakSequence(
    List<GameObject> stuckKnives)
    {
        yield return new WaitForSeconds(delayBeforeBreak);

        // Gom TẤT CẢ object cần bay ra:
        // 1. Dao phi vào (stuckKnives từ KnifeThrower)
        // 2. Dao cắm sẵn (con của Log, tag StuckKnife)
        // 3. Táo (con của Log, tag Apple)
        List<GameObject> allItems = GatherAllLogItems(
                                        stuckKnives);

        // Tách tất cả ra khỏi Log
        DetachAllItems(allItems);

        // Flash trắng
        yield return StartCoroutine(FlashRoutine());

        // Ẩn Log
        if (_sr != null)
            _sr.enabled = false;

        // Spawn mảnh vỡ
        SpawnBreakPieces();

        // Bay tứ phía
        LaunchAllItems(allItems);

        yield return new WaitForSeconds(delayBeforeNew);

        if (_sr != null)
            _sr.enabled = true;

        OnBreakComplete?.Invoke();
    }

    /// Tách dao ra khỏi Log sớm
    /// Tránh bị ẩn theo Log
    private void DetachKnivesFromLog(
    List<GameObject> stuckKnives)
    {
        if (stuckKnives == null) return;

        foreach (var knife in stuckKnives)
        {
            if (knife == null) continue;

            // Tách khỏi Log
            knife.transform.SetParent(null);

            // Dừng KnifeController nếu có
            KnifeController kc =
                knife.GetComponent<KnifeController>();
            if (kc != null)
                kc.enabled = false; // Tắt Update() luôn
        }
    }

    // ═══════════════════════════════════════════
    // FLASH TRẮNG
    // ═══════════════════════════════════════════
    private IEnumerator FlashRoutine()
    {
        if (flashOverlay == null) yield break;

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color white = new Color(1f, 1f, 1f, 0.85f);

        // Sáng nhanh
        float t = 0f;
        while (t < flashInDuration)
        {
            t += Time.deltaTime;
            flashOverlay.color = Color.Lerp(
                clear, white,
                t / flashInDuration);
            yield return null;
        }

        // Mờ dần
        t = 0f;
        while (t < flashOutDuration)
        {
            t += Time.deltaTime;
            flashOverlay.color = Color.Lerp(
                white, clear,
                t / flashOutDuration);
            yield return null;
        }

        flashOverlay.color = clear;
    }

    // ═══════════════════════════════════════════
    // SPAWN MẢNH VỠ
    // ═══════════════════════════════════════════
    private void SpawnBreakPieces()
    {
        if (breakPiecePrefabs == null ||
            breakPiecePrefabs.Count == 0) return;

        // Chia đều góc cho đúng số mảnh
        // Mỗi prefab spawn đúng 1 lần
        int total = breakPiecePrefabs.Count; // = 3

        float angleStep = 360f / total;

        for (int i = 0; i < total; i++)
        {
            // Góc chia đều + random nhỏ tự nhiên
            float angle = i * angleStep
                         + Random.Range(-20f, 20f);
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(
                              Mathf.Cos(rad),
                              Mathf.Sin(rad));

            // Lấy đúng prefab theo thứ tự
            // Không random → Mỗi loại 1 cái
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
    // DAO BAY RA
    // ═══════════════════════════════════════════
    private void LaunchStuckKnives(
    List<GameObject> stuckKnives)
    {
        if (stuckKnives == null) return;

        foreach (var knife in stuckKnives)
        {
            if (knife == null) continue;

            // Gọi StopFollowing trước
            // → Dừng FollowLog() trong Update()
            KnifeController kc =
                knife.GetComponent<KnifeController>();
            if (kc != null)
            {
                kc.StopFollowing();
            }
            else
            {
                // Dao cắm sẵn (không có KnifeController)
                // → Xử lý thủ công
                knife.transform.SetParent(null);

                Rigidbody2D rb =
                    knife.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.gravityScale = 0.3f;
                    rb.velocity =
                        Random.insideUnitCircle.normalized
                        * Random.Range(4f, 8f);
                    rb.angularVelocity =
                        Random.Range(-400f, 400f);
                }
            }

            // Mờ dần rồi destroy
            StartCoroutine(FadeAndDestroy(
                knife, pieceDuration + 0.2f));
        }
    }

    private IEnumerator FadeAndDestroy(
        GameObject obj, float duration)
    {
        SpriteRenderer sr =
            obj.GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        float fadeStart = duration * 0.5f;

        while (elapsed < duration)
        {
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

        if (obj != null)
            Destroy(obj);
    }

    /// <summary>
    /// Gom tất cả item cần bay ra khi Log vỡ:
    /// Dao phi vào + Dao cắm sẵn + Táo
    /// </summary>
    private List<GameObject> GatherAllLogItems(
        List<GameObject> stuckKnives)
    {
        List<GameObject> all = new List<GameObject>();

        // 1. Dao đã phi vào (từ KnifeThrower)
        if (stuckKnives != null)
            all.AddRange(stuckKnives);

        // 2. Tìm tất cả CHILDREN của Log
        //    Bao gồm: dao cắm sẵn + táo
        foreach (Transform child in transform)
        {
            GameObject obj = child.gameObject;

            // Bỏ qua LogOverlay và PS_WoodChips
            if (obj.name == "LogOverlay") continue;
            if (obj.name == "PS_WoodChips") continue;

            // Lấy dao cắm sẵn
            if (obj.CompareTag("StuckKnife"))
            {
                if (!all.Contains(obj))
                    all.Add(obj);
                continue;
            }

            // Lấy táo
            if (obj.CompareTag("Apple"))
            {
                if (!all.Contains(obj))
                    all.Add(obj);
                continue;
            }
        }

        Debug.Log($"LogBreakEffect: " +
                  $"Gathered {all.Count} items to launch");
        return all;
    }

    private void DetachAllItems(List<GameObject> items)
    {
        foreach (var item in items)
        {
            if (item == null) continue;

            // Tách khỏi Log
            item.transform.SetParent(null);

            // Tắt KnifeController nếu có
            KnifeController kc =
                item.GetComponent<KnifeController>();
            if (kc != null)
                kc.enabled = false;

            // Tắt Apple script nếu có
            // Tránh trigger nhặt táo sau khi Log vỡ
            Apple apple = item.GetComponent<Apple>();
            if (apple != null)
                apple.enabled = false;

            // Tắt Collider
            Collider2D col =
                item.GetComponent<Collider2D>();
            if (col != null)
                col.enabled = false;
        }
    }

    private void LaunchAllItems(List<GameObject> items)
    {
        if (items == null) return;

        foreach (var item in items)
        {
            if (item == null) continue;

            // Hướng bay từ tâm Log ra ngoài
            Vector2 dir = (item.transform.position
                          - transform.position).normalized;

            // Thêm random
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
                                         pieceSpeed + 3f);
                rb.angularVelocity = Random.Range(
                                         -400f, 400f);
            }
            else
            {
                // Táo không có Rigidbody → Thêm vào
                Rigidbody2D newRb =
                    item.AddComponent<Rigidbody2D>();
                newRb.gravityScale = 0.3f;
                newRb.velocity = dir * Random.Range(
                                            pieceSpeed,
                                            pieceSpeed + 3f);
                newRb.angularVelocity = Random.Range(
                                            -400f, 400f);
            }

            // Mờ dần rồi destroy
            StartCoroutine(FadeAndDestroy(
                item, pieceDuration + 0.2f));
        }
    }
}