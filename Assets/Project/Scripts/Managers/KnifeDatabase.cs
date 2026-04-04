using UnityEngine;

/// <summary>
/// ScriptableObject làm "bảng tra cứu" sprite dao.
/// Tạo 1 lần duy nhất: Assets > Create > KnifeHit > KnifeDatabase
/// Sau đó kéo từng coloredSprite của mỗi KnifeSlotUI vào mảng knifeSprites
/// theo đúng thứ tự: Page0_Slot0, Page0_Slot1, ..., Page1_Slot0, ...
///
/// Công thức index: pageIndex * slotsPerPage + slotIndex
/// </summary>
[CreateAssetMenu(fileName = "KnifeDatabase", menuName = "KnifeHit/KnifeDatabase")]
public class KnifeDatabase : ScriptableObject
{
    [Tooltip("Số ô dao trên mỗi trang (của bạn là 25)")]
    public int slotsPerPage = 25;

    [Tooltip("Kéo coloredSprite của từng KnifeSlotUI vào đây theo thứ tự Page→Slot")]
    public Sprite[] knifeSprites;

    /// <summary>
    /// Lấy sprite theo pageIndex và slotIndex.
    /// Trả về sprite đầu tiên nếu index vượt quá.
    /// </summary>
    public Sprite GetSprite(int pageIndex, int slotIndex)
    {
        if (knifeSprites == null || knifeSprites.Length == 0)
        {
            Debug.LogWarning("KnifeDatabase: Chưa có sprite nào được gán!");
            return null;
        }

        int globalIndex = pageIndex * slotsPerPage + slotIndex;

        if (globalIndex < 0 || globalIndex >= knifeSprites.Length)
        {
            Debug.LogWarning($"KnifeDatabase: Index {globalIndex} vượt quá mảng ({knifeSprites.Length}). Dùng sprite đầu tiên.");
            return knifeSprites[0];
        }

        return knifeSprites[globalIndex];
    }
}