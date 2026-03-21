using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    // Kéo cái ô Image hiển thị con dao trên Main Menu vào đây
    public Image currentKnifeImage;

    private void Start()
    {
        // 1. Đăng ký lắng nghe sự kiện từ GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.KnifeChangedEvent += UpdateMainKnifeImage;

            // 2. Thiết lập hình ảnh ban đầu (Nếu GameManager đã có sẵn dao)
            if (GameManager.Instance.currentSelectedKnifeSprite != null)
            {
                UpdateMainKnifeImage(GameManager.Instance.currentSelectedKnifeSprite);
            }
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký để tránh lỗi bộ nhớ
        if (GameManager.Instance != null)
        {
            GameManager.Instance.KnifeChangedEvent -= UpdateMainKnifeImage;
        }
    }

    // Hàm nhận Sprite mới và cập nhật vào Image
    private void UpdateMainKnifeImage(Sprite newKnifeSprite)
    {
        if (currentKnifeImage != null && newKnifeSprite != null)
        {
            currentKnifeImage.sprite = newKnifeSprite;
            // currentKnifeImage.SetNativeSize(); // Nếu cần
        }
    }
}