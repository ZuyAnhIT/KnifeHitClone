using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý chuyển scene toàn game
/// Gắn vào 1 object duy nhất trong mỗi scene
/// Gọi trực tiếp từ Button.OnClick()
/// </summary>
public class SceneTransition : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // SCENE NAMES — Đặt đúng tên scene
    // ═══════════════════════════════════════════
    private const string SCENE_MENU = "SC_MenuGame";
    private const string SCENE_GAMEPLAY = "SC_Gameplay";
    private const string SCENE_STORE = "SC_MenuStore";
    private const string SCENE_CHALLENGE = "SC_Challenge";

    // ═══════════════════════════════════════════
    // PUBLIC — Gọi từ Button.OnClick()
    // ═══════════════════════════════════════════

    /// <summary>
    /// Chuyển sang màn chơi game (Mặc định là màn Normal)
    /// Gắn vào Btn_Play trong SC_MenuGame
    /// </summary>
    public void GoToGameplay()
    {
        // ── BƯỚC SỬA LỖI Ở ĐÂY ──
        // Ép game trở về chế độ chơi thường trước khi load Scene
        GameModeManager.SetNormalMode();

        SceneManager.LoadScene(SCENE_GAMEPLAY);
    }

    /// <summary>
    /// Chuyển về màn menu
    /// Gắn vào Btn_Home trong Panel_GameOver
    /// </summary>
    public void GoToMenu()
    {
        SceneManager.LoadScene(SCENE_MENU);
    }

    public void GoToStore()
    {
        SceneManager.LoadScene(SCENE_STORE);
    }

    public void GoToChallenge()
    {
        SceneManager.LoadScene(SCENE_CHALLENGE);
    }

    public void OpenShopKnife()
    {
        PlayerPrefs.SetInt("OpenShopKnife", 1);
        SceneManager.LoadScene(SCENE_MENU);
    }
}