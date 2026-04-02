using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Điều phối toàn bộ luồng game:
/// Start → Playing → StageClear/GameOver → Next Stage
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ═══════════════════════════════════════════
    // GAME STATE
    // ═══════════════════════════════════════════
    public enum GameState
    {
        Idle,
        Playing,
        StageClear,
        GameOver,
        Paused
    }

    // ═══════════════════════════════════════════
    // INSPECTOR REFERENCES
    // ═══════════════════════════════════════════
    [Header("── Gameplay References ──")]
    [SerializeField] private KnifeThrower knifeThrower;
    [SerializeField] private LogRotator logRotator;
    [SerializeField] private LogItemPlacer logItemPlacer;

    [Header("── Environment (Background) ──")]
    [Tooltip("Kéo object Background vào đây")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [Tooltip("Ảnh nền xanh đen gốc của màn thường")]
    [SerializeField] private Sprite defaultBackgroundSprite;

    [Header("── HUD References ──")]
    [SerializeField] private TextMeshProUGUI txtStageName;
    [SerializeField] private TextMeshProUGUI txtBossName;
    [SerializeField] private List<Image> stageDots;
    [SerializeField] private Image bossDotIcon;
    [SerializeField] private GameObject stageProgressGroup;
    [SerializeField] private GameObject bossIconCenter;

    [Header("── Break Effect ──")]
    [SerializeField] private LogBreakEffect logBreakEffect;

    [Header("── Boss ──")]
    [SerializeField] private BossLogManager bossLogManager;
    [SerializeField] private SpriteRenderer logSpriteRenderer;
    [SerializeField] private Sprite defaultLogSprite;

    [Header("── Delay Settings ──")]
    [SerializeField] private float stageClearDelay = 1.5f;
    [SerializeField] private float gameOverDelay = 1.0f;

    [Header("── Âm thanh ──")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip targetAppearSound;

    [Header("── UI Transitions ──")]
    [SerializeField] private BossTransitionUI bossTransitionUI;
    [SerializeField] private ContinueUI continueUI; // Giao diện Hồi sinh

    // ═══════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════
    private GameState _state = GameState.Idle;
    private LevelData _currentLevel;
    private Sprite _originalLogSprite; // Lưu gỗ thường gốc

    // ═══════════════════════════════════════════
    // EVENTS
    // ═══════════════════════════════════════════
    public System.Action<LevelData> OnStageStarted;
    public System.Action OnStageClear;
    public System.Action OnGameOver;

    // ═══════════════════════════════════════════
    // UNITY LIFECYCLE
    // ═══════════════════════════════════════════
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        knifeThrower.OnAllKnivesStuck += HandleStageClear;
        knifeThrower.OnGameOver += HandleGameOver;

        _originalLogSprite = defaultLogSprite; // Lưu lại gỗ chuẩn ban đầu

        ApplyGameModeSettings(); // BƯỚC MỚI: Mặc áo trước khi chơi

        StartStage();
    }

    // ═══════════════════════════════════════════
    // PRIVATE — SETUP GAME MODE (CHALLENGE / NORMAL)
    // ═══════════════════════════════════════════
    private void ApplyGameModeSettings()
    {
        if (GameModeManager.CurrentMode == GameMode.Challenge && GameModeManager.CurrentChallenge != null)
        {
            ChallengeData challenge = GameModeManager.CurrentChallenge;

            // 1. Đổi ảnh nền
            if (backgroundRenderer != null && challenge.backgroundSprite != null)
                backgroundRenderer.sprite = challenge.backgroundSprite;

            // 2. Đổi ảnh khúc gỗ (Ghi đè tạm thời defaultLogSprite)
            if (challenge.logSprite != null)
                defaultLogSprite = challenge.logSprite;

            Debug.Log($"GameManager: Load Challenge -> {challenge.challengeName}");
        }
        else
        {
            // Trả về Normal
            if (backgroundRenderer != null && defaultBackgroundSprite != null)
                backgroundRenderer.sprite = defaultBackgroundSprite;

            if (_originalLogSprite != null)
                defaultLogSprite = _originalLogSprite;
        }
    }


    // ═══════════════════════════════════════════
    // PUBLIC
    // ═══════════════════════════════════════════
    public void StartNextStage()
    {
        if (_state == GameState.StageClear)
            StartStage();
    }

    public void RestartCurrentStage()
    {
        _state = GameState.Playing;

        // Reset tất cả
        ScoreManager.Instance?.ResetScore();
        LevelGenerator.Instance.Reset();
        ResetLogSprite();

        // Cho phép ném dao
        if (knifeThrower != null)
            knifeThrower.SetCanThrow(true);

        // Bắt đầu lại Stage 1
        StartStage();

        Debug.Log("GameManager: Restarted!");
    }

    public GameState CurrentState => _state;
    public LevelData CurrentLevel => _currentLevel;

    // ═══════════════════════════════════════════
    // PRIVATE — STAGE FLOW
    // ═══════════════════════════════════════════
    private void StartStage()
    {
        _currentLevel = LevelGenerator.Instance.GenerateNext();
        _state = GameState.Playing;

        UpdateStageHUD(_currentLevel);

        if (_currentLevel.isBossStage)
        {
            // Ẩn khúc gỗ đi để màn hình trống trải khi đao bay lên
            if (logSpriteRenderer != null)
                logSpriteRenderer.enabled = false;

            if (bossTransitionUI != null)
            {
                bossTransitionUI.PlayTransition(() =>
                {
                    // Hiện lại khúc gỗ sau khi 2 thanh đao đã rớt xuống
                    if (logSpriteRenderer != null)
                        logSpriteRenderer.enabled = true;

                    StartBossStage();
                    OnStageStarted?.Invoke(_currentLevel);
                });
            }
            else
            {
                // Fallback an toàn
                if (logSpriteRenderer != null) logSpriteRenderer.enabled = true;
                StartBossStage();
                OnStageStarted?.Invoke(_currentLevel);
            }
        }
        else
        {
            // Nếu là màn thường thì luôn luôn hiển thị khúc gỗ
            if (logSpriteRenderer != null) logSpriteRenderer.enabled = true;
            StartNormalStage();
            OnStageStarted?.Invoke(_currentLevel);
        }

        Debug.Log($"GameManager: Started {_currentLevel}");
    }

    private void StartBossStage()
    {
        BossLogData boss = bossLogManager.GetRandomBoss();
        bossLogManager.ApplyBossLog(boss);
        logRotator.SetActive(true);

        // Set boss mode cho LogBreakEffect
        if (logBreakEffect != null)
            logBreakEffect.SetBossMode(true, boss.explodeColor);

        logItemPlacer.Setup(
            _currentLevel.preplacedCount,
            _currentLevel.appleCount);

        knifeThrower.SetupLevel(_currentLevel.knifeCount);
        UpdateBossNameHUD(boss);

        // Phát âm thanh khi Log Boss từ từ hiện lên
        if (audioSource != null && targetAppearSound != null)
        {
            audioSource.PlayOneShot(targetAppearSound);
        }
    }

    private void StartNormalStage()
    {
        ResetLogSprite();

        // Reset về normal mode
        if (logBreakEffect != null)
            logBreakEffect.SetBossMode(false, Color.white);

        logRotator.SetPattern(_currentLevel);
        logRotator.SetActive(true);

        logItemPlacer.Setup(
            _currentLevel.preplacedCount,
            _currentLevel.appleCount);

        knifeThrower.SetupLevel(_currentLevel.knifeCount);

        if (audioSource != null && targetAppearSound != null)
        {
            audioSource.PlayOneShot(targetAppearSound);
        }
    }

    // ═══════════════════════════════════════════
    // PRIVATE — STAGE CLEAR
    // ═══════════════════════════════════════════
    private void HandleStageClear()
    {
        if (_state != GameState.Playing) return;

        _state = GameState.StageClear;
        logRotator.SetActive(false);
        knifeThrower.SetCanThrow(false);

        if (SaveManager.Instance != null)
            SaveManager.Instance.UpdateBestStage(_currentLevel.stageNumber);

        OnStageClear?.Invoke();

        List<GameObject> stuck = knifeThrower.GetStuckKnives();
        logBreakEffect.OnBreakComplete += HandleBreakComplete;
        logBreakEffect.PlayBreak(stuck);
    }

    private void HandleBreakComplete()
    {
        logBreakEffect.OnBreakComplete -= HandleBreakComplete;
        knifeThrower.ClearAllKnivesPublic();

        // Nếu vừa xong boss → Reset log về sprite thường
        if (_currentLevel.isBossStage)
            ResetLogSprite();

        // Stage tự tăng tiếp, KHÔNG reset LevelGenerator
        StartStage();
    }

    // ═══════════════════════════════════════════
    // PRIVATE — GAME OVER & CONTINUE
    // ═══════════════════════════════════════════
    private void HandleGameOver()
    {
        if (_state != GameState.Playing) return;

        _state = GameState.GameOver;
        logRotator.SetActive(false);
        knifeThrower.SetCanThrow(false);

        Debug.Log("GameManager: Dao va cham! Cho man hinh Continue...");

        // Gọi UI Continue lên trước
        if (continueUI != null)
        {
            continueUI.ShowPanel();
        }
        else
        {
            // Nếu quên gắn UI Continue thì mới hiện Game Over luôn
            ShowFinalGameOver();
        }
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(gameOverDelay);

        // Lưu best score
        if (SaveManager.Instance != null &&
            ScoreManager.Instance != null)
            SaveManager.Instance.UpdateBestScore(
                ScoreManager.Instance.KnifeThrown);

        // Báo cho GameOverUI hiện panel
        OnGameOver?.Invoke();
    }

    // ═══════════════════════════════════════════
    // PUBLIC — REVIVE (HỒI SINH TỪ CONTINUE UI)
    // ═══════════════════════════════════════════

    public void ReviveGame()
    {
        _state = GameState.Playing;
        logRotator.SetActive(true); // Gỗ quay trở lại

        if (knifeThrower != null)
        {
            knifeThrower.Revive(); // Mở khóa ném dao
        }

        Debug.Log("GameManager: Da hoi sinh thanh cong!");
    }

    public void ShowFinalGameOver()
    {
        // Khởi chạy tiến trình Game Over thực sự (Lưu điểm và hiện bảng)
        StartCoroutine(GameOverRoutine());
    }

    // ═══════════════════════════════════════════
    // PRIVATE — HUD UPDATE
    // ═══════════════════════════════════════════
    private void UpdateStageHUD(LevelData data)
    {
        if (data.isBossStage)
            UpdateHUDBoss();
        else
            UpdateHUDNormal(data);
    }

    private void UpdateHUDBoss()
    {
        // ẨN toàn bộ HUD_StageProgress
        if (stageProgressGroup != null)
            stageProgressGroup.SetActive(false);

        // HIỆN BossIconCenter căn giữa + đỏ
        if (bossIconCenter != null)
            bossIconCenter.SetActive(true);

        // ẨN Stage Name thường
        if (txtStageName != null)
            txtStageName.gameObject.SetActive(false);
    }

    private void UpdateHUDNormal(LevelData data)
    {
        // HIỆN lại HUD_StageProgress
        if (stageProgressGroup != null)
            stageProgressGroup.SetActive(true);

        // ẨN BossIconCenter
        if (bossIconCenter != null)
            bossIconCenter.SetActive(false);

        // ẨN Boss Name
        if (txtBossName != null)
            txtBossName.gameObject.SetActive(false);

        // HIỆN Stage Name & Đổi chữ theo chế độ
        if (txtStageName != null)
        {
            txtStageName.gameObject.SetActive(true);

            // BƯỚC MỚI: Cập nhật chữ nếu đang ở chế độ Challenge
            if (GameModeManager.CurrentMode == GameMode.Challenge && GameModeManager.CurrentChallenge != null)
            {
                txtStageName.text = $"{GameModeManager.CurrentChallenge.challengeName.ToUpper()} CHALLENGE {data.stageNumber}";
            }
            else
            {
                txtStageName.text = $"STAGE {data.stageNumber}";
            }

            txtStageName.color = Color.white;
        }

        // Cập nhật màu Dots
        UpdateStageDots(data.stageInCycle);
    }

    private void UpdateStageDots(int currentPos)
    {
        for (int i = 0; i < stageDots.Count; i++)
        {
            if (stageDots[i] == null) continue;

            bool isPassed = (i + 1) < currentPos;
            bool isCurrent = (i + 1) == currentPos;

            if (isCurrent)
                stageDots[i].color =
                    new Color(1f, 0.7f, 0f, 1f);   // Vàng
            else if (isPassed)
                stageDots[i].color =
                    new Color(1f, 1f, 1f, 0.4f);   // Trắng mờ
            else
                stageDots[i].color =
                    new Color(1f, 1f, 1f, 0.8f);   // Trắng
        }

        // Boss icon luôn trắng ở màn thường
        if (bossDotIcon != null)
            bossDotIcon.color = new Color(1f, 1f, 1f, 0.8f);
    }

    private void UpdateBossNameHUD(BossLogData boss)
    {
        if (txtBossName == null || boss == null) return;

        txtBossName.text = $"BOSS: {boss.bossName}";
        txtBossName.color = new Color(1f, 0.2f, 0.2f, 1f);
        txtBossName.gameObject.SetActive(true);
    }

    // ═══════════════════════════════════════════
    // PRIVATE — HELPERS
    // ═══════════════════════════════════════════
    private void ResetLogSprite()
    {
        if (logSpriteRenderer != null &&
            defaultLogSprite != null)
            logSpriteRenderer.sprite = defaultLogSprite;
    }
}