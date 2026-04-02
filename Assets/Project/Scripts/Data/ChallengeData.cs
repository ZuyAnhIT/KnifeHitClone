using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Challenge_New", menuName = "KnifeHit/Challenge Data")]
public class ChallengeData : ScriptableObject
{
    [Header("── Thông tin chung ──")]
    public string challengeName = "Monsters";
    public int rewardApples = 100; // Phần thưởng khi hoàn thành toàn bộ thử thách

    [Header("── Giao diện (Lớp áo) ──")]
    public Sprite backgroundSprite; // Ảnh nền phía sau
    public Sprite logSprite;        // Ảnh mục tiêu (thay cho khúc gỗ)
    public Color explodeColor = Color.white; // Màu khi mục tiêu vỡ tung

    [Header("── Âm thanh ──")]
    public AudioClip[] hitSounds;   // Mảng âm thanh khi dao cắm vào mục tiêu

    [Header("── Độ khó ──")]
    [Tooltip("Danh sách các màn chơi nhỏ trong thử thách này")]
    // Bạn có thể tận dụng luôn LevelData cũ để cấu hình độ khó, số dao, táo... cho từng stage của Challenge
    public List<LevelData> challengeStages;
}