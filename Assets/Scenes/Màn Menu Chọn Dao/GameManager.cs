using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Tạo Singleton để dễ dàng gọi GameManager từ mọi nơi
    public static GameManager Instance { get; private set; }

    [Header("Dữ liệu Trò chơi")]
    // Biến lưu trữ ảnh con dao mà người chơi đang sử dụng (Chỉ lưu dao ĐÃ SỞ HỮU)
    public Sprite currentSelectedKnifeSprite;

    // Sự kiện để báo cho các màn hình khác biết con dao vừa được thay đổi
    public delegate void OnKnifeChanged(Sprite newKnifeSprite);
    public event OnKnifeChanged KnifeChangedEvent;

    private void Awake()
    {
        // Thiết lập Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(this.gameObject); // Mở dòng này nếu bạn có nhiều Scene
        }
    }

    // Hàm để cập nhật con dao mới vào bộ não
    public void SetCurrentKnife(Sprite knifeSprite)
    {
        currentSelectedKnifeSprite = knifeSprite;

        // Notify any listeners (Ví dụ: Screen_MainMenu)
        KnifeChangedEvent?.Invoke(knifeSprite);
    }
}