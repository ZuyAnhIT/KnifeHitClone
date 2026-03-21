using UnityEngine;

public class AppleBreakEffect : MonoBehaviour
{
    [Header("── 2 Mảnh táo ──")]
    [SerializeField] private GameObject appleHalfLeft;
    [SerializeField] private GameObject appleHalfRight;

    public void PlayBreak(Vector3 position)
    {
        SpawnHalves(position);
    }

    private void SpawnHalves(Vector3 pos)
    {
        // ── Mảnh trái — Bay lên trái ──
        if (appleHalfLeft != null)
        {
            GameObject left = Instantiate(
                appleHalfLeft,
                pos,
                Quaternion.Euler(0f, 0f,
                    Random.Range(-20f, 20f))
            );

            AppleHalf ah = left.GetComponent<AppleHalf>();
            if (ah != null)
                ah.Launch(new Vector2(
                    Random.Range(-3f, -1.5f), // Sang trái
                    Random.Range(3f, 5f)      // Bay lên
                ));
        }

        // ── Mảnh phải — Bay lên phải ──
        if (appleHalfRight != null)
        {
            GameObject right = Instantiate(
                appleHalfRight,
                pos,
                Quaternion.Euler(0f, 0f,
                    Random.Range(-20f, 20f))
            );

            AppleHalf ah = right.GetComponent<AppleHalf>();
            if (ah != null)
                ah.Launch(new Vector2(
                    Random.Range(1.5f, 3f),  // Sang phải
                    Random.Range(3f, 5f)     // Bay lên
                ));
        }
    }
}