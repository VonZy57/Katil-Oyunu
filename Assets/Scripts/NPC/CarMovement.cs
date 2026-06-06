using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineAnimate))]
public class CarMovement : MonoBehaviour
{
    [Header("Referanslar")]
    public SplineContainer carPathSpline;
    public Transform[] wheels; // Tekerleklerin merkezini temsil eden boş objeler atandı
    
    [Header("Ayarlar")]
    public float carSpeed = 5f;
    public float wheelRotationSpeed = 360f; // Saniyede kaç derece döneceği

    private SplineAnimate splineAnimate;

    void Start()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        splineAnimate.enabled = false;
    }

    void Update()
    {
        // Spline üzerinde araba hareket ediyorsa tekerlekleri de döndür
        if (splineAnimate != null && splineAnimate.IsPlaying)
        {
            RotateWheels();
        }
    }

    public void StartCarMovement()
    {
        if (carPathSpline == null) return;

        splineAnimate.Container = carPathSpline;
        splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
        splineAnimate.MaxSpeed = carSpeed;
        splineAnimate.Loop = SplineAnimate.LoopMode.Once;
        splineAnimate.Alignment = SplineAnimate.AlignmentMode.SplineElement; // Arabanın yönünü Spline'a uydur (burnu yola dönsün)

        splineAnimate.enabled = true;
        splineAnimate.Restart(true);
    }

    private void RotateWheels()
    {
        foreach (Transform wheel in wheels)
        {
            if (wheel != null)
            {
                // Kendi local ekseninde Z'de negatif yönde döndür (Space.Self ile Local dönüş sağlanır)
                wheel.Rotate(0f, -wheelRotationSpeed * Time.deltaTime, 0f, Space.Self);
            }
        }
    }
}
