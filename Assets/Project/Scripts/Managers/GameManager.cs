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
    [SerializeField] private float normalFontSize = 65f;     // Cỡ chữ màn thường
    [SerializeField] private float challengeFontSize = 45f;  // Cỡ chữ màn Challenge (nhỏ hơn)
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
    [SerializeField] private ChallengeCompleteUI challengeCompleteUI; // BƯỚC MỚI: Giao diện Chúc mừng Challenge

    // ═══════════════════════════════════════════
    // PRIVATE
    // ═══════════════════════════════════════════
    private GameState _state = GameState.Idle;
    private LevelData _currentLevel;
    private Sprite _originalLogSprite; // Lưu gỗ thường gốc

    // Đếm vị trí Stage trong chu kỳ Challenge (1, 2, 3, 4, 5)
    private int _challengeStageInCycle = 1;

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
        _challengeStageInCycle = 1;

        ApplyGameModeSettings(); // Mặc áo trước khi chơi

        StartStage();
    }

    // ═══════════════════════════════════════════
    // PRIVATE — SETUP GAME MODE (CHALLENGE / NORMAL)
    // ═══════════════════════════════════════════
    private void ApplyGameModeSettings()
    {
        Color currentThemeColor = new Color(1f, 0.7f, 0f, 1f); // Vàng mặc định

        if (GameModeManager.CurrentMode == GameMode.Challenge && GameModeManager.CurrentChallenge != null)
        {
            ChallengeData challenge = GameModeManager.CurrentChallenge;

            // 1. Đổi ảnh nền
            if (backgroundRenderer != null && challenge.backgroundSprite != null)
                backgroundRenderer.sprite = challenge.backgroundSprite;

            // 2. Đổi ảnh khúc gỗ thường
            if (challenge.normalLogSprite != null)
                defaultLogSprite = challenge.normalLogSprite;

            // 3. Lấy màu chủ đạo
            currentThemeColor = challenge.themeColor;

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

        // Truyền màu chủ đạo sang UI va kiếm
        if (bossTransitionUI != null)
        {
            bossTransitionUI.SetThemeColor(currentThemeColor);
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
        _challengeStageInCycle = 1; // Reset vòng stage khi thua chơi lại

        // Reset tất cả
        ScoreManager.Instance?.ResetScore();
        LevelGenerator.Instance.Reset();
        ResetLogSprite();

        // Cho phép ném dao
        if (knifeThrower != null)
            knifeThrower.SetCanThrow(true);

        // Bắt đầu lại
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

        // ĐÁNH CHẶN LOGIC CHO CHALLENGE (ÉP VỀ 5 STAGE NHƯ BÌNH THƯỜNG)
        if (GameModeManager.CurrentMode == GameMode.Challenge)
        {
            _currentLevel.stageInCycle = _challengeStageInCycle;
            _currentLevel.isBossStage = (_challengeStageInCycle == 5); // Màn 5 là Boss
        }

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
        BossLogData boss = null;

        // Lấy Boss riêng từ ChallengeData thay vì Random toàn game
        if (GameModeManager.CurrentMode == GameMode.Challenge && GameModeManager.CurrentChallenge != null)
        {
            var bosses = GameModeManager.CurrentChallenge.challengeBosses;
            if (bosses != null && bosses.Count > 0)
            {
                // Lấy boss theo Level (Lặp lại vòng nếu level lớn hơn số lượng boss cài sẵn)
                int index = (GameModeManager.CurrentChallengeLevel - 1) % bosses.Count;
                boss = bosses[index];
            }
        }

        // Fallback về Boss ngẫu nhiên nếu là màn Normal
        if (boss == null) boss = bossLogManager.GetRandomBoss();

        bossLogManager.ApplyBossLog(boss);
        logRotator.SetActive(true);

        // Set boss mode cho LogBreakEffect
        if (logBreakEffect != null)
            logBreakEffect.SetBossMode(true, boss.explodeColor);

        // ── KHÔNG ĐẺ TÁO TRONG CHALLENGE ──
        int spawnApples = (GameModeManager.CurrentMode == GameMode.Challenge) ? 0 : _currentLevel.appleCount;
        logItemPlacer.Setup(_currentLevel.preplacedCount, spawnApples);

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

        Color breakColor = Color.white;
        Sprite[] customBrokenSprites = null; // Biến lưu mảng ảnh vỡ

        // Lấy màu vỡ và ảnh vỡ của gỗ thường trong Challenge
        if (GameModeManager.CurrentMode == GameMode.Challenge && GameModeManager.CurrentChallenge != null)
        {
            breakColor = GameModeManager.CurrentChallenge.normalExplodeColor;
            customBrokenSprites = GameModeManager.CurrentChallenge.normalBrokenSprites; // Lấy ảnh vỡ
        }

        if (logBreakEffect != null)
            logBreakEffect.SetBossMode(false, breakColor, customBrokenSprites); // Truyền ảnh vỡ xuống Effect

        logRotator.SetPattern(_currentLevel);
        logRotator.SetActive(true);

        // ── KHÔNG ĐẺ TÁO TRONG CHALLENGE ──
        int spawnApples = (GameModeManager.CurrentMode == GameMode.Challenge) ? 0 : _currentLevel.appleCount;
        logItemPlacer.Setup(_currentLevel.preplacedCount, spawnApples);

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

        // BƯỚC MỚI: XỬ LÝ CHUYỂN STAGE TRONG CHALLENGE VÀ HIỆN BẢNG CHÚC MỪNG
        if (GameModeManager.CurrentMode == GameMode.Challenge)
        {
            if (_challengeStageInCycle == 5) // Đã qua màn Boss (vòng 5)
            {
                _challengeStageInCycle = 1; // Quay lại vòng mới

                // ── BƯỚC SỬA LỖI: ẨN LOG KHỎI MÀN HÌNH CHỜ HIỆN BẢNG ──
                if (logSpriteRenderer != null) logSpriteRenderer.enabled = false;
                if (logRotator != null) logRotator.SetActive(false);

                // HIỆN BẢNG CHÚC MỪNG VÀ DỪNG LẠI CHỜ NGƯỜI CHƠI BẤM NÚT
                if (challengeCompleteUI != null)
                {
                    challengeCompleteUI.ShowPanel();
                    return; // Ngắt hàm, không tự động gọi StartStage() nữa!
                }
                else
                {
                    // Fallback an toàn nếu bạn quên gắn Panel
                    GameModeManager.CompleteCurrentAndMoveToNext();
                }
            }
            else
            {
                _challengeStageInCycle++;
            }
        }

        // Stage tự tăng tiếp (Nếu là màn thường hoặc chưa qua Boss Challenge)
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
    // BƯỚC MỚI: PUBLIC — GỌI KHI BẤM NÚT "NEXT" TRÊN BẢNG CHÚC MỪNG
    // ═══════════════════════════════════════════
    /// <summary>
    /// Gọi từ ChallengeCompleteUI khi người chơi bấm "Next Challenge"
    /// </summary>
    public void StartNextChallengeFromUI()
    {
        _state = GameState.Playing;
        ScoreManager.Instance?.ResetScore();
        LevelGenerator.Instance.Reset();

        if (knifeThrower != null) knifeThrower.SetCanThrow(true);
        StartStage(); // Bắt đầu màn chơi của Challenge tiếp theo
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

            // Tự động chỉnh cỡ chữ cho gọn và xuống dòng
            if (GameModeManager.CurrentMode == GameMode.Challenge && GameModeManager.CurrentChallenge != null)
            {
                txtStageName.text = $"{GameModeManager.CurrentChallenge.challengeName.ToUpper()}\nCHALLENGE {GameModeManager.CurrentChallengeLevel}";
                txtStageName.fontSize = challengeFontSize; // Thu nhỏ chữ
            }
            else
            {
                txtStageName.text = $"STAGE {data.stageNumber}";
                txtStageName.fontSize = normalFontSize; // Trả về chữ to
            }

            txtStageName.color = Color.white;
        }

        // Cập nhật màu Dots
        UpdateStageDots(data.stageInCycle);
    }

    private void UpdateStageDots(int currentPos)
    {
        // Lấy màu chủ đạo
        Color activeColor = new Color(1f, 0.7f, 0f, 1f); // Mặc định vàng
        if (GameModeManager.CurrentMode == GameMode.Challenge && GameModeManager.CurrentChallenge != null)
        {
            activeColor = GameModeManager.CurrentChallenge.themeColor;
        }

        for (int i = 0; i < stageDots.Count; i++)
        {
            if (stageDots[i] == null) continue;
            stageDots[i].gameObject.SetActive(true); // Luôn hiện đủ các dot

            bool isPassed = (i + 1) < currentPos;
            bool isCurrent = (i + 1) == currentPos;

            // Tô màu theo Theme
            if (isCurrent)
                stageDots[i].color = activeColor;
            else if (isPassed)
                stageDots[i].color = new Color(1f, 1f, 1f, 0.4f);   // Trắng mờ
            else
                stageDots[i].color = new Color(1f, 1f, 1f, 0.8f);   // Trắng
        }

        // Tô màu cho Icon Boss nếu đang chuẩn bị đánh boss (Vòng 5)
        if (bossDotIcon != null)
        {
            bool isBossNext = (currentPos == 5);
            bossDotIcon.color = isBossNext ? activeColor : new Color(1f, 1f, 1f, 0.8f);
        }
    }

    private void UpdateBossNameHUD(BossLogData boss)
    {
        if (txtBossName == null) return;

        // Nếu là Boss Challenge -> Ghi "TÊN CHALLENGE (Cấp độ)"
        if (GameModeManager.CurrentMode == GameMode.Challenge && GameModeManager.CurrentChallenge != null)
        {
            txtBossName.text = $"{GameModeManager.CurrentChallenge.challengeName.ToUpper()} CHALLENGE {GameModeManager.CurrentChallengeLevel}";
        }
        else
        {
            txtBossName.text = $"BOSS: {boss?.bossName}";
        }

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