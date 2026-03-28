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

    [Header("── Boss Transition UI ──")]
    [SerializeField] private BossTransitionUI bossTransitionUI;

    // ═══════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════
    private GameState _state = GameState.Idle;
    private LevelData _currentLevel;

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
        StartStage();
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
        // XÓA điều kiện check state
        // Cho phép gọi từ bất kỳ đâu

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
                    // BƯỚC QUAN TRỌNG 2: Hiện lại khúc gỗ sau khi 2 thanh đao đã rớt xuống
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

        // CHỈ CÒN NHƯ THẾ NÀY (Đã xóa đoạn bossTransitionUI đi)
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
    // PRIVATE — GAME OVER
    // ═══════════════════════════════════════════
    private void HandleGameOver()
    {
        if (_state != GameState.Playing) return;

        _state = GameState.GameOver;
        logRotator.SetActive(false);
        knifeThrower.SetCanThrow(false);

        Debug.Log("GameManager: Game Over!");
        OnGameOver?.Invoke();

        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(gameOverDelay);

        // Lưu best score
        if (SaveManager.Instance != null &&
            ScoreManager.Instance != null)
            SaveManager.Instance.UpdateBestScore(
                ScoreManager.Instance.KnifeThrown);

        // KHÔNG tự restart nữa
        // GameOverUI sẽ xử lý khi người chơi nhấn RESTART
        OnGameOver?.Invoke(); // Báo cho GameOverUI hiện panel
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

        // txtBossName đã được set trong UpdateBossNameHUD()
        // được gọi trước UpdateStageHUD trong StartBossStage()
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

        // HIỆN Stage Name
        if (txtStageName != null)
        {
            txtStageName.gameObject.SetActive(true);
            txtStageName.text = $"STAGE {data.stageNumber}";
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