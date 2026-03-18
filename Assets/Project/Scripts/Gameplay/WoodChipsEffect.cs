using UnityEngine;

public class WoodChipsEffect : MonoBehaviour
{
    private ParticleSystem _ps;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Gọi với vị trí TIẾP ĐIỂM (mép Log)
    /// Không phải tâm Log, không phải đầu dao
    /// </summary>
    public void Play(Vector3 contactPoint)
    {
        // Đặt particle tại điểm tiếp xúc chính xác
        transform.position = contactPoint;

        _ps.Stop(true,
            ParticleSystemStopBehavior.StopEmittingAndClear);
        _ps.Play();
    }
}