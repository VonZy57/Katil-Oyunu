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
        //Basit çiğneme animasyonu
        if (playerCamera != null)
        {
            player.GetComponent<FirstPersonController>().enabled = false;

            // DOTween Sequence oluştur (Animasyonları sıraya dizmek için)
            Sequence eatSequence = DOTween.Sequence();

            // 1. Kamera 15 derece yukarı kalksın (1 saniyede)
            eatSequence.Append(playerCamera.transform.DORotate(new Vector3(-15f, 0f, 0f), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));

            // 2. Çiğneme efekti (4 saniye boyunca)
            // DOPunchRotation kameranın X ekseninde ufak ritmik vuruşlar yapmasını sağlar (kafa sallama/çiğneme hissi).
            // 1 saniyelik bir sarsıntıyı 4 kere tekrar ederek toplam 4 saniye boyunca yukarıda çiğniyor gibi görünür.
            eatSequence.Append(playerCamera.transform.DOPunchRotation(new Vector3(3f, 0f, 1f), 1f, 1, 0.5f).SetLoops(4));

            // 3. Kamera orijinal pozisyonuna geri dönsün (+15 derece ekleyerek çıktığımız kadar iniyoruz)
            //eatSequence.Append(playerCamera.transform.DORotate(new Vector3(15f, 0f, 0f), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.InOutQuad));

            // 4. Tüm süreç bittiğinde kontrolleri tekrar oyuncuya ver
            eatSequence.OnComplete(() => 
            {
                FirstPersonController fpsController = player.GetComponent<FirstPersonController>();
                fpsController.AddXRotation(-15f); // Animasyonda verdiğimiz -15 derecelik açıyı Controller'a da bildiriyoruz
                fpsController.enabled = true;
            });
        }
            
    }
}
