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
    [SerializeField] private List<Image> stageDots;
    [SerializeField] private Image bossDotIcon;

    [Header("── Delay Settings ──")]
    [SerializeField] private float stageClearDelay = 1.5f;
    [SerializeField] private float gameOverDelay = 1.0f;

    // ═══════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════
    private GameState _state = GameState.Idle;
    private LevelData _currentLevel;
    private int _stageInCycle = 0; // 0-3 = thường, 4 = boss

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
        // Đăng ký events từ KnifeThrower
        knifeThrower.OnAllKnivesStuck += HandleStageClear;
        knifeThrower.OnGameOver += HandleGameOver;

        // Bắt đầu màn 1
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
        if (_state == GameState.GameOver)
        {
            LevelGenerator.Instance.Reset();
            ScoreManager.Instance?.ResetScore();
            StartStage();
        }
    }

    public GameState CurrentState => _state;
    public LevelData CurrentLevel => _currentLevel;

    // ═══════════════════════════════════════════
    // PRIVATE — STAGE FLOW
    // ═══════════════════════════════════════════
    private void StartStage()
    {
        // Generate level mới
        _currentLevel = LevelGenerator.Instance.GenerateNext();
        _state = GameState.Playing;

        // ── Setup Log ──
        logRotator.SetPattern(_currentLevel);
        logRotator.SetActive(true);

        logItemPlacer.Setup(
        _currentLevel.preplacedCount,
        _currentLevel.appleCount
        );

        // ── Setup KnifeThrower ──
        knifeThrower.SetupLevel(_currentLevel.knifeCount);

        // ── Cập nhật HUD ──
        UpdateStageHUD(_currentLevel);

        // ── Reset Score ──
        ScoreManager.Instance?.ResetScore();

        OnStageStarted?.Invoke(_currentLevel);

        Debug.Log($"GameManager: Started {_currentLevel}");
    }

    private void HandleStageClear()
    {
        if (_state != GameState.Playing) return;

        _state = GameState.StageClear;
        logRotator.SetActive(false);

        Debug.Log("GameManager: Stage Clear!");
        OnStageClear?.Invoke();

        // Tự động sang màn tiếp sau delay
        StartCoroutine(NextStageRoutine());
    }

    private void HandleGameOver()
    {
        if (_state != GameState.Playing) return;

        _state = GameState.GameOver;
        logRotator.SetActive(false);
        knifeThrower.SetCanThrow(false);

        Debug.Log("GameManager: Game Over!");
        OnGameOver?.Invoke();

        // TODO: Hiện panel Game Over sau
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator NextStageRoutine()
    {
        yield return new WaitForSeconds(stageClearDelay);
        StartStage();
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(gameOverDelay);
        // TODO: Hiện panel Game Over
        // Tạm thời restart luôn
        RestartCurrentStage();
    }

    // ═══════════════════════════════════════════
    // PRIVATE — HUD UPDATE
    // ═══════════════════════════════════════════
    private void UpdateStageHUD(LevelData data)
    {
        // ── Stage Name ──
        if (txtStageName != null)
        {
            txtStageName.text = data.isBossStage
                ? "BOSS STAGE"
                : $"STAGE {data.stageNumber}";
        }

        // ── Stage Dots ──
        // 4 chấm thường + 1 icon boss
        UpdateStageDots(data.stageInCycle, data.isBossStage);
    }

    private void UpdateStageDots(int currentPos, bool isBoss)
    {
        // currentPos: 1-4 = thường, 5 = boss
        for (int i = 0; i < stageDots.Count; i++)
        {
            if (stageDots[i] == null) continue;

            // i+1 <= currentPos → đã qua hoặc đang ở stage này
            bool isPassed = (i + 1) < currentPos;
            bool isCurrent = (i + 1) == currentPos && !isBoss;

            if (isCurrent)
            {
                // Chấm hiện tại: vàng đậm
                stageDots[i].color = new Color(1f, 0.7f, 0f, 1f);
            }
            else if (isPassed)
            {
                // Chấm đã qua: trắng mờ
                stageDots[i].color = new Color(1f, 1f, 1f, 0.4f);
            }
            else
            {
                // Chấm chưa tới: trắng bình thường
                stageDots[i].color = new Color(1f, 1f, 1f, 0.8f);
            }
        }

        // Boss icon: highlight khi đang ở boss stage
        if (bossDotIcon != null)
        {
            bossDotIcon.color = isBoss
                ? new Color(1f, 0.3f, 0f, 1f)  // Cam đỏ khi boss
                : new Color(1f, 1f, 1f, 0.8f);  // Trắng bình thường
        }
    }
}