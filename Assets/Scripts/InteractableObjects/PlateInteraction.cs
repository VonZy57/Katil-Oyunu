using DG.Tweening;
using UnityEngine;

public class PlateInteraction : FoodInteractable
{
    [Header("Referanslar")]
    [Tooltip("Yutkunma animasyonu için oyuncu camerasına uygulanacak animasyon.")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject player;
    protected override void DoEventsWhileEating()
    {
        //Basit yutma animasyonu
        if (playerCamera != null)
        {
            player.GetComponent<FirstPersonController>().enabled = false;

            // DOTween Sequence oluştur (Animasyonları sıraya dizmek için)
            Sequence eatSequence = DOTween.Sequence();

            // 1. Kamera 45 derece yukarı kalksın (1 saniyede)
            eatSequence.Append(playerCamera.transform.DORotate(new Vector3(-45f, 0f, 0f), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));

            // 2. Çiğneme efekti (5 saniye boyunca)
            // DOPunchRotation kameranın X ekseninde ufak ritmik vuruşlar yapmasını sağlar (kafa sallama/çiğneme hissi).
            // 0.5 saniyelik bir sarsıntıyı 10 kere tekrar ederek toplam 5 saniye boyunca yukarıda çiğniyor gibi görünür.
            eatSequence.Append(playerCamera.transform.DOPunchRotation(new Vector3(3f, 0f, 0f), 0.5f, 1, 0.5f).SetLoops(10));

            // 3. Kamera orijinal pozisyonuna geri dönsün (+45 derece ekleyerek çıktığımız kadar iniyoruz)
            eatSequence.Append(playerCamera.transform.DORotate(new Vector3(45f, 0f, 0f), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));

            // 4. Tüm süreç bittiğinde kontrolleri tekrar oyuncuya ver
            eatSequence.OnComplete(() => 
            {
                player.GetComponent<FirstPersonController>().enabled = true;
            });
        }
            
    }
}
