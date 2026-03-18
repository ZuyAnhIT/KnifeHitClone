using UnityEngine;

/// <summary>
/// Điều khiển toàn bộ hành vi của 1 con dao:
/// Chờ → Bay → Cắm vào Log → Xoay cùng Log
/// hoặc Bay ngược lại khi va chạm dao khác
/// </summary>
public class KnifeController : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // ENUM STATE
    // ═══════════════════════════════════════════
    public enum KnifeState
    {
        Waiting,    // Đang chờ tại SpawnPoint
        Flying,     // Đang bay lên phía Log
        Stuck,      // Đã cắm vào Log, xoay cùng Log
        Bouncing    // Va chạm dao khác, bay ngược xuống
    }

    // ═══════════════════════════════════════════
    // INSPECTOR SETTINGS
    // ═══════════════════════════════════════════
    [Header("── Bay ──")]
    [SerializeField] private float flySpeed = 18f;

    [Header("── Bounce khi thua ──")]
    [SerializeField] private float bounceSpeed = 8f;
    [SerializeField] private float bounceTime = 0.6f;
    // Thêm vào phần INSPECTOR
    [Header("── Effects ──")]
    [SerializeField] private LogHitEffect logHitEffect;
    [SerializeField] private WoodChipsEffect woodChipsEffect;

    // ═══════════════════════════════════════════
    // PRIVATE VARIABLES
    // ═══════════════════════════════════════════
    private KnifeState _state = KnifeState.Waiting;
    private Rigidbody2D _rb;

    // Dùng để xoay cùng Log sau khi cắm
    private Transform _logTransform;
    private Vector3 _localOffset;      // Vị trí tương đối so với Log
    private float _localAngle;       // Góc tương đối so với Log

    private float _bounceTimer;

    // ═══════════════════════════════════════════
    // EVENTS — KnifeThrower lắng nghe
    // ═══════════════════════════════════════════
    public System.Action OnStuckInLog;   // Dao cắm thành công
    public System.Action OnHitKnife;     // Dao trúng dao → Game Over
    public System.Action OnOutOfBounds;  // Dao ra ngoài màn hình

    // ═══════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        switch (_state)
        {
            case KnifeState.Stuck:
                FollowLog();
                break;

            case KnifeState.Bouncing:
                HandleBouncing();
                break;

            case KnifeState.Flying:
                CheckOutOfBounds();
                break;
        }
    }

    // ═══════════════════════════════════════════
    // PUBLIC METHODS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Gọi từ KnifeThrower khi player tap màn hình
    /// </summary>
    public void Launch()
    {
        if (_state != KnifeState.Waiting) return;

        _state = KnifeState.Flying;
        _rb.velocity = Vector2.up * flySpeed;

        Debug.Log("Knife: Launched!");
    }

    /// <summary>
    /// Reset về trạng thái chờ
    /// Dùng khi cần tái sử dụng dao (object pooling sau này)
    /// </summary>
    public void ResetKnife(Vector3 spawnPosition)
    {
        _state = KnifeState.Waiting;
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = false;
        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;
        _logTransform = null;
        _bounceTimer = 0f;
        gameObject.tag = "Knife";
    }

    // ═══════════════════════════════════════════
    // COLLISION
    // ═══════════════════════════════════════════
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_state != KnifeState.Flying) return;

        // THÊM DÒNG NÀY ĐỂ DEBUG
        Debug.Log($"Knife hit object: {other.gameObject.name} | Tag: {other.gameObject.tag}");

        if (other.CompareTag("Log"))
        {
            StickToLog(other.transform);
            return;
        }

        if (other.CompareTag("StuckKnife"))
        {
            HitOtherKnife();
            return;
        }
    }

    // ═══════════════════════════════════════════
    // PRIVATE METHODS
    // ═══════════════════════════════════════════

    private void StickToLog(Transform logTransform)
    {
        _state = KnifeState.Stuck;
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true;

        CircleCollider2D logCollider =
            logTransform.GetComponent<CircleCollider2D>();

        float logRadius = logCollider != null
            ? logCollider.radius * logTransform.localScale.x
            : 0.95f * logTransform.localScale.x;

        Vector2 dirFromCenter = (transform.position
                                - logTransform.position).normalized;

        // Vị trí cắm — KHÔNG THAY ĐỔI
        Vector3 stickPosition = logTransform.position
                               + (Vector3)(dirFromCenter
                               * (logRadius + 1.05f));
        transform.position = stickPosition;

        // Góc xoay — KHÔNG THAY ĐỔI
        float angle = Mathf.Atan2(dirFromCenter.y,
                                   dirFromCenter.x)
                                   * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0f, 0f, angle + 90f);

        _logTransform = logTransform;
        _localOffset = logTransform.InverseTransformPoint(
                            transform.position);
        _localAngle = transform.eulerAngles.z
                        - logTransform.eulerAngles.z;

        // ── CHỈ SỬA PHẦN NÀY ──
        // rawRadius = radius chưa nhân scale
        // = vị trí mép Log thực tế nhìn thấy
        float rawRadius = logCollider != null
            ? logCollider.radius
            : 0.95f;

        Vector3 contactPoint = logTransform.position
                              + (Vector3)(dirFromCenter
                              * (rawRadius + 0.05f));
        // ── HẾT PHẦN SỬA ──

        LogHitEffect hitEffect =
            logTransform.GetComponent<LogHitEffect>();
        if (hitEffect != null)
            hitEffect.PlayHitEffect();

        WoodChipsEffect woodEffect =
            logTransform.GetComponentInChildren<WoodChipsEffect>();
        if (woodEffect != null)
            woodEffect.Play(contactPoint);

        gameObject.tag = "StuckKnife";

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddKnifeThrown();

        OnStuckInLog?.Invoke();
    }

    private void FollowLog()
    {
        if (_logTransform == null) return;

        // Cập nhật vị trí theo Log đang xoay
        transform.position = _logTransform.TransformPoint(_localOffset);

        // Cập nhật góc xoay theo Log
        transform.eulerAngles = new Vector3(
            0f,
            0f,
            _logTransform.eulerAngles.z + _localAngle
        );
    }

    private void HitOtherKnife()
    {
        _state = KnifeState.Bouncing;
        _rb.velocity = Vector2.down * bounceSpeed;
        _bounceTimer = 0f;

        Debug.Log("Knife: Hit another knife! Game Over!");

        // Thông báo Game Over
        OnHitKnife?.Invoke();
    }

    private void HandleBouncing()
    {
        _bounceTimer += Time.deltaTime;
        if (_bounceTimer >= bounceTime)
        {
            Destroy(gameObject);
        }
    }

    private void CheckOutOfBounds()
    {
        // Dao bay ra ngoài màn hình (trường hợp lạ)
        if (transform.position.y > 10f || transform.position.y < -10f)
        {
            Debug.LogWarning("Knife: Out of bounds!");
            OnOutOfBounds?.Invoke();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Gọi khi Log vỡ → Dao thoát khỏi Log
    /// Dừng FollowLog, chuyển sang trạng thái tự do
    /// </summary>
    public void StopFollowing()
    {
        // Xóa reference Log
        _logTransform = null;

        // Đổi state → không còn Stuck
        _state = KnifeState.Bouncing;

        // Bật lại Rigidbody
        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.gravityScale = 0.3f;

            // Hướng bay từ tâm Log ra ngoài
            // Tính từ vị trí hiện tại
            Vector2 dir = Random.insideUnitCircle.normalized;
            _rb.velocity = dir * Random.Range(4f, 8f);
            _rb.angularVelocity = Random.Range(-400f, 400f);
        }

        // Tag đổi lại để không gây collision
        gameObject.tag = "Untagged";
    }

    // ═══════════════════════════════════════════
    // GETTERS
    // ═══════════════════════════════════════════
    public KnifeState CurrentState => _state;
    public bool IsWaiting => _state == KnifeState.Waiting;
    public bool IsFlying => _state == KnifeState.Flying;
    public bool IsStuck => _state == KnifeState.Stuck;
}