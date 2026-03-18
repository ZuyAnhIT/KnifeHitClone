using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuPagination : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    [Header("Thành phần UI")]
    public ScrollRect scrollRect;
    public Image[] dots; // Khai báo mảng chứa các dấu chấm

    [Header("Cài đặt Màu sắc")]
    public Color activeColor = Color.white;   // Màu khi đang ở trang đó
    public Color inactiveColor = Color.gray;  // Màu khi không ở trang đó

    [Header("Cài đặt Vuốt")]
    public float snapSpeed = 10f;

    private float[] pagePositions;
    private int currentPage = 0;
    private bool isDragging;
    private float targetPosition;

    void Start()
    {
        int pages = dots.Length;
        pagePositions = new float[pages];

        // Tính tọa độ của các trang (Ví dụ 2 trang thì trang 1 là 0, trang 2 là 1)
        for (int i = 0; i < pages; i++)
        {
            if (pages <= 1) pagePositions[i] = 0;
            else pagePositions[i] = (float)i / (pages - 1);
        }

        targetPosition = pagePositions[0];
        UpdateDots(0);
    }

    void Update()
    {
        // Tự động trượt mượt mà (Snap) hít vào trang gần nhất khi thả tay
        if (!isDragging)
        {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, targetPosition, Time.deltaTime * snapSpeed);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        float currentPos = scrollRect.horizontalNormalizedPosition;
        float minDistance = float.MaxValue;

        // Tính toán xem ngón tay đang thả ra ở gần trang nào nhất
        for (int i = 0; i < pagePositions.Length; i++)
        {
            float dist = Mathf.Abs(currentPos - pagePositions[i]);
            if (dist < minDistance)
            {
                minDistance = dist;
                currentPage = i;
            }
        }

        targetPosition = pagePositions[currentPage];
        UpdateDots(currentPage);
    }

    void UpdateDots(int activeIndex)
    {
        // Đổi màu các dấu chấm
        for (int i = 0; i < dots.Length; i++)
        {
            dots[i].color = (i == activeIndex) ? activeColor : inactiveColor;
        }
    }
}