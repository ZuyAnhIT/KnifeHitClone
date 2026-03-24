using UnityEngine;
using UnityEngine.UI;

public class ToggleSwitch : MonoBehaviour
{
    [Header("Giao diện trạng thái")]
    public GameObject stateON;  // Kéo object State_ON vào đây
    public GameObject stateOFF; // Kéo object State_OFF vào đây

    [Header("Trạng thái mặc định")]
    public bool isON = true;

    private Button myButton;

    void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnToggleClicked);

        // Cập nhật giao diện lúc vừa vào game
        UpdateVisuals();
    }

    public void OnToggleClicked()
    {
        // Lật ngược trạng thái (Đang True thì thành False, đang False thì thành True)
        isON = !isON;

        UpdateVisuals();

        // TODO: Chỗ này sau này chúng ta sẽ viết code lưu vào PlayerPrefs (tắt âm, tắt rung...)
    }

    private void UpdateVisuals()
    {
        // Bật/Tắt các object theo biến isON
        if (stateON != null) stateON.SetActive(isON);
        if (stateOFF != null) stateOFF.SetActive(!isON);
    }
}