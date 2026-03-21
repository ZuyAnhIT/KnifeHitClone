using UnityEngine;

/// <summary>
/// Dữ liệu 1 loại log Boss
/// </summary>
[CreateAssetMenu(
    fileName = "BossLog_New",
    menuName = "KnifeHit/BossLog")]
public class BossLogData : ScriptableObject
{
    [Header("── Thông tin ──")]
    public string bossName = "LEMON";  // Tên hiển thị
    public Sprite logSprite;            // Sprite log boss

    [Header("── Độ khó ──")]
    public float logSpeed = 160f;
    public bool canReverse = true;
    public float reverseInterval = 1.2f;
}