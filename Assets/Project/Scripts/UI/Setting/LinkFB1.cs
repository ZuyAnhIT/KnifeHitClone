using UnityEngine;

public class LinkFB1 : MonoBehaviour
{
    [Header("Cài đặt Link")]
    [Tooltip("Nhập đường link Facebook của bạn vào đây")]
    public string facebookUrl = "https://www.facebook.com/TenTrangCuaBan";

    // Hàm này sẽ được gọi khi nhấn nút
    public void OpenFacebookLink()
    {
        Application.OpenURL(facebookUrl);
        Debug.Log("Đang mở link: " + facebookUrl);
    }
}