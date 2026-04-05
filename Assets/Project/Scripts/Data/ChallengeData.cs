using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Challenge_New", menuName = "KnifeHit/Challenge Data")]
public class ChallengeData : ScriptableObject
{
    [Header("── Thông tin chung ──")]
    public string challengeName = "Monsters";
    public int rewardApples = 100; // Phần thưởng khi hoàn thành toàn bộ thử thách

    [Tooltip("Màu chủ đạo cho HUD (Chấm tròn, Dao boss, Chuyển cảnh)")]
    public Color themeColor = new Color(1f, 0.7f, 0f, 1f); // Mặc định là màu Vàng/Cam

    [Header("── Giao diện Nền ──")]
    public Sprite backgroundSprite; // Ảnh nền phía sau của riêng Challenge này

    [Tooltip("Kéo 3 mảnh vỡ (1/2, 1/3, 1/4) của gỗ màn này vào đây")]
    public Sprite[] normalBrokenSprites;

    [Header("── Màn Thường (Normal Stage) ──")]
    public Sprite normalLogSprite;                 // Ảnh mục tiêu thường (Ví dụ: Quái vật nhỏ)
    public Color normalExplodeColor = Color.white; // Màu hạt vỡ khi mục tiêu thường nổ tung
    public AudioClip[] hitSounds;                  // Âm thanh khi đâm dao trúng mục tiêu thường

    [Header("── Màn Boss (Boss Stage) ──")]
    [Tooltip("Danh sách Boss sẽ xuất hiện (Vòng 1 đánh Boss ở index 0, Vòng 2 đánh index 1...)")]
    public List<BossLogData> challengeBosses;

    [Header("── Độ khó (Tùy chọn) ──")]
    [Tooltip("Danh sách cấu hình tốc độ, số dao... riêng biệt cho thử thách này")]
    public List<LevelData> challengeStages;

    [Tooltip("Icon đại diện cố định (Hiện trên Banner Game Over/Win)")]
    public Sprite challengeBadgeIcon;
}