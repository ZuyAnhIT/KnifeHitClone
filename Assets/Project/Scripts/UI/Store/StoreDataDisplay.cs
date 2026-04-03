using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoreDataDisplay : MonoBehaviour
{
    [Header("── Text References ──")]
    [SerializeField] private TextMeshProUGUI txtApple;

    private void Start()
    {
        RefreshDisplay();
    }
  
    public void RefreshDisplay()
    {
        if (SaveManager.Instance == null) return;
        // Tổng táo 
        if (txtApple != null)
            txtApple.text = SaveManager.Instance
                                .TotalApple.ToString();
    }
}
