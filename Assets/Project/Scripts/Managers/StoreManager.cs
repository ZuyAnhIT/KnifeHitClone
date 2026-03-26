using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoreManager : MonoBehaviour
{
    public void BackMenu( )
    {
           SceneManager.LoadScene("SC_MenuGame");
    }
}
