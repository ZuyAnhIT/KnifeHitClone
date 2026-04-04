using UnityEngine;
using System.Collections;
using System.IO;
using System.Diagnostics;

public class ShareButton : MonoBehaviour
{
    public void CaptureScreenshot()
    {
        StartCoroutine(Capture());
    }

    private IEnumerator Capture()
    {
        yield return new WaitForEndOfFrame();

        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        tex.Apply();

        string folder = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            "KnifeHit_Screenshots"
        );

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, "screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");

        File.WriteAllBytes(path, tex.EncodeToPNG());

        UnityEngine.Debug.Log("Saved: " + path);

        Destroy(tex);

        // Mở thư mục chứa ảnh
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        Process.Start("explorer.exe", folder);
#endif
    }
}